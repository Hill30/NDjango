(****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Parser.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************)


namespace NDjango.Tags

open System.IO

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.ParserNodes
open NDjango.ASTNodes
open NDjango.OutputHandling
open NDjango.Expressions

module internal LoaderTags =

    /// Define a block that can be overridden by child templates.
    type BlockTag() =
        interface ITag with
            member this.Perform token provider tokens =
                match token.Args with 
                | name::[] -> 
                    let node_list, remaining = (provider :?> IParser).Parse (Some token) tokens ["endblock"; "endblock " + name]
                    (new BlockNode(provider, token, name, node_list) :> INodeImpl), remaining
                | _ -> raise (SyntaxError ("block tag takes only one argument"))

    /// Signal that this template extends a parent template.
    /// 
    /// This tag may be used in two ways: ``{% extends "base" %}`` (with quotes)
    /// uses the literal value "base" as the name of the parent template to extend,
    /// or ``{% extends variable %}`` uses the value of ``variable`` as either the
    /// name of the parent template to extend (if it evaluates to a string) or as
    /// the parent tempate itelf (if it evaluates to a Template object).
    type ExtendsTag() =
        interface ITag with
            member this.Perform token provider tokens = 
                match token.Args with
                | parent::[] -> 
                    let node_list, remaining = (provider :?> IParser).Parse (Some token) tokens []
                    
                    let parent_name_expr = 
                        new FilterExpression(provider, Block token, parent)
                        
                    (new ExtendsNode(provider, token, node_list, parent_name_expr) :> INodeImpl), LazyList.empty<Token>()
                | _ -> raise (SyntaxError ("extends tag takes only one argument"))

    /// Loads a template and renders it with the current context. This is a way of "including" other templates within a template.
    ///
    /// The template name can either be a variable or a hard-coded (quoted) string, in either single or double quotes.
    ///
    /// This example includes the contents of the template "foo/bar.html":
    ///
    /// {% include "foo/bar.html" %}
    /// This example includes the contents of the template whose name is contained in the variable template_name:
    ///
    /// {% include template_name %}
    /// An included template is rendered with the context of the template that's including it. This example produces the output "Hello, John":
    ///
    /// Context: variable person is set to "john".
    ///
    /// Template:
    ///
    /// {% include "name_snippet.html" %}
    /// The name_snippet.html template:
    ///
    /// Hello, {{ person }}
    /// See also: {% ssi %}.

    type IncludeTag() =

        interface ITag with
            member this.Perform token provider tokens = 
                match token.Args with
                | name::[] -> 
                    let template_name = 
                        new FilterExpression(provider, Block token, name)
                    ({
                        //todo: we're not producing a node list here. may have to revisit
                        new TagNode(provider, token) 
                        with
                            override this.walk manager walker = 
                                {walker with parent=Some walker; nodes=(get_template manager template_name walker.context).Nodes}
                    } :> INodeImpl), tokens
                | _ -> raise (SyntaxError ("'include' tag takes only one argument"))

/// ssi¶
/// Output the contents of a given file into the page.
/// 
/// Like a simple "include" tag, {% ssi %} includes the contents of another file -- which must be specified using an absolute path -- in the current page:
/// 
/// {% ssi /home/html/ljworld.com/includes/right_generic.html %}
/// If the optional "parsed" parameter is given, the contents of the included file are evaluated as template code, within the current context:
/// 
/// {% ssi /home/html/ljworld.com/includes/right_generic.html parsed %}
/// Note that if you use {% ssi %}, you'll need to define ALLOWED_INCLUDE_ROOTS in your Django settings, as a security measure.
/// 
/// See also: {% include %}.

    type Reader = Path of string | TextReader of System.IO.TextReader

    type SsiNode(provider, token, reader: Reader, loader: string->TextReader) = 
        inherit TagNode(provider, token)

        override this.walk manager walker =
            let templateReader =  
                match reader with 
                | Path path -> loader path
                | TextReader reader -> reader
            let bufarray = Array.create 4096 ' '
            let length = templateReader.Read(bufarray, 0, bufarray.Length)
            let buffer = Array.sub bufarray 0 length |> Seq.fold (fun status item -> status + string item) "" 
            let nodes = 
                if length = 0 
                then templateReader.Close(); walker.nodes
                else (new SsiNode(provider, token, TextReader templateReader, loader) :> INodeImpl) :: walker.nodes
            {walker with buffer = buffer; nodes=nodes}

    type SsiTag() =

        interface ITag with
            member this.Perform token provider tokens = 
                match token.Args with
                | path::[] -> (new SsiNode(provider, token, Path path, provider.Loader.GetTemplate) :> INodeImpl), tokens
                | path::"parsed"::[] ->
                    let templateRef = FilterExpression (provider, Block token, "\"" + path + "\"")
                    ({
                        new TagNode(provider, token) 
                        with
                            override this.walk manager walker = 
                                {walker with parent=Some walker; nodes=(get_template manager templateRef walker.context).Nodes}
                    } :> INodeImpl), tokens
                | _ ->
                    raise (SyntaxError ("malformed 'ssi' tag"))
                