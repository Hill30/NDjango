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

open NDjango.Interfaces
open OutputHandling
open Utilities
open Lexer

module Variables =
        
    /// <summary>
    /// Tries to resolve current bit as a member of the current object        
    /// </summary>
    /// <remarks>
    /// Goes through a list of all members with the name matching the value of bit
    /// and looks for the first one to return a value. The list of members 
    /// includes fields, parameterless methods and properties
    /// </remarks>
    let private member_resolver bit (current:obj) =
        
        /// determine the type we need to use with the object
        /// in case if the bit references an explicit interface implementation
        /// the type will come in the tuple, otherwise the type of the object itself 
        /// is used
        let object_type, object_instance = 
            match current with
            | :? (System.Type*obj) as a  -> a
            | _ -> (current.GetType(), current)
        
        object_type.GetMember(bit, 
                    MemberTypes.Field ||| 
                    MemberTypes.Method ||| 
                    MemberTypes.Property, 
                    BindingFlags.Public ||| 
                    BindingFlags.NonPublic ||| 
                    BindingFlags.Instance) 
                |> Array.tryPick 
                (fun member_info -> 
                    match member_info with
                    | :? MethodInfo as m -> 
                        if m.GetParameters().Length = 0 
                        then Some <| m.Invoke(object_instance, null) else None
                    | :? FieldInfo as f -> Some <| f.GetValue(object_instance)
                    | :? PropertyInfo as p -> 
                        if p.GetIndexParameters().Length = 0
                        then Some <| p.GetValue(object_instance, null)
                        else None
                    | _ -> None
                )
    
    /// <summary>
    /// Tries to resolve current bit as an interface implemented by the current object        
    /// </summary>
    /// <remarks>
    /// This resolver takes care of explicit interface method implementations
    /// If the name of the bit is recognized as a name of interface implemente by the
    /// object, it returns a tuple with the interface type as the first member and the 
    /// original object as the second one 
    /// </remarks>
    let private interface_resolver bit (current:obj) =
        let itype = current.GetType().GetInterface(bit) 
        if itype = null then None
        else Some ((itype, current) :> obj)

    /// <summary>
    /// Tries to resolve current bit as an index using the indexer on the current object        
    /// </summary>
    /// <remarks>
    /// Among other things takes care of lists and dictionaries through their indexers<br/>
    /// before attempting to resolve the index value converts the value to the type
    /// required by the property index type<br/>
    /// If it fails to resolve the index value in its origianl form (string)
    /// tries to convert it to an integer and repeats the attempt
    /// </remarks>
    let private indexer_resolver bit (current:obj) =
    
        /// determine the type we need to use with the object
        /// in case if the bit references an explicit interface implementation
        /// the type will come in the tuple, otherwise the type of the object itself 
        /// is used
        let object_type, object_instance = 
            match current with
            | :? (System.Type*obj) as a  -> a
            | _ -> (current.GetType(), current)

        /// try to resolve current bit against and indexer
        let resolve_indexer value (indexer:MemberInfo) = 
            match indexer with
            | :? PropertyInfo as p ->
                let parms = p.GetIndexParameters()
                // we are only interested in indexers with a single parameter
                if parms.Length = 1
                then
                    try
                        Some <| p.GetValue(object_instance, [|System.Convert.ChangeType(value, parms.[0].ParameterType)|])
                    with
                    |_ -> None
                else None
            | _ -> None
            
        /// an array of all indexer properties
        let indexers = 
            object_type.GetMember("Item", 
                        MemberTypes.Property, 
                        BindingFlags.Public ||| 
                        BindingFlags.NonPublic ||| 
                        BindingFlags.Instance) 
        
        match indexers |> Array.tryPick (resolve_indexer bit) with
        | Some result -> Some result
        | None -> 
            // convert the bit to integer and try again
            match bit |> string |> System.Int32.TryParse with
            | true, int_bit -> 
                indexers |> Array.tryPick (resolve_indexer int_bit)
            | _ -> None 

    /// <summary>
    /// Tries to resolve current bit by calling avary resolver in turn
    /// and returning after the first one returning some value        
    /// </summary>
    let private resolve_member bit current =
        let resolvers = [member_resolver; interface_resolver; indexer_resolver] 
        match current with
        | null -> None
        | _ -> 
            resolvers |> 
                List.tryPick 
                    (fun resolver -> 
                        try
                            resolver bit current
                        with
                        | _ -> None
                    )
    
    /// <summary>
    /// recurses through the supplied list and attempts to navigate the "current" object graph using
    /// name elements provided by the list. This function will invoke method and evaluate properties and members
    /// </summary>
    let rec internal resolve_members current = function
        | [] -> None
        | h::[] -> resolve_member h current
        | h::tail ->
            match resolve_member h current with
            | Some v -> resolve_members v (tail)
            | None -> None

    /// <summary>
    /// A template variable, resolvable against a given context. The variable may be
    /// a hard-coded string (if it begins and ends with single or double quote
    /// marks)::
    /// </summary>
    /// <remarks>
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
    /// </remarks>
    type Variable(context: ParsingContext, token:Lexer.TextToken) =

        /// Returns a tuple of (var * value * needs translation)
        let find_literal = function
            | Utilities.Int i -> (None, Some (i :> obj), false)
            | Utilities.Float f -> (None, Some (f :> obj), false)
            | _ as v -> OutputHandling.strip_markers v

        let var, literal, translate = find_literal token.Value
        
        // So far no errors
        let error = Error.None
        
        let error, lookups = 
            match var with
            | Some v -> 
                try
                    let var_list = List.of_array (v.Split(Constants.VARIABLE_ATTRIBUTE_SEPARATOR.ToCharArray()))
                    var_list |> 
                        List.iter 
                            (fun v ->
                                if v = "" || v.StartsWith("-") || v.StartsWith("_") 
                                then 
                                    raise (SyntaxError 
                                            (sprintf "Variables and attributes may not be empty, begin with underscores or minus (-) signs: '%s'" v))
                        )
                    error, Some var_list  
                with
                | :? SyntaxError as ex -> 
                    if (context.Provider.Settings.[Constants.EXCEPTION_IF_ERROR] :?> bool)
                    then
                        raise (SyntaxException(ex.Message, Text token))
                    else
                        new Error(2, ex.Message), None
                | _ -> rethrow()
            | None -> error, None
        
        let clean_nulls  = function
        | Some v as orig -> if v = null then None else orig
        | None -> None
        
        let template_string_if_invalid = context.Provider.Settings.TryFind(Constants.TEMPLATE_STRING_IF_INVALID)

        /// <summary>
        /// Resolves this variable against a given context
        /// </summary>
        member this.Resolve (context: IContext) =
            match lookups with
            | None -> (literal.Value, false)
            | Some lkp ->
                let result =
                    match 
                        match context.tryfind (List.hd lkp) with
                        | Some v -> 
                            match List.tl lkp with
                            // make sure we don't end up with a 'Some null'
                            | [] -> Some v |> clean_nulls
                            | list -> 
                                resolve_members v (list) |> clean_nulls
                        | None -> None
                        with
                    | Some v1 -> v1
                    | None -> 
                        match template_string_if_invalid with
                        | Some o -> o
                        | None -> "" :> obj
                (result, context.Autoescape)

        member this.IsLiteral with get() = lookups.IsNone

        interface INode with            
            member x.NodeType = NodeType.Reference 
            member x.Position = token.Location.Offset
            member x.Length = token.Location.Length
            member x.Values =  seq []
            member x.ErrorMessage = error
            member x.Description = ""
            member x.Nodes = Map.empty :> IDictionary<string, IEnumerable<INode>>
