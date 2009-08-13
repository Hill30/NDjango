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

open System

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.ParserNodes
open NDjango.ASTNodes
open NDjango.OutputHandling
open NDjango.Expressions

module internal Filter =

    let FILTER_VARIABLE_NAME = "$filter"

    type FilterNode(provider, token, filter: FilterExpression, node_list) =
        inherit TagNode(provider, token)

        override this.walk manager walker = 
            let reader = 
                new NDjango.ASTWalker.Reader (manager, {walker with parent=None; nodes=node_list; context=walker.context}) 
            match filter.ResolveForOutput manager
                     {walker with context=walker.context.add(FILTER_VARIABLE_NAME, (reader.ReadToEnd():>obj))}
                with
            | Some w -> w
            | None -> walker

    /// Filters the contents of the block through variable filters.
    /// 
    /// Filters can also be piped through each other, and they can have
    /// arguments -- just like in variable syntax.
    /// 
    /// Sample usage::
    /// 
    ///     {% filter force_escape|lower %}
    ///         This text will be HTML-escaped, and will appear in lowercase.
    ///     {% endfilter %}
    type FilterTag() =
        interface ITag with
            member this.Perform token provider tokens =
                match token.Args with
                | filter::[] ->
                    let filter_expr = new FilterExpression(provider, Block token, FILTER_VARIABLE_NAME + "|" + filter)
                    let node_list, remaining = (provider :?> IParser).Parse (Some token) tokens ["endfilter"]
                    (new FilterNode(provider, token, filter_expr, node_list) :> INodeImpl), remaining
                | _ -> raise (SyntaxError ("'filter' tag requires one argument"))
                
               
