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

    and SuperBlock (context: ParsingContext, token:BlockToken, parents: BlockNode list) =
        inherit TagNode(context, token)
        
        let nodes, parent = 
            match parents with
            | h::[] -> h.nodelist, None
            | h::t -> h.nodelist, Some <| new SuperBlock(context, token,t)
            | _ -> [], None
        
        override this.walk manager walker = 
            {walker with parent=Some walker; nodes= nodes}
            
        override this.nodelist with get() = nodes
        
        member this.super = 
            match parent with
            | Some v -> v
            | None -> new SuperBlock(context, token,[])
        
        
    and BlockNode(parsing_context: ParsingContext, token: BlockToken, name: string, nodelist: INodeImpl list, ?parent: BlockNode) =
        inherit TagNode(parsing_context, token)

        member x.MapNodes blocks =
            match Map.tryFind x.Name blocks with
            | Some (children: BlockNode list) -> 
                match children with
                | active::parents ->
                    active.nodelist, (*[x], true //*) (match parents with | [] -> [x] | _ -> parents), true
                | [] -> x.nodelist, [], true
            | None -> x.nodelist, [], false
        
        member x.Name = name
        member x.Parent = parent
        
        override x.walk manager walker =
            let final_nodelist, parents, overriden =
                match walker.context.tryfind "__blockmap" with
                | None -> x.nodelist, [], false
                | Some ext -> 
                    x.MapNodes (ext :?> Map<string, BlockNode list>)
                    
            {walker with 
                parent=Some walker; 
                nodes=final_nodelist; 
                context= 
                    if overriden && not (List.isEmpty parents) then
                        walker.context.add("block", ({super= new SuperBlock(parsing_context, token, parents)} :> obj))
                    else
                        walker.context
            }
            
        override x.nodelist = nodelist
       
    and ExtendsNode(parsing_context: ParsingContext, token: BlockToken, nodes: INode list, parent: Expressions.FilterExpression) =
        inherit TagNode(parsing_context, token)
        
        /// produces a flattened list of all nodes and child nodes within a 'node list'.
        /// the 'node list' is a list of all nodes collected from Nodes property of the INode interface
        let rec unfold_nodes = function
        | (h:INode)::t -> 
            h :: unfold_nodes 
                (h.Nodes.Values |> Seq.cast |> Seq.map(fun (seq) -> (Seq.to_list seq)) |>
                    List.concat |>
                        List.filter (fun node -> match node with | :? Node -> true | _ -> false))
                             @ unfold_nodes t
        | _ -> []

        // even though the extends filters its node list, we still need to filter the flattened list because of nested blocks
        let blocks = Map.of_list <| List.choose 
                        (fun (node: INode) ->  match node with | :? BlockNode as block -> Some (block.Name,[block]) | _ -> None) 
                        (unfold_nodes nodes)                      

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