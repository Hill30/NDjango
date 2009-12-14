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
        /// The whole django construct.
        /// <example>{% if somevalue %} {{ variable }} </example>
        /// </summary>
        | Construct = 0x0001
        
        /// <summary>
        /// The markers, which frame django construct. 
        /// <example>{%, {{, %}, }}</example>
        /// </summary>
        | Marker = 0x0002
        
        /// <summary>
        /// Django tag name.
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
        | VariableDefinition = 0x0005

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

        /// <summary>
        /// A special node to carrying the parsing context info for code completion
        /// </summary>
        | ParsingContext = 0x000d

        /// <summary>
        /// A closing node terminating the list of the nested tags
        /// </summary>
        | CloseTag = 0x000e

/// Error message
type Error(severity:int, message:string) =
    /// indicates the severity of the error with 0 being the information message
    /// negative severity is used to mark a dummy message ("No messages" message) 
    member x.Severity = severity
    member x.Message = message
    static member None = new Error(-1, "") 

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
    
    /// Translation routine - when applied to the value returns it translated 
    /// to the language for the user
    abstract member Translate: string -> string

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
    
    /// Creates a translator for a given language
    abstract member CreateTranslator: string-> (string->string)

    /// current template loader
    abstract member Loader: ITemplateLoader

    /// Retrieves the requested template checking first the global
    /// dictionary and validating the timestamp
    abstract member GetTemplate: string -> (ITemplate * System.DateTime)

    /// Retrieves the requested template without checking the 
    /// local dictionary and/or timestamp
    /// the retrieved template is placed in the dictionary replacing 
    /// the existing template with the same name (if any)
    abstract member LoadTemplate: string -> (ITemplate * System.DateTime)
    
/// A tag implementation
and ITag = 
    ///<summary>
    /// Transforms a {% %} tag into a list of nodes and uncommited token list
    ///</summary>
    ///<param name="token">token for the tag name</param>
    ///<param name="context">the parsing context for the token</param>
    ///<param name="tokens">the remainder of the token list</param>
    ///<returns>
    /// a tuple consisting of the INodeImpl object representing the result of node parsing as the first element
    /// followed by the the remainder of the token list with all the token related to the node removed
    ///</returns>
    abstract member Perform: Lexer.BlockToken -> ParsingContext -> LazyList<Lexer.Token> -> (INodeImpl * LazyList<Lexer.Token>)

/// Parsing context is a container for information specific to the tag being parsed
and ParsingContext(provider: ITemplateManagerProvider, extra_tags: string list) =
    
    /// List (sequence) of all registered tag names. Includes all registered tags as well as 
    member x.Tags = provider.Tags |> Map.toSeq |> Seq.map (fun tag -> tag |> fst) 

    /// a list (sequence) of all closing tags for the context
    member x.TagClosures = Seq.ofList extra_tags                    
   
   /// Parent provider owning the context
    member x.Provider = provider
   
   /// Parent provider owning the context
    member x.Filters = provider.Filters |> Map.toSeq |> Seq.map (fun filter -> filter |> fst)
    
/// A representation of a node of the template abstract syntax tree    
type INode =

    /// TagNode type
    abstract member NodeType: NodeType 
    
    /// Position of the first character of the node text
    abstract member Position: int
    
    /// Length of the node text
    abstract member Length: int

    /// a list of values allowed for the node
    abstract member Values: IEnumerable<string>
    
    /// message associated with the node
    abstract member ErrorMessage: Error
    
    /// TagNode description (will be shown in the tooltip)
    abstract member Description: string
    
    /// node lists
    abstract member Nodes: IDictionary<string, IEnumerable<INode>>

/// This exception is thrown if a problem encountered while rendering the template
/// This exception will be later caught in the ASTWalker and re-thrown as the 
/// RenderingException
type RenderingError (message: string, ?innerException: exn) =
        inherit System.ApplicationException(message, defaultArg innerException null)
        
/// The actaual redering exception. The original RenderingError exceptions are caught and re-thrown
/// as RenderingExceptions
type RenderingException (message: string, token:NDjango.Lexer.Token, ?innerException: exn) =
        inherit System.ApplicationException(message + token.DiagInfo, defaultArg innerException null)
       
/// Exception raised when template syntax errors are encountered
/// this exception is defined here because it its dependency on the TextToken class
type SyntaxException (message: string, token: NDjango.Lexer.Token) =
    inherit System.ApplicationException(message + token.DiagInfo)
    member x.Token = token
    member x.ErrorMessage = message  

/// This esception is thrown if a problem encountered while parsing the template
/// This exception will be later caught and re-thrown as the SyntaxException
type SyntaxError (message, nodes: seq<INodeImpl> option, pattern:INode list option, remaining: LazyList<NDjango.Lexer.Token> option) = 
    inherit System.ApplicationException(message)
    new (message) = new SyntaxError(message, None, None, None)

    /// constructor to be used when the error applies to 
    /// multiple tags i.e. missing closing tag exception. Inculdes node list as an
    /// additional parameter 
    new (message, nodes) = new SyntaxError(message, Some nodes, None, None)

    ///constructor to be used when it is necessary 
    ///to include nodes and remaining tokens to SyntaxError.
    new (message, nodes, remaining) = new SyntaxError(message, Some nodes, None, Some remaining)

    new (message, remaining) = new SyntaxError(message, None, None, Some remaining)
    
    /// constructor to be used when the error applies to a partially parsed tag 
    /// Inculdes a list of tag elements to be associated with the error
    new (message, pattern) = new SyntaxError(message, None, Some pattern, None)
    
    new (message, pattern, remaining) = new SyntaxError(message, None, Some pattern, Some remaining)
    
    /// list (sequence) of nodes related to the error
    member x.Nodes = match nodes with | Some n -> n | None -> seq []
    
    /// list of tag elements from the partially parsed tag
    member x.Pattern = match pattern with | Some p -> p | None -> []
    
    member x.Remaining = remaining

/// Tags and/or filters marked with this attribute will be registered under the name
/// supplied by the attribute unless the name will be provided explicitly during the registartion
type NameAttribute(name:string) = 
    inherit System.Attribute() 
    member x.Name = name