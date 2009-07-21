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


namespace NDjango
open System.Text
open System.Text.RegularExpressions
open System.Collections
open System.Collections.Generic
open System.Reflection

open Interfaces
open OutputHandling
open Utilities

module Expressions =

    /// Finds and invokes the first property, field or 0-parameter method in the list.
    let rec private find_and_invoke_member (members: MemberInfo list) current bit =
        match members with 
        | h::t ->
            match h with
            | :? MethodInfo as mtd -> 
                // only call methods that don't have any parameters
                match mtd.GetParameters().Length with 
                | 0 -> Some <| mtd.Invoke(current, null)
                | _ -> find_and_invoke_member t current bit
            | :? FieldInfo as fld -> Some <| fld.GetValue(current)
            | :? PropertyInfo as prop -> 
                match prop.GetIndexParameters().Length with
                | 0 -> Some <| prop.GetValue(current, null)     // non-indexed property
                | 1 -> Some <| prop.GetValue(current, [|bit|])  // indexed property
                | _ -> None                                     // indexed property with more indeces that we can handle
            | _ -> failwith <| sprintf "%A is unexpected." current // this shouldn't happen, as all other types would be filtered out
        | [] -> None
    
    /// recurses through the supplied list and attempts to navigate the "current" object graph using
    /// name elements provided by the list. This function will invoke method and evaluate properties and members
    let rec internal resolve_lookup (current: obj) = function
        | h::t ->
            // tries to invoke a member in the members list, calling f if find_and_invoke_member returned None
            let try_invoke = fun (members: array<MemberInfo>) bit (f: unit -> obj option) ->
                match find_and_invoke_member (List.of_array members) current bit with
                | Some v -> Some v
                | None -> f()
                
            let find_intermediate = fun bit (current: obj) ->        
                match current with 
                | :? IDictionary as dict ->
                    match dict with
                    | Utilities.Contains (Some bit) v -> Some v
                    | Utilities.Contains (Some (String.concat System.String.Empty [OutputHandling.django_ns; bit])) v -> Some v
                    | Utilities.Contains (Utilities.try_int bit) v -> Some v
                    | _ ->
                        let (dict_members: array<MemberInfo>) = current.GetType().GetMember(bit, MemberTypes.Field ||| MemberTypes.Method ||| MemberTypes.Property, BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance)
                        find_and_invoke_member (List.of_array dict_members) current bit
                | :? IList as list ->
                    match bit with 
                    | Utilities.Int i when list.Count > i -> Some list.[i]
                    | _ -> None
                | null -> None
                | _ ->
                    let (members: array<MemberInfo>) = current.GetType().GetMember(bit, MemberTypes.Field ||| MemberTypes.Method ||| MemberTypes.Property, BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance)
                    let indexed_members = lazy ( current.GetType().GetMember("Item", MemberTypes.Property, BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance))
                    let as_array = fun () -> 
                        try 
                            // bit is an index into an array
                            if (Utilities.is_int bit) && current.GetType().IsArray && Array.length (current :?> array<obj>) > (int bit) then
                                Some <| Array.get (current :?> array<obj>) (int bit)
                            // no clue
                            else
                                None
                        // any number of issues as the result trying to parse an unknown value
                        with | _ -> None
                                           
                    // this bit of trickery needs explanation. try_invoke tries to find and invoke a member in the
                    // members array. if it doesn't, it will call the supplied function f. here, we're chaining together
                    // a search across all members, followed by a search for indexed members. the second search is 
                    // supplied as the "f" to the first search. also, indexed members are defined as lazy so that we
                    // don't take the reflection hit if we dont' need it
                    try_invoke members bit (fun () -> try_invoke (indexed_members.Force()) bit (fun () -> as_array()))

            if not (t = []) then
                match find_intermediate h current with
                | Some v -> resolve_lookup v t
                | None -> None
            else
                find_intermediate h current
        | [] ->  None
            
    /// A template variable, resolvable against a given context. The variable may be
    /// a hard-coded string (if it begins and ends with single or double quote
    /// marks)::
    /// 
    ///     >>> c = {'article': {'section':u'News'}}
    ///     >>> Variable('article.section').resolve(c)
    ///     u'News'
    ///     >>> Variable('article').resolve(c)
    ///     {'section': u'News'}
    ///     >>> class AClass: pass
    ///     >>> c = AClass()
    ///     >>> c.article = AClass()
    ///     >>> c.article.section = u'News'
    ///     >>> Variable('article.section').resolve(c)
    ///     u'News'
    /// (The example assumes VARIABLE_ATTRIBUTE_SEPARATOR is '.')
    type Variable(parser : IParser, token:Lexer.Token, variable:string) =
        let (|ComponentStartsWith|_|) chr (text: string) =
            if text.StartsWith(chr) || text.Contains(Constants.VARIABLE_ATTRIBUTE_SEPARATOR + chr) then
                Some chr
            else
                None
        
        let fail_syntax v = 
            raise (
                TemplateSyntaxError (
                    sprintf "Variables and attributes may not be empty, begin with underscores or minus (-) signs: '%s', '%s'" variable v
                    , Lexer.get_textToken token))

        do match variable with 
            | ComponentStartsWith "-" v->
                match variable with
                | Int i -> () 
                | Float f -> ()
                | _ -> fail_syntax v
            | ComponentStartsWith "_" v when not <| variable.StartsWith(Constants.I18N_OPEN) ->
                fail_syntax v
            | _ -> () // need this to show the compiler that all cases are covered. 

        /// Returns a tuple of (var * value * needs translation)
        let find_literal = function
            | Utilities.Int i -> (None, Some (i :> obj), false)
            | Utilities.Float f -> (None, Some (f :> obj), false)
            | _ as v ->
                let var, is_literal, translate = OutputHandling.strip_markers v

                ((if not is_literal then Some var else None), (if is_literal then Some (var :> obj) else None), translate)

        let var, literal, translate = find_literal variable
        
        let lookups = if var.IsSome then Some <| List.of_array (var.Value.Split(Constants.VARIABLE_ATTRIBUTE_SEPARATOR.ToCharArray())) else None
        
        let clean_nulls  = function
        | Some v as orig -> if v = null then None else orig
        | None -> None

        /// Resolves this variable against a given context
        member this.Resolve (context: IContext) =
            match lookups with
            | None -> (literal.Value, false)
            | Some lkp ->
                let result =
                    match 
                        match context.tryfind (List.hd <| lkp) with
                        | Some v -> 
                            match lkp |> List.tl with
                            | h::t -> 
                                // make sure we don't end up with a 'Some null'
                                resolve_lookup v (h::t) |> clean_nulls
                            | _ -> Some v |> clean_nulls
                        | None -> None
                        with
                        | Some v1 -> v1
                        | None -> context.TEMPLATE_STRING_IF_INVALID
