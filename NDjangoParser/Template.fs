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
open System.Collections
open System.Collections.Generic
open System.IO

open NDjango.Interfaces
open NDjango.Filters
open NDjango.Constants
open NDjango.Tags
open NDjango.Tags.Misc

module internal Template =    


    type internal Manager(provider:ITemplateManagerProvider, templates) =
        
        let templates = ref(templates)
        
        let load_template name validated =
            let tr = 
                if validated then provider.LoadTemplate name
                else provider.GetTemplate name
            templates := Map.add name tr !templates
            fst tr

        let validate_template = 
            if (provider.Settings.[Constants.RELOAD_IF_UPDATED] :?> bool) then provider.Loader.IsUpdated
            else (fun (name,ts) -> false) 
        
        member x.Provider = provider
        
        interface ITemplateManager with
            member x.RenderTemplate (name, context) =
                ((x :>ITemplateManager).GetTemplate name).Walk x context

            member x.GetTemplate name =
                match Map.tryFind name !templates with
                | Some (template, ts) -> 
                    if validate_template (name, ts) then
                       load_template name true
                    else
                       template
                | None ->
                       load_template name false
        
            
    /// Implements the template (ITemplate interface)
    and internal Impl(provider : ITemplateManagerProvider, template: TextReader) =

        let node_list = (provider :?> IParser).ParseTemplate template

//            // this will cause the TextReader to be closed when the template goes out of scope
//            use template = template
//            fst <| (provider :?> IParser).Parse (NDjango.Lexer.tokenize template) []
        
        interface ITemplate with
            member this.Walk manager context=
                new NDjango.ASTWalker.Reader (
                    manager,
                    {parent=None; 
                     nodes=node_list; 
                     buffer="";
                     bufferIndex = 0; 
                     context=
                        new Context(
                            context, 
                            (new Map<string,obj>(context |> Seq.map (fun item-> (item.Key, item.Value)))),
                            ((manager :?> Manager).Provider.Settings.[Constants.DEFAULT_AUTOESCAPE] :?> bool)
                            )
                    }) :> System.IO.TextReader
                
            member this.Nodes = node_list
            
    and
        private Context (externalContext, variables, autoescape: bool) =

        override this.ToString() =
            
            let autoescape = "autoescape = " + autoescape.ToString() + "\r\n"
            let vars =
                variables |> Microsoft.FSharp.Collections.Map.fold
                    (fun result name value -> 
                        result + name + 
                            if (value = null) then " = NULL\r\n"
                            else " = \"" + value.ToString() + "\"\r\n" 
                        ) "" 
                        
            externalContext.ToString() + "\r\n---- NDjango Context ----\r\nSettings:\r\n" + autoescape + "Variables:\r\n" + vars

        interface IContext with
            member this.add(pair) =
                new Context(externalContext, Map.add (fst pair) (snd pair) variables, autoescape) :> IContext
                
            member this.tryfind(name) =
                match variables.TryFind(name) with
                | Some v -> Some v
                | None -> None 
                
            member this.Autoescape = autoescape

            member this.WithAutoescape(value) =
                new Context(externalContext, variables, value) :> IContext
