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


namespace NDjango.Interfaces

open System.Collections.Generic
open System.IO
open NDjango

type NodeType =
            
        /// <summary>
        /// The whole tag.
        /// <example>{% if somevalue %}</example>
        /// </summary>
        | Construct = 0x0001
        
        /// <summary>
        /// The markers, which frame django tag. 
        /// <example>{%, %}</example>
        /// </summary>
        | Marker = 0x0002
        
        /// <summary>
        /// Django template tag.
        /// <example> "with", "for", "ifequal"</example>
        /// </summary>
        | TagName = 0x0003

        /// <summary>
        /// The keyword, as required by some tags.
        /// <example>"and", "as"</example>
        /// </summary>
        | Keyword = 0x0004

        /// <summary>
        /// The variable definition used in tags which introduce new variables i.e. 
        /// loop variable in the For tag.
        /// <example>loop_item</example>
        /// </summary>
        | Variable = 0x0005

        /// <summary>
        /// Expression, which consists of a reference followed by 0 or more filters
        /// <example>User.DoB|date:"D d M Y"</example>
        /// </summary>
        | Expression = 0x0006
        
        /// <summary>
        /// Reference to a value in the current context.
        /// <example>User.DoB</example>
        /// </summary>
        | Reference = 0x0007

        /// <summary>
        /// Filter with o without a parameter. Parameter can be a constant or a reference
        /// <example>default:"nothing"</example>
        /// </summary>
        | Filter = 0x0008

        /// <summary>
        /// The name of the filter.
        /// <example>"length", "first", "default"</example>
        /// </summary>
        | FilterName = 0x0009
        
        /// <summary>
        /// Filter parameter.
        /// <example>any valid value</example>
        /// </summary>
        | FilterParam = 0x000a

        /// <summary>
        /// Text node.
        /// <example>any valid value</example>
        /// </summary>
        | Text = 0x000b

        /// <summary>
        /// Text node.
        /// <example>any valid value</example>
        /// </summary>
        | Comment = 0x000c

/// A representation of a node of the template abstract syntax tree    
type INode =

    /// TagNode type
    abstract member NodeType: NodeType 
    
    /// Position of the first character of the node text
    abstract member Position: int
    
    /// Length of the node text
    abstract member Length: int

    /// a list of values allowed for the node
    abstract member Values: string list
    
    /// message associated with the node
    abstract member ErrorMessage: OutputHandling.Error
    
    /// TagNode description (will be shown in the tooltip)
    abstract member Description: string
    
    /// node lists
    abstract member Nodes: IDictionary<string, IEnumerable<INode>>

/// A no-parameter filter
type ISimpleFilter = 
    /// Applies the filter to the value
    abstract member Perform: value: obj -> obj
    
/// A filter that accepts a single parameter of type 'a
type IFilter =
    inherit ISimpleFilter
    /// Provides the default value for the filter parameter
    abstract member DefaultValue: obj
    /// Applies the filter to the value using provided parameter value
    abstract member PerformWithParam: value:obj * parameter:obj -> obj    
    
/// Template loader. Retrieves the template content
type ITemplateLoader = 
    /// Given the path to the template returns the textreader to be used to retrieve the template content
    abstract member GetTemplate: path:string -> TextReader
    /// returns true if the specified template was modified since the specified time
    abstract member IsUpdated: path:string * timestamp:System.DateTime -> bool
    
    
/// An execution context container. This interface defines a set of methods necessary 
/// for templates and external entities to exchange information.
type IContext =
    /// Adds an object to the context
    abstract member add:(string*obj)->IContext
    
    /// Attempts to find an object in the context by the key
    abstract member tryfind: string->obj option
    
    /// Indicates that this Context is in Autoescape mode
    abstract member Autoescape: bool
    
    /// Returns a new Context with the specified Autoescape mode
    abstract member WithAutoescape: bool -> IContext

