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

open NDjango.Interfaces
open NDjango.Expressions

module TypeResolver =
    
   type DefaultTypeResolver() =
        interface ITypeResolver with
            member x.Resolve type_name = [] |> List.toSeq
            
   let private resolver = 
        let resolver_type =
            System.AppDomain.CurrentDomain.GetAssemblies() |> 
                Array.tryPick 
                    (fun assembly ->
                        if not (assembly.FullName.StartsWith("NDjango")) then None
                        else assembly.GetTypes() |>
                                Array.tryPick
                                    (fun clrType ->
                                        if clrType = typeof<DefaultTypeResolver> then None
                                        else if clrType = typeof<ITypeResolver> then None
                                        else if typeof<ITypeResolver>.IsAssignableFrom(clrType) then Some clrType
                                        else None
                                    )
                        )
        match resolver_type with
        | Some resolver_type -> System.Activator.CreateInstance(resolver_type) :?> ITypeResolver
        | None -> DefaultTypeResolver() :> ITypeResolver
    
   let Resolve = resolver.Resolve
        
   type ValueDjangoType(name) =
        interface IDjangoType with
            member x.Name = name
            member x.Type = DjangoType.Value
            member x.Members = Seq.empty

    type CLRTypeMember(expression: FilterExpression, member_name:string) =
        interface IDjangoType with
            member x.Name = member_name
            member x.Type = DjangoType.Value
            member x.Members = Seq.empty

    type CLRType(name, type_name) =
        interface IDjangoType with
            member x.Name = name
            member x.Type = DjangoType.Type
            member x.Members = Resolve type_name
           
    