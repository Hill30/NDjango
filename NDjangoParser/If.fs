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
open System.Collections

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Expressions
open NDjango.ParserNodes

module internal If = 
    /// The ``{% if %}`` tag evaluates a variable, and if that variable is "true"
    /// (i.e., exists, is not empty, and is not a false boolean value), the
    /// contents of the block are output:
    /// 
    /// ::
    /// 
    ///     {% if athlete_list %}
    ///         Number of athletes: {{ athlete_list|count }}
    ///     {% else %}
    ///         No athletes.
    ///     {% endif %}
    /// 
    /// In the above, if ``athlete_list`` is not empty, the number of athletes will
    /// be displayed by the ``{{ athlete_list|count }}`` variable.
    /// 
    /// As you can see, the ``if`` tag can take an option ``{% else %}`` clause
    /// that will be displayed if the test fails.
    /// 
    /// ``if`` tags may use ``or``, ``and`` or ``not`` to test a number of
    /// variables or to negate a given variable::
    /// 
    ///     {% if not athlete_list %}
    ///         There are no athletes.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list or coach_list %}
    ///         There are some athletes or some coaches.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list and coach_list %}
    ///         Both atheletes and coaches are available.
    ///     {% endif %}
    /// 
    ///     {% if not athlete_list or coach_list %}
    ///         There are no athletes, or there are some coaches.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list and not coach_list %}
    ///         There are some athletes and absolutely no coaches.
    ///     {% endif %}
    /// 
    /// ``if`` tags do not allow ``and`` and ``or`` clauses with the same tag,
    /// because the order of logic would be ambigous. For example, this is
    /// invalid::
    /// 
    ///     {% if athlete_list and coach_list or cheerleader_list %}
    /// 
    /// If you need to combine ``and`` and ``or`` to do advanced logic, just use
    /// nested if tags. For example::
    /// 
    ///     {% if athlete_list %}
    ///         {% if coach_list or cheerleader_list %}
    ///             We have athletes, and either coaches or cheerleaders!
    ///         {% endif %}
    ///     {% endif %}


    type IfLinkType = 
        | And
        | Or

    /// AST TagNode representing an entire if tag, with all nested and composing tags
    type TagNode(
                provider,
                token,
                bool_vars: (bool * FilterExpression) list, 
                node_list_true: NDjango.Interfaces.INodeImpl list, 
                node_list_false: NDjango.Interfaces.INodeImpl list, 
                link_type: IfLinkType
                ) =
        inherit NDjango.ParserNodes.TagNode(provider, token)
        
        /// Evaluates a single filter expression against the context. Results are intepreted as follows: 
        /// None: false (or invalid values, as FilterExpression.Resolve is called with ignoreFailure = true)
        /// Any value of type System.Boolean: actual value
        /// Any IEnumerable: true if at least one element exists, false otherwise
        /// Any other non-null value: true
        let eval context (bool_expr: FilterExpression) = 
            match fst (bool_expr.Resolve context true) with
            | None -> false
            | Some v -> 
                match v with 
                | :? System.Boolean as b -> b                           // boolean value, take literal
                | :? IEnumerable as e -> e.GetEnumerator().MoveNext()   // some sort of collection, take if empty
                | null -> false                                         // null evaluates to false
                | _ -> true                                             // anything else. true because it's there
                    
        /// recursivly evaluates the entire list of expressions and returns the boolean value.
        let rec eval_expression context (expr: (bool * FilterExpression) list) =
            match expr with 
            | h::t ->
                let ifnot, bool_expr = h
                let e = eval context bool_expr
                match link_type with
                | Or ->
                    if (e && not ifnot) || (ifnot && not e) then
                        true
                    else
                        eval_expression context t
                | And ->
                    if not((e && not ifnot) || (ifnot && not e)) then
                        false
                    else
                        eval_expression context t
            | [] -> 
                    match link_type with 
                    | Or -> false
                    | And -> true
                
        override x.walk manager walker =
            match eval_expression walker.context bool_vars with
            | true -> {walker with parent=Some walker; nodes=node_list_true}
            | false -> {walker with parent=Some walker; nodes=node_list_false}
            
        override x.Nodes =
            base.Nodes 
                |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFTRUE) (node_list_true |> Seq.map (fun node -> (node :?> INode)))
                |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFFALSE) (node_list_false |> Seq.map (fun node -> (node :?> INode)))
    
    [<NDjango.ParserNodes.Description("Outputs the content of enclosed tags based on expression evaluation result.")>]
    type Tag() =
        /// builds a list of FilterExpression objects for the variable components of an if statement. 
        /// The tuple returned is (not flag, FilterExpression), where not flag is true when the value
        /// is modified by the "not" keyword, and false otherwise.
        let rec build_vars token notFlag (tokens: TextToken list) parser (vars:(IfLinkType option)*(bool*FilterExpression) list) =
            match tokens with
            | MatchToken("not")::var::tail -> build_vars token true (var::tail) parser vars
            | var::[] -> 
                match fst vars with
                | None -> IfLinkType.Or, [(notFlag, new FilterExpression(parser, var))]
                | Some any -> any, snd vars @ [(notFlag, new FilterExpression(parser, var))]
            | var::MatchToken("and")::var2::tail -> 
                append_vars IfLinkType.And var token notFlag (var2::tail) parser vars 
            | var::MatchToken("or")::var2::tail -> 
                append_vars IfLinkType.Or var token notFlag (var2::tail) parser vars 
            | _ -> raise (SyntaxError ("invalid conditional expression in 'if' tag"))
            
        and append_vars linkType var
            token notFlag (tokens: TextToken list) parser vars =
            match fst vars with
            | Some any when any <> linkType -> raise (SyntaxError ("'if' tags can't mix 'and' and 'or'"))
            | _ -> ()
            build_vars token false tokens parser (Some linkType, snd vars @ [(notFlag, new FilterExpression(parser, var))])
        
        
        interface ITag with 
            member this.Perform token context tokens =

                let node_list_true, remaining = (context.Provider :?> IParser).Parse (Some token) tokens ["else"; "endif"]
                let node_list_false, remaining2 =
                    match node_list_true.[node_list_true.Length-1].Token with
                    | NDjango.Lexer.Block b -> 
                        if b.Verb.RawText = "else" then
                            (context.Provider :?> IParser).Parse (Some token) remaining ["endif"]
                        else
                            [], remaining
                    | _ -> [], remaining
                    
                let link_type, bool_vars = 
                    try
                        build_vars token false token.Args context (None,[])
                    with
                    | :? SyntaxError as e ->
                            raise (SyntaxError(e.Message, 
                                    node_list_true @ node_list_false,
                                    remaining2))
                    |_ -> reraise()
                  
                (({
                    new TagNode(context, token, bool_vars, node_list_true, node_list_false, link_type)
                        with
                            override this.elements
                                with get()=
                                    List.append (bool_vars |> List.map(fun (_, element) -> (element :> INode))) base.elements
                    } :> NDjango.Interfaces.INodeImpl),
                    remaining2)