/// Single threaded template manager. Caches templates it renders in a non-synchronized dictionary
/// should be used only to service rendering requests from a single thread
type ITemplateManager = 
    
    /// given the path to the template and context returns the <see cref="System.IO.TextReader"/> 
    /// that will stream out the results of the render.
    abstract member RenderTemplate: path:string * context:IDictionary<string, obj> -> TextReader

    /// given the path to the template and context returns the template implementation
    abstract member GetTemplate: path:string -> ITemplate

/// Template imeplementation. This interface effectively represents the root-level node
/// in the Django AST.
and ITemplate =
    /// Recursivly "walks" the AST, returning a text reader that will stream out the 
    /// results of the render.
    abstract Walk: ITemplateManager -> IDictionary<string, obj> -> System.IO.TextReader
    
    /// A list of top level sibling nodes
    abstract Nodes: INodeImpl list

/// Rendering state used by the ASTWalker to keep track of the rendering process as it walks through 
/// the template abstract syntax tree
and Walker =
    {
        /// parent walker to be resumed after the processing of this one is completed
        parent: Walker option
        /// List of nodes to walk
        nodes: INodeImpl list
        /// string rendered by the last node
        buffer: string
        /// the index of the first character in the buffer yet to be sent to output
        bufferIndex: int
        /// rendering context
        context: IContext
    }
    
/// A representation of a node of the template abstract syntax tree    
and INodeImpl =

    /// Indicates whether this node must be the first non-text node in the template
    abstract member must_be_first: bool
    
    /// The token that defined the node
    abstract member Token : Lexer.Token

    /// Processes this node and advances the walker to reflect the progress made
    abstract member walk: manager:ITemplateManager -> walker:Walker -> Walker

/// Parsing interface definition
type IParser =
    /// Produces a commited node list and uncommited token list as a result of parsing until
    /// a block from the string list is encotuntered
    abstract member Parse: parent: Lexer.BlockToken option -> tokens:LazyList<Lexer.Token> -> parse_until:string list -> (INodeImpl list * LazyList<Lexer.Token>)
   
    /// Parses the template From the source in the reader into the node list
    abstract member ParseTemplate: template:TextReader -> INodeImpl list
   
    /// Produces an uncommited token list as a result of parsing until
    /// a block from the string list is encotuntered
    abstract member Seek: tokens:LazyList<Lexer.Token> -> parse_until:string list -> LazyList<Lexer.Token>
    
/// Top level object managing multi threaded access to configuration settings and template cache.
and ITemplateManagerProvider =

    /// tag definitions available to the provider    
    abstract member Tags: Map<string, ITag>
    
    /// filter definitions available to the provider    
    abstract member Filters: Map<string, ISimpleFilter>

    /// current configuration settings
    abstract member Settings: Map<string, obj>

    /// current template loader
    abstract member Loader: ITemplateLoader

    /// Retrieves the requested template checking first the global
    /// dictionary and validating the timestamp
    abstract member GetTemplate: string -> (ITemplate* System.DateTime)

    /// Retrieves the requested template without checking the 
    /// local dictionary and/or timestamp
    /// the retrieved template is placed in the dictionary replacing 
    /// the existing template with the same name (if any)
    abstract member LoadTemplate: string -> (ITemplate* System.DateTime)
        
/// A tag implementation
and ITag = 
    /// Transforms a {% %} tag into a list of nodes and uncommited token list
    abstract member Perform: Lexer.BlockToken -> ITemplateManagerProvider -> LazyList<Lexer.Token> -> (INodeImpl * LazyList<Lexer.Token>)

type RenderingError (message: string, ?innerException: exn) =
        inherit System.ApplicationException(message, defaultArg innerException null)

type TemplateRenderingError (message: string, token:NDjango.Lexer.TextToken, ?innerException: exn) =
        inherit System.ApplicationException(message, defaultArg innerException null)

type internal CompoundSyntaxError(message, nodes:INodeImpl list) =
    inherit OutputHandling.SyntaxError(message)
    
    member x.Nodes = nodes

/// Tags and/or filters marked with this attribute will be registered under the name
/// supplied by the attribute unless the name will be provided explicitly during the registartion
type NameAttribute(name:string) = 
    inherit System.Attribute() 
    member x.Name = name