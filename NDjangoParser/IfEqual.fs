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

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Expressions
open NDjango.OutputHandling

module internal IfEqual =

    ///    Outputs the contents of the block if the two arguments equal each other.
    ///
    ///    Examples::
    ///
    ///        {% ifequal user.id comment.user_id %}
    ///            ...
    ///        {% endifequal %}
    ///
    ///        {% ifnotequal user.id comment.user_id %}
    ///            ...
    ///        {% else %}
    ///            ...
    ///        {% endifnotequal %}

    type Tag(not:bool) =
        interface ITag with
            member this.Perform token parser tokens =
                let tag = token.Verb
                let node_list_true, remaining = parser.Parse tokens ["else"; "end" + tag]
                let node_list_false, remaining =
                    match node_list_true.[node_list_true.Length-1].Token with
                    | NDjango.Lexer.Block b -> 
                        if b.Verb = "else" then
                            parser.Parse remaining ["end" + tag]
                        else
                            [], remaining
                    | _ -> [], remaining

                let getNodeList v1 v2 = 
                    match v1,v2 with
                    | None,_ | _, None -> node_list_false
                    | Some value1, Some value2 -> 
                        if not = value1.Equals value2 
                            then node_list_true
                            else node_list_false

                match token.Args with
                | var1::var2::[] ->
                    let var1 = new FilterExpression(parser, Block token, var1)
                    let var2 = new FilterExpression(parser, Block token, var2)
                    {
                        new Node(Block token)
                        with 
                            override this.walk walker =
                                {
                                    walker 
                                    with 
                                        parent=Some walker;
                                        nodes=getNodeList (fst (var1.Resolve walker.context true)) (fst (var2.Resolve walker.context true))
                                }
                        
                            override this.nodes with get() = node_list_true @ node_list_false
                    }, remaining
                | _ -> raise (TemplateSyntaxError (sprintf "'%s' takes two arguments" tag, Some (token:>obj)))

                