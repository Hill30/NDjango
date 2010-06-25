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

module TypeResolver =
    
    type ITypeResolver =
        abstract member Resolve: type_name: string -> IDjangoType list
        
    type DefaultTypeResolver() =
        interface ITypeResolver with
            member x.Resolve type_name = []
            
    type ValueType(name) =
        interface IDjangoType with
            member x.Name = name
            member x.Type = DjangoType.Value
            member x.Members = Seq.empty
            
    let private resolver = DefaultTypeResolver() :> ITypeResolver
    
    let Resolve = resolver.Resolve
        
    