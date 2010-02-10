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
open NDjango.Expressions

module internal LoaderTags =

    type TemplateNameExpression(context:ParsingContext, expression: TextToken) =
        inherit FilterExpression (context, expression)

        interface INode with            
                     
            /// TagNode type = Expression
            member x.NodeType = NodeType.TemplateName


    /// Define a block that can be overridden by child templates.
    [<Description("Defines a block that can be overridden by child templates.")>]
    type BlockTag() =
        interface ITag with
            member this.Perform token context tokens =
                match token.Args with 
                | name::[] -> 
                    let node_list, remaining = (context.Provider :?> IParser).Parse (Some token) tokens ["endblock"; "endblock " + name.RawText]
                    (new BlockNode(context, token, name.RawText, node_list) :> INodeImpl), remaining
                | _ ->
                    let node_list, remaining = (context.Provider :?> IParser).Parse (Some token) tokens ["endblock"]
                    raise (SyntaxError("block tag requires exactly one argument", 
                            node_list,
                            remaining))
                

    /// Signal that this template extends a parent template.
    /// 
    /// This tag may be used in two ways: ``{% extends "base" %}`` (with quotes)
    /// uses the literal value "base" as the name of the parent template to extend,
    /// or ``{% extends variable %}`` uses the value of ``variable`` as either the
    /// name of the parent template to extend (if it evaluates to a string) or as
    /// the parent tempate itelf (if it evaluates to a Template object).
    [<Description("Signals that this template extends a parent template.")>]
    type ExtendsTag() =
        interface ITag with
            member this.Perform token context tokens = 
                let node_list, remaining = (context.Provider :?> IParser).Parse (Some token) tokens []
                match token.Args with
                | parent::[] -> 
                    
                    /// expression yielding the name of the parent template
                    let parent_name_expr = 
                        new TemplateNameExpression(context, parent)
                        
                    /// a list of all blocks in the template starting with the extends tag
                    let node_list = 
                        node_list |> List.choose 
                            (fun node ->
                                match node with
                                /// we need ParsingContextNode in the nodelist for code completion issues
                                | :? ParsingContextNode -> Some node
                                | :? BlockNode -> Some node
                                | :? INode when (node :?> INode).NodeType = NodeType.Text -> Some node
                                | _ -> 
                                    if (context.Provider.Settings.[NDjango.Constants.EXCEPTION_IF_ERROR] :?> bool)
                                    then None
                                    else
                                        Some ({new ErrorNode
                                                (Block(token), 
                                                 new Error(1, "All tags except 'block' tag inside inherited template are ignored"))
                                                 with
                                                    override x.nodelist = [node]
                                                   } :> INodeImpl)
                            )
                    
                    let (nodes : INode list) = List.map(fun (node : INodeImpl) -> node :?> INode) node_list
                    
                    ((new ExtendsNode(context, token, nodes, parent_name_expr) :> INodeImpl), 
                       remaining)
                | _ -> raise (SyntaxError (
                                 "extends tag requires exactly one argument",
                                 node_list,
                                 remaining))

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

    [<Description("Loads and renders a template.")>]
    type IncludeTag() =

        interface ITag with
            member this.Perform token context tokens = 
                match token.Args with
                | name::[] -> 
                    let template_name = 
                        new TemplateNameExpression(context, name)
                    ({
                        //todo: we're not producing a node list here. may have to revisit
                        new TagNode(context, token) 
                        with
                            override this.walk manager walker = 
                                {walker with parent=Some walker; nodes=(get_template manager template_name walker.context).Nodes}
                            override this.elements 
                                with get()=
                                    (template_name :> INode) :: base.elements
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

    [<Description("Outputs the contents of a given file into the page.")>]
    type SsiTag() =

        interface ITag with
            member this.Perform token context tokens = 
                match token.Args with
                | path::[] -> (new SsiNode(context, token, Path path.Value, context.Provider.Loader.GetTemplate) :> INodeImpl), tokens
                | path::MatchToken("parsed")::[] ->
// TODO: ExpressionToken
                    let templateRef = FilterExpression (context, path.WithValue("\"" + path.Value + "\"") (Some [1,false;path.Value.Length,true;1,false]))
                    ({
                        new TagNode(context, token) 
                        with
                            override this.walk manager walker = 
                                {walker with parent=Some walker; nodes=(get_template manager templateRef walker.context).Nodes}
                    } :> INodeImpl), tokens
                | _ ->
                    raise (SyntaxError ("malformed 'ssi' tag"))
                