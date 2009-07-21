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

open Interfaces
open Filters
open Constants
open NDjango.Tags
open NDjango.Tags.Misc

module Template =
    
    type private DefaultLoader() =
        interface ITemplateLoader with
            member this.GetTemplate source = 
                if not <| File.Exists(source) then
                    raise (FileNotFoundException (sprintf "Could not locate template '%s'" source))
                else
                    (new StreamReader(source) :> TextReader)
                    
            member this.IsUpdated (source, timestamp) = File.GetLastWriteTime(source) > timestamp
            
    
    /// adds the key/value pair into the supplied map (usage: map ++ (key, value))
    let private (++) map (key: 'a, value: 'b) = Map.add key value map

    let private standardFilters = 
        new Map<string, ISimpleFilter>([])
            ++ ("date", (new Now.DateFilter() :> ISimpleFilter))
            ++ ("escape", (new EscapeFilter() :> ISimpleFilter))
            ++ ("force_escape", (new ForceEscapeFilter() :> ISimpleFilter))
            ++ ("slugify", (new Slugify() :> ISimpleFilter))
            ++ ("truncatewords" , (new TruncateWords() :> ISimpleFilter))
            ++ ("urlencode", (new UrlEncodeFilter() :> ISimpleFilter))
            ++ ("urlize", (new UrlizeFilter() :> ISimpleFilter))
            ++ ("urlizetrunc", (new UrlizeTruncFilter() :> ISimpleFilter))
            ++ ("wordwrap", (new WordWrapFilter() :> ISimpleFilter))
            ++ ("default_if_none", (new DefaultIfNoneFilter() :> ISimpleFilter))
            ++ ("linebreaks", (new LineBreaksFilter() :> ISimpleFilter))
            ++ ("linebreaksbr", (new LineBreaksBrFilter() :> ISimpleFilter))
            ++ ("striptags", (new StripTagsFilter() :> ISimpleFilter))
            ++ ("join", (new JoinFilter() :> ISimpleFilter))
            ++ ("yesno", (new YesNoFilter() :> ISimpleFilter))
            ++ ("dictsort", (new DictSortFilter() :> ISimpleFilter))
            ++ ("dictsortreversed", (new DictSortReversedFilter() :> ISimpleFilter))
            ++ ("time", (new Now.TimeFilter() :> ISimpleFilter))
            ++ ("timesince", (new Now.TimeSinceFilter() :> ISimpleFilter))
            ++ ("timeuntil", (new Now.TimeUntilFilter() :> ISimpleFilter))
            ++ ("pluralize", (new PluralizeFilter() :> ISimpleFilter))
            ++ ("phone2numeric", (new Phone2numericFilter() :> ISimpleFilter))
            ++ ("filesizeformat", (new FileSizeFormatFilter() :> ISimpleFilter))
            
        
    let private standardTags =
        new Map<string, ITag>([])
            ++ ("autoescape", (new AutoescapeTag() :> ITag))
            ++ ("block", (new LoaderTags.BlockTag() :> ITag))
            ++ ("comment", (new CommentTag() :> ITag))
            ++ ("cycle", (new Cycle.Tag() :> ITag))
            ++ ("debug", (new DebugTag() :> ITag))
            ++ ("extends", (new LoaderTags.ExtendsTag() :> ITag))
            ++ ("filter", (new Filter.FilterTag() :> ITag))
            ++ ("firstof", (new FirstOfTag() :> ITag))
            ++ ("for", (new For.Tag() :> ITag))
            ++ ("if", (new If.Tag() :> ITag))
            ++ ("ifchanged", (new IfChanged.Tag() :> ITag))
            ++ ("ifequal", (new IfEqual.Tag(true) :> ITag))
            ++ ("ifnotequal", (new IfEqual.Tag(false) :> ITag))
            ++ ("include", (new LoaderTags.IncludeTag() :> ITag))
            ++ ("now", (new Now.Tag() :> ITag))
            ++ ("regroup", (new RegroupTag() :> ITag))
            ++ ("spaceless", (new SpacelessTag() :> ITag))
            ++ ("ssi", (new LoaderTags.SsiTag() :> ITag))
            ++ ("templatetag", (new TemplateTag() :> ITag))
            ++ ("widthratio", (new WidthRatioTag() :> ITag))
            ++ ("with", (new WithTag() :> ITag))
        
    let private defaultSettings = 
        new Map<string, string>([])
            ++ ("settings.DEFAULT_AUTOESCAPE","true")
            ++ ("settings.TEMPLATE_STRING_IF_INVALID","")

    type Manager 
        private (
                    filters: Map<string, ISimpleFilter>, 
                    tags: Map<string, ITag>, 
                    templates: Map<string, ITemplate * System.DateTime>, 
                    loader: ITemplateLoader, 
                    settings:Map<string,string>) =

        /// global lock
        static let lockMgr = new obj()

        /// pointer to the most recent template manager. don't want to have an
        /// interface for the sake of an interface, so we'll make it obj to
        /// avoid dealing with circular references
        static let active = 
            ref (new Manager(
                            standardFilters,
                            standardTags,
                            Map.empty,
                            new DefaultLoader(),
                            defaultSettings))
        
        member private this.Templates = templates
        member private this.Loader = loader
        member private this.Settings = settings
        member internal this.Filters = filters
        member internal this.Tags = tags

        /// Retrieves the current active template manager
        static member internal  GetActiveManager = lock lockMgr (fun () -> !active)
            
        /// Creates a new filter manager with the filter registered 
        static member RegisterFilter (name, filter) =
            lock lockMgr
                (fun () ->
                    let mgr = Manager.GetActiveManager
                    let new_filters = Map.add name filter mgr.Filters
                    let m = new Manager(new_filters, mgr.Tags, mgr.Templates, mgr.Loader, mgr.Settings)
                    active := m
                    m
                )
        
        /// Creates a new filter manager with the tag registered 
        static member RegisterTag (name,tag) =
            lock lockMgr
                (fun() ->
                    let mgr = Manager.GetActiveManager
                    let new_tags = Map.add name tag mgr.Tags
                    let m = new Manager(mgr.Filters, new_tags, mgr.Templates, mgr.Loader, mgr.Settings)
                    active := m
                    m
                )
        
        /// Creates a new filter manager with the tempalte registered 
        static member RegisterLoader (loader:ITemplateLoader) =
            lock lockMgr 
                (fun () -> 
                    let mgr = Manager.GetActiveManager
                    let m = new Manager(mgr.Filters, mgr.Tags, mgr.Templates, loader, mgr.Settings)
                    active := m
                    m
                )

        /// Retrieves the template, and verifies that if a template is currently available,
        /// that it is still valid. if not valid, causes a reload to occur, and all outstanding
        /// 
        member private this.GetAndReloadIfNeeded full_name template ts = 
            if loader.IsUpdated (full_name, ts) then
                lock lockMgr 
                    (fun () -> (!active).RegisterTemplate full_name (loader.GetTemplate full_name) )
            else
                ((this:>ITemplateManager), template)

        /// Retrieves a template, along with the instance of the ITemplateManager that contains it.
        /// While any GetTemplate request is guaranteed to retrieve the latest version of the
        /// ITemplate, retaining the instance (the second in the returned tuple) will be more
        /// efficient, as subsequent requests for that template from the returned instance are
        /// guaranteed to be non-blocking
        member this.GetTemplate full_name =
            match Map.tryFind full_name templates with
            | Some (template, ts) -> 
                this.GetAndReloadIfNeeded full_name template ts
            | None -> 
                lock lockMgr 
                    (fun () -> 
                        let mgr = !active
                        match Map.tryFind full_name mgr.Templates with
                        | Some (template, ts) -> this.GetAndReloadIfNeeded full_name template ts
                        | None -> mgr.RegisterTemplate full_name (loader.GetTemplate full_name)
                    )

        interface ITemplateContainer with
            member this.RenderTemplate (full_name, context) = 
                let manager, template = this.GetTemplate full_name
                (manager, template.Walk context)

            member this.GetTemplateReader full_name = loader.GetTemplate full_name
                            
            member this.Settings = settings
            
            member this.FindTag name = Map.tryFind name tags
            
            member this.FindFilter name = Map.tryFind name filters

        /// Creates a new filter manager with the tempalte registered 
        member private this.RegisterTemplate name template =
            // this is called internally, from within a lock statement
            let t = new Impl(template, this) :> ITemplate
            
            // this may be an update call. if that's the case, then we should
            // add the template into a map that has it already removed
            let new_templates = 
                Map.add name (t, System.DateTime.Now) <|
                match Map.tryFind name this.Templates with
                | Some (template, ts) -> Map.remove name this.Templates
                | None -> this.Templates
                
            active := new Manager(this.Filters, this.Tags, new_templates, this.Loader, this.Settings)
            (!active :> ITemplateManager), t
            
    /// Implements the template (ITemplate interface)
    and private Impl(template, manager: Manager) =

        let node_list =

            let parser = new Parser.DefaultParser(manager) :> IParser
            // this will cause the TextReader to be closed when the template goes out of scope
            use template = template
            fst <| parser.Parse (Lexer.tokenize template) []
        
        interface ITemplate with
            member this.Walk context=
                new ASTWalker.Reader (
                    {parent=None; 
                     nodes=node_list; 
                     buffer="";
                     bufferIndex = 0; 
                     context=new Context(manager, context, (new Map<string,obj>(context |> Seq.map (fun item-> (item.Key, item.Value)))))
                    }) :> System.IO.TextReader
                
            member this.Nodes = node_list
            
    and
        private Context (manager: ITemplateManager, externalContext: obj, variables: Map<string, obj>, ?autoescape: bool) =

        let autoescape = match autoescape with | Some v -> v | None -> bool.Parse(manager.Settings.["settings.DEFAULT_AUTOESCAPE"])
        
        override this.ToString() =
            
            let autoescape = "autoescape = " + autoescape.ToString() + "\r\n"
            // Map.fold works like Map.foldBack, and folds in reverse direction! Looks like an issue in F#
            let vars = 
                Microsoft.FSharp.Collections.Map.fold
                    (fun (result:string) (name:string) (value:obj)  -> 
                        result + name + 
                            if (value = null) then " = NULL\r\n"
                            else " = \"" + value.ToString() + "\"\r\n" 
                        ) "" variables
                        
            externalContext.ToString() + "\r\n---- NDjango Context ----\r\nSettings:\r\n" + autoescape + "Variables:\r\n" + vars
            
        interface IContext with
            member this.add(pair) =
                new Context(manager, externalContext, Map.add (fst pair) (snd pair) variables, autoescape) :> IContext
                
            member this.tryfind(name) =
                match variables.TryFind(name) with
                | Some v -> Some v
                | None -> None 
                
            member this.GetTemplate(template) = 
                (manager :?> Manager).GetTemplate(template)

            member this.TEMPLATE_STRING_IF_INVALID = manager.Settings.["settings.TEMPLATE_STRING_IF_INVALID"] :> obj
            
            member this.Autoescape = autoescape

            member this.WithAutoescape(value) =
                new Context(manager, externalContext, variables, value) :> IContext
                
            member this.WithNewManager(manager) =
                new Context(manager, externalContext, variables, autoescape) :> IContext
                
            member this.Manager =
                manager
            
