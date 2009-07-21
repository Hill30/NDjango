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
open NDjango.OutputHandling
open NDjango.Expressions

module internal Cycle =
    /// Cycles among the given strings each time this tag is encountered.
    /// 
    /// Within a loop, cycles among the given strings each time through
    /// the loop::
    /// 
    ///     {% for o in some_list %}
    ///         <tr class="{% cycle 'row1' 'row2' %}">
    ///             ...
    ///         </tr>
    ///     {% endfor %}
    /// 
    /// Outside of a loop, give the values a unique name the first time you call
    /// it, then use that name each sucessive time through::
    /// 
    ///     <tr class="{% cycle 'row1' 'row2' 'row3' as rowcolors %}">...</tr>
    ///     <tr class="{% cycle rowcolors %}">...</tr>
    ///     <tr class="{% cycle rowcolors %}">...</tr>
    /// 
    /// You can use any number of values, separated by spaces. Commas can also
    /// be used to separate values; if a comma is used, the cycle values are
    /// interpreted as literal strings.
        
    type CycleController(values : Variable list, origValues : Variable list) =

        member this.Values = values
        member this.OrigValues = origValues
        member this.Value = List.hd values
        
    /// Cycles among the given strings each time this tag is encountered.
    type Node(token:BlockToken, name: string, values: Variable list) =
        inherit NDjango.Interfaces.Node(Block token)
        
        let createController (controller: CycleController option) =
            match controller with
                | None -> new CycleController(values, values)
                | Some c -> 
                    match List.tl c.Values with
                        | head::tail -> new CycleController(List.tl c.Values, c.OrigValues)
                        | [] -> new CycleController (c.OrigValues, c.OrigValues)

        override this.walk walker = 
            let oldc = 
                match walker.context.tryfind ("$cycle" + name) with 
                | Some v -> Some (v :?> CycleController)
                | None -> 
                    match values with
                    | [] -> raise (TemplateSyntaxError (sprintf "Named cycle '%s' does not exist" name, Some (token:>obj)))
                    | _ -> None
            let newc =
                match oldc with
                    | None -> new CycleController(values, values)
                    | Some c -> 
                        match List.tl c.Values with
                            | head::tail -> new CycleController(List.tl c.Values, c.OrigValues)
                            | [] -> new CycleController (c.OrigValues, c.OrigValues)

            let buffer = newc.Value.Resolve(walker.context) |> fst |> string
            {walker with 
                buffer = buffer;
                context = (walker.context.add ("$cycle" + name, (newc :> obj))).add (name, (buffer :> obj)) 
                }
     
    type Tag() = 

            // Note that the django implementation returned the same instance of the 
            // CycleNode for each instance of a given named cycle tag. This implementation
            // Relies on the CycleNode instances to communicate with each other through 
            // the context object available at render time to synchronize their activities
        let checkForOldSyntax value = 
            if (String.IsNullOrEmpty value) then false
            else match value.[0] with
                    | '"' -> false
                    | '\'' -> false
                    | _ when value.Contains(",") -> true
                    | _ -> false
                            
        let normalize values =
            if List.exists checkForOldSyntax values then
                let compacted = List.fold (fun status value -> status + value) "" values
                List.map (fun value -> "'" + value + "'" ) (String.split [','] compacted)   
            else
                values
                
        interface NDjango.Interfaces.ITag with
            member this.Perform token parser tokens =
                let name, values =
                    match List.rev token.Args with
                    | [] -> raise (TemplateSyntaxError ("'cycle' tag requires at least one argument", Some (token:>obj)))
                    | name::"as"::values ->
                        (name, values |> List.rev |> normalize)
                    | _ ->
                        let values = token.Args |> normalize
                        if values.Length = 1 then (values.[0], [])
                        else ("$Anonymous$Cycle", values)
                        
                let values = List.map (fun v -> new Variable(parser, Block token, v)) values
                ((new Node(token, name, values) :> NDjango.Interfaces.Node), tokens)

