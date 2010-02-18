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
    
    //SuperBlock that can be inserted into context to use it while rendering {{block.super}} variable
    and SuperBlock (context: ParsingContext, token:BlockToken, parents: BlockNode list) =
        inherit TagNode(context, token)
        
        let nodes, parent = 
            match parents with
            | h::[] -> h.nodelist, None
            | h::t -> h.nodelist, Some <| new SuperBlock(context, token,t)
            | _ -> [], None
        
        override this.walk manager walker = 
            {walker with 
                parent=Some walker; 
                nodes= nodes;
                context = 
                    if Option.isSome parent then
                        //We have to replace(refresh) the context, because later, 
                        //while rendering another {{block.super}} variable, 
                        //we should use the parent SuperBlock - not the same all the time.
                        //Avoiding this replacement may cause endless looping in case when 
                        //you have chain of block.super variables (see 'extends 05-CHAIN' unit test).
                        walker.context.remove("block").add("block", ({super= Option.get parent} :> obj))
                    else
                        walker.context.remove("block")
            }
            
        override this.nodelist with get() = nodes

    //During parsing the templates, we build(see ExtendsNode) dictionary "__blockmap" consisting 
    //of different blocks. For each block name we have a list of blocks, 
    //where the most child block is in the head and the most parental - in the tail of the list.
    //During the rendering, we will use the head of each list and give the head's nodes(=final_nodelist)
    //to the walker in order to implement the blocks overriding. 
    //Moreover, we will use the rest (=parents) of the list for {{super.bock}} issues.
    //This rest of the list will be added to context with a "block" key.
    and BlockNode(parsing_context: ParsingContext, token: BlockToken, name: string, nodelist: INodeImpl list, ?parent: BlockNode) =
        inherit TagNode(parsing_context, token)

        //get the head's nodes to give them later to the walker
        //the rest of the list will be given to the context for {{super.block}} issues
        member x.MapNodes blocks =
            match Map.tryFind x.Name blocks with
            | Some (children: BlockNode list) -> 
                match children with
                | active::parents ->
                    //always append current block to 'parents'. We need this block in case 
                    //when {{block.super}} refers to a simple block, not another {{block.super}} 
                    active.nodelist, List.append parents [x]
                | [] -> x.nodelist, []
            | None -> x.nodelist, []
        
        member x.Name = name
        member x.Parent = parent
        
        //get the final_nodelist and parents from the "__blockmap" dictionary using MapNodes function
        override x.walk manager walker =
            let final_nodelist, parents =
                match walker.context.tryfind "__blockmap" with
                | None -> x.nodelist, []
                | Some ext -> 
                    x.MapNodes (ext :?> Map<string, BlockNode list>)
                    
            {walker with 
                parent=Some walker; 
                nodes=final_nodelist; 
                context= 
                    if  not (List.isEmpty parents) then
                        //add SuperBlockPointer to the context. Later, when we will render {{block.super}} variable,
                        //we will get inside this inserted SuperBlock and 'walk' it.
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
                (h.Nodes.Values |> Seq.cast |> Seq.map(fun (seq) -> (Seq.toList seq)) |>
                    List.concat |>
                        List.filter (fun node -> match node with | :? Node -> true | _ -> false))
                             @ unfold_nodes t
        | _ -> []

        // even though the extends filters its node list, we still need to filter the flattened list because of nested blocks
        let blocks = Map.ofList <| List.choose 
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
                                        
        override x.elements = (parent :> INode) :: base.elements
        override x.Nodes =
            base.Nodes 
                |> Map.add (NDjango.Constants.NODELIST_EXTENDS_BLOCKS) (Seq.ofList nodes)

        override this.walk manager walker =
            let context = 
                match walker.context.tryfind "__blockmap" with
                | Some v -> walker.context.add ("__blockmap", (join_replace (v:?> Map<_,_>) (Map.toList blocks) :> obj))
                | None -> walker.context.add ("__blockmap", (blocks :> obj))
       
            {walker with nodes=(get_template manager parent context).Nodes; context = context}