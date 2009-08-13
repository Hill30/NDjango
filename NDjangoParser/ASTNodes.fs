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

open System.Collections.Generic
open OutputHandling
open Lexer
open NDjango.Interfaces
open Expressions
open ParserNodes

module internal ASTNodes =

    /// retrieves a template given the template name. The name is supplied as a FilterExpression
    /// which when resolved should eithter get a ready to use template, or a string (url)
    /// to the source code for the template
    let get_template (manager:ITemplateManager) (templateRef:FilterExpression) context =
        match fst (templateRef.Resolve context false) with  // ignoreFailures is false because we have to have a name.
        | Some o -> 
            match o with
            | :? ITemplate as template -> template
            | :? string as name -> manager.GetTemplate name
            | _ -> raise (RenderingError (sprintf "Invalid template name in 'extends' tag. Can't construct template from %A" o))
        | _ -> raise (RenderingError (sprintf "Invalid template name in 'extends' tag. Variable %A is undefined" templateRef))

    type SuperBlockPointer = {super:TagNode}

    and SuperBlock (provider: ITemplateManagerProvider, token:BlockToken, parents: BlockNode list) =
        inherit TagNode(provider, token)
        
        let nodelist, parent = 
            match parents with
            | h::[] -> h.Nodelist, None
            | h::t -> h.Nodelist, Some <| new SuperBlock(provider, token,t)
            | _ -> [], None
        
        override this.walk manager walker = 
            {walker with parent=Some walker; nodes= nodelist}
            
        override this.nodes with get() = nodelist
        
        member this.super = 
            match parent with
            | Some v -> v
            | None -> new SuperBlock(provider, token,[])
        
        
    and BlockNode(provider: ITemplateManagerProvider, token: BlockToken, name: string, nodelist: INodeImpl list, ?parent: BlockNode) =
        inherit TagNode(provider, token)

        member this.MapNodes blocks =
            match Map.tryFind this.Name blocks with
            | Some (children: BlockNode list) -> 
                match children with
                | active::parents ->
                    active.Nodelist, (match parents with | [] -> [this] | _ -> parents), true
                | [] -> this.Nodelist, [], true
            | None -> this.Nodelist, [], false
        
        member this.Name = name
        member this.Parent = parent
        member internal this.Nodelist = nodelist
        
        override this.walk manager walker =
            let final_nodelist, parents, overriden =
                match walker.context.tryfind "__blockmap" with
                | None -> this.Nodelist, [], false
                | Some ext -> 
                    this.MapNodes (ext :?> Map<string, BlockNode list>)
                    
            {walker with 
                parent=Some walker; 
                nodes=final_nodelist; 
                context= 
                    if overriden && not (List.isEmpty parents) then
                        walker.context.add("block", ({super= new SuperBlock(provider, token, parents)} :> obj))
                    else
                        walker.context
            }
            
        override this.nodes with get() = this.Nodelist
       
    and ExtendsNode(provider: ITemplateManagerProvider, token: BlockToken, nodelist: INodeImpl list, parent: Expressions.FilterExpression) =
        inherit TagNode(provider, token)
            
        /// produces a flattened list of all nodes and child nodes within a node list
        let rec unfold_nodes = function
        | (h:INodeImpl)::t -> 
            h :: unfold_nodes (h:?>Node).nodes @ unfold_nodes t
        | _ -> []

        let blocks = Map.of_list 
                     <| List.choose 
                             (fun (node: INodeImpl) ->  match node with | :? BlockNode as block -> Some (block.Name,[block]) | _ -> None) 
                              (unfold_nodes nodelist)
                              

        let add_if_missing key value map = 
            match Map.tryFind key map with
            | Some v -> Map.add key (map.[key] @ value) map
            | None -> Map.add key value map
            
        let rec join_replace primary (secondary: ('a*'b list) list) =
            match secondary with
            | h::t -> 
                let key,value = h
                join_replace primary t |>
                add_if_missing key value
            | [] -> primary
            
        override this.walk manager walker =
            let context = 
                match walker.context.tryfind "__blockmap" with
                | Some v -> walker.context.add ("__blockmap", (join_replace (v:?> Map<_,_>) (Map.to_list blocks) :> obj))
                | None -> walker.context.add ("__blockmap", (blocks :> obj))
       
            {walker with nodes=(get_template manager parent context).Nodes; context = context}