//                        | None -> None :> obj
                (result, context.Autoescape)
        
        member this.ExpressionText with get() = variable

    // TODO: we still need to figure out the translation piece
    // python code
    //        if self.translate:
    //            return _(value)

    /// Helper class for parsing variable blocks
    /// Parses a variable token and its optional filters (all as a single string),
    /// and return a list of tuples of the filter name and arguments.
    /// Sample:
    ///     >>> token = 'variable|default:\"Default value\"|date:\"Y-m-d\"'
    ///     >>> p = Parser('')
    ///     >>> fe = FilterExpression(token, p)
    ///     >>> len(fe.filters)
    ///     2
    ///     >>> fe.var
    ///     <Variable: 'variable'>
    ///
    /// This class should never be instantiated outside of the
    /// get_filters_from_token helper function. *)
    type FilterExpression (manager: IParser, token:Lexer.Token, expression: string) =
        
        /// unescapes literal quotes. takes '\"value\"' and returns '"value"'
        let flatten (text: string) = text.Replace("\\\"", "\"")
        
        /// unescapes literal quotes. takes '\"value\"' and returns '"value"'
        let flatten_group (group: Group) = flatten group.Value

        /// validates that the argument supplied to the filter is sufficient per the filter definition
        let args_check name (filter: ISimpleFilter) provided = 
            match filter with 
            | :? IFilter as f->
                if List.length provided = 0 && f.DefaultValue = null then
                    raise (TemplateSyntaxError (sprintf "%s requires argument, none provided" name, Lexer.get_textToken token ))
                else true
            | _ -> true
        
        /// Parses a variable definition
        let rec parse_var (filter_match: Match) upto (var: Option<string>) =
            if not (filter_match.Success) then
                if not (upto = expression.Length) 
                then raise (TemplateSyntaxError (sprintf "Could not parse the remainder: '%s' from '%s'" expression.[upto..] expression
                    , Lexer.get_textToken token))
                else
                    (upto, new Variable(manager, token, var.Value), [])
            else
                // short-hand for the recursive call. the values for match and upto are always computed the same way
                let fast_call = fun v -> parse_var (filter_match.NextMatch()) (filter_match.Index + filter_match.Length) v
            
                if not (upto = filter_match.Index) then
                    raise 
                        (TemplateSyntaxError 
                            (sprintf "Could not parse some characters %s|%s|%s" 
                                expression.[..upto] 
                                expression.[upto..filter_match.Index] 
                                expression.[filter_match.Index..]
                            , Lexer.get_textToken token))
                else
                    match var with
                    | None ->
                        match filter_match with
                        | Utilities.Matched "var" var_match -> fast_call (Some var_match)
                        | _ -> raise (TemplateSyntaxError (sprintf "Could not find variable at start of %s" expression, Lexer.get_textToken token))
                    | Some s ->
                        let filter_name = filter_match.Groups.["filter_name"]
                        let arg = filter_match.Groups.["arg"].Captures |> Seq.cast |> Seq.to_list |> List.map (fun c -> Variable(manager, token, c.ToString())) 
                        let filter = manager.FindFilter filter_name.Value
                        
                        if filter.IsNone then raise (TemplateSyntaxError (sprintf "filter %A could not be found" filter_name.Value
                            , Lexer.get_textToken token))
                        else
                            ignore <| args_check filter_name.Value filter.Value arg

                            let _upto, variable, filters = fast_call var
                            (_upto, variable, [(filter.Value, arg)] @ filters)

        // list of filters along with the arguments that they expect
        let upto, variable, filters = parse_var (Constants.filter_re.Match(expression)) 0 None

        /// recursively evaluates the filter list against the context and input objects.
        /// the tuple returned consists of the value as the first item in the tuple
        /// and a boolean indicating whether the value needs escaping 
        let rec do_resolve (context: IContext) (input: obj option*bool) (filter_list: list<ISimpleFilter * Variable list>) = 
            match fst input with
            | None -> (None, false)
            | Some v ->
                let wrap o = if o = null then None else Some o
                match filter_list with
                | h::t ->
                    match fst h with
                    | :? NDjango.Filters.IEscapeFilter -> (fst (do_resolve context input t), true)
                    | :? IFilter as std -> 
                        // we don't have to check for the presence of a default value here, as an earlier
                        // check enforces that filters without defaults do not get called without parameters
                        let param = 
                            if List.length (snd h) = 0 
                                then std.DefaultValue
                                else fst <| (List.hd <| snd h).Resolve context
                        do_resolve context (wrap (std.PerformWithParam (v, param)), snd input) t
                    | _ as simple -> do_resolve context (wrap (simple.Perform v), snd input) t
                | [] -> input
            
        /// resolves the filter against the given context. if ignoreFailures is true, None is returned for failed expressions.
        member this.Resolve (context: IContext) ignoreFailures =
            let resolved_value = 
                try
                    let result = variable.Resolve context
                    (Some (fst <| result), snd <| result)
                with
                    | _ as exc -> 
                        if ignoreFailures then
                            (None, false)
                        else
                            raise (RenderingError((sprintf "Exception occured while processing variable '%s'" variable.ExpressionText), exc))
            
            do_resolve context resolved_value filters
            
        /// resolves the filter against the given context and 
        /// converts it to string taking into account escaping. 
        /// This method never fails, if the expression fails to resolve, 
        /// the method returns None
        member this.ResolveForOutput walker =
            let result, needsEscape = this.Resolve walker.context false
            match result with 
            | None -> None  // this results in no output from the expression
            | Some o -> 
                match o with 
                | :? Node as node -> Some (node.walk walker) // take output from the node
                | null -> None // this results in no output from the expression
                | _ as v ->
                    match if needsEscape then escape v else string v with
                    | "" -> None
                    | _ as s -> Some {walker with buffer = s}
            

            //TODO: django language spec allows 0 or 1 arguments to be passed to a filter, however the django implementation will handle any number
            //for filter, args in filters do
                
        member this.Token with get() = expression