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
open System.Text.RegularExpressions

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.ParserNodes
open NDjango.ASTNodes
open NDjango.OutputHandling
open NDjango.Expressions

module internal Misc =

    /// Force autoescape behavior for this block
    type AutoescapeTag() =
        interface ITag with
            member this.Perform token provider tokens =

                let nodelist, remaining = (provider :?> IParser).Parse (Some token) tokens ["endautoescape"]

                let createContext walker = 
                    match token.Args with 
                    | "on"::[] -> walker.context.WithAutoescape(true)
                    | "off"::[] -> walker.context.WithAutoescape(false)
                    | _ -> raise (SyntaxError("invalid arguments for 'Autoescape' tag"))
                    
                (({
                    new TagNode(provider, token) with
                        override this.walk manager walker = 
                            {walker with 
                                parent=Some walker; 
                                context = createContext walker; 
                                nodes=nodelist}
                        override this.nodes with get() = nodelist
                   } :> INodeImpl), 
                   remaining)
                        
    /// Ignores everything between ``{% comment %}`` and ``{% endcomment %}``.
    type CommentTag() =
        interface ITag with
            member this.Perform token provider tokens =
                let remaining = (provider :?> IParser).Seek tokens ["endcomment"]
                ((TagNode(provider, token) :> INodeImpl), remaining)
                
    /// Outputs a whole load of debugging information, including the current
    /// context and imported modules.
    /// 
    /// Sample usage::
    /// 
    ///     <pre>
    ///         {% debug %}
    ///     </pre>
    type DebugTag() =
        interface ITag with
            member this.Perform token provider tokens = (new NDjango.Tags.Debug.TagNode(provider, token) :> INodeImpl), tokens
            
    /// Outputs the first variable passed that is not False.
    /// 
    /// Outputs nothing if all the passed variables are False.
    /// 
    /// Sample usage::
    /// 
    ///     {% firstof var1 var2 var3 %}
    /// 
    /// This is equivalent to::
    /// 
    ///     {% if var1 %}
    ///         {{ var1 }}
    ///     {% else %}{% if var2 %}
    ///         {{ var2 }}
    ///     {% else %}{% if var3 %}
    ///         {{ var3 }}
    ///     {% endif %}{% endif %}{% endif %}
    /// 
    /// but obviously much cleaner!
    /// 
    /// You can also use a literal string as a fallback value in case all
    /// passed variables are False::
    /// 
    ///     {% firstof var1 var2 var3 "fallback value" %}
    type FirstOfTag() =
        interface ITag with
            member this.Perform token provider tokens =
                match token.Args with
                    | [] -> raise (SyntaxError ("'firstof' tag requires at least one argument"))
                    | _ -> 
                        let variables = token.Args |> List.map (fun (name) -> new FilterExpression(provider, Block token, name))
                        ({
                            new TagNode(provider, token)
                            with 
                                override this.walk manager walker =
                                    match variables |> List.tryPick (fun var -> var.ResolveForOutput manager walker ) with
                                    | None -> walker 
                                    | Some w -> w
                        } :> INodeImpl), tokens
                        
/// regroup¶
/// Regroup a list of alike objects by a common attribute.
/// 
/// This complex tag is best illustrated by use of an example: say that people is a list of people represented by dictionaries with first_name, last_name, and gender keys:
/// 
/// people = [
///     {'first_name': 'George', 'last_name': 'Bush', 'gender': 'Male'},
///     {'first_name': 'Bill', 'last_name': 'Clinton', 'gender': 'Male'},
///     {'first_name': 'Margaret', 'last_name': 'Thatcher', 'gender': 'Female'},
///     {'first_name': 'Condoleezza', 'last_name': 'Rice', 'gender': 'Female'},
///     {'first_name': 'Pat', 'last_name': 'Smith', 'gender': 'Unknown'},
/// ]
/// ...and you'd like to display a hierarchical list that is ordered by gender, like this:
/// 
/// 
/// Male: 
/// George Bush 
/// Bill Clinton 
/// 
/// Female: 
/// Margaret Thatcher 
/// Condoleezza Rice 
/// 
/// Unknown: 
/// Pat Smith 
/// You can use the {% regroup %} tag to group the list of people by gender. The following snippet of template code would accomplish this:
/// 
/// {% regroup people by gender as gender_list %}
/// 
/// <ul>
/// {% for gender in gender_list %}
///     <li>{{ gender.grouper }}
///     <ul>
///         {% for item in gender.list %}
///         <li>{{ item.first_name }} {{ item.last_name }}</li>
///         {% endfor %}
///     </ul>
///     </li>
/// {% endfor %}
/// </ul>
/// Let's walk through this example. {% regroup %} takes three arguments: the list you want to regroup, the attribute to group by, and the name of the resulting list. Here, we're regrouping the people list by the gender attribute and calling the result gender_list.
/// 
/// {% regroup %} produces a list (in this case, gender_list) of group objects. Each group object has two attributes:
/// 
/// grouper -- the item that was grouped by (e.g., the string "Male" or "Female"). 
/// list -- a list of all items in this group (e.g., a list of all people with gender='Male'). 
/// Note that {% regroup %} does not order its input! Our example relies on the fact that the people list was ordered by gender in the first place. If the people list did not order its members by gender, the regrouping would naively display more than one group for a single gender. For example, say the people list was set to this (note that the males are not grouped together):
/// 
/// people = [
///     {'first_name': 'Bill', 'last_name': 'Clinton', 'gender': 'Male'},
///     {'first_name': 'Pat', 'last_name': 'Smith', 'gender': 'Unknown'},
///     {'first_name': 'Margaret', 'last_name': 'Thatcher', 'gender': 'Female'},
///     {'first_name': 'George', 'last_name': 'Bush', 'gender': 'Male'},
///     {'first_name': 'Condoleezza', 'last_name': 'Rice', 'gender': 'Female'},
/// ]
/// With this input for people, the example {% regroup %} template code above would result in the following output:
/// 
/// 
/// Male: 
/// Bill Clinton 
/// 
/// Unknown: 
/// Pat Smith 
/// 
/// Female: 
/// Margaret Thatcher 
/// 
/// Male: 
/// George Bush 
/// 
/// Female: 
/// Condoleezza Rice 
/// The easiest solution to this gotcha is to make sure in your view code that the data is ordered according to how you want to display it.
/// 
/// Another solution is to sort the data in the template using the dictsort filter, if your data is in a list of dictionaries:
/// 
/// {% regroup people|dictsort:"gender" by gender as gender_list %}

    type Grouper =
        { 
            grouper: obj
            list: obj list
        }
        member this.Append(o) = {this with list=this.list @ o}

    type RegroupTag() =
        interface ITag with
            member this.Perform token provider tokens =
                match token.Args with
                | source::"by"::grouper::"as"::result::[] ->
                    let value = new FilterExpression(provider, Block token, source)
                    let regroup context =
                        match fst <| value.Resolve context false with
                        | Some o ->
                            match o with
                            | :? System.Collections.IEnumerable as loop -> 
                                let groupers = 
                                    loop |> Seq.cast |> 
                                        Seq.fold 
                                            // this function takes a tuple with the first element representing the grouper
                                            // currently under construction and the second the list of groupers built so far
                                            (fun (groupers:Grouper option*Grouper list) item -> 
                                                match resolve_lookup item [grouper] with
                                                | Some value ->  // this is the current value to group by
                                                    match fst groupers with
                                                    | Some group -> // group is a group currently being built
                                                        if value = group.grouper then
                                                            (Some {group with list=group.list @ [item]}, snd groupers)
                                                        else
                                                            (Some {grouper=value; list=[item]}, snd groupers@[group])
                                                    | None -> (Some {grouper=value; list=[item]}, []) // No group - we are just starting
                                                | None -> groupers
                                                )
                                             
                                            (None, [])  // start expression for seq.fold
                                match fst groupers with
                                | None -> []
                                | Some grouper -> snd groupers @ [grouper]
                            | _ -> []
                        | None -> []
                    ({
                        new TagNode(provider, token)
                        with 
                            override this.walk manager walker =
                                match regroup walker.context with
                                | [] -> walker
                                | _ as list -> {walker with context=walker.context.add(result, (list :> obj))}
                    } :> INodeImpl), tokens

                | _ -> raise (SyntaxError ("malformed 'regroup' tag"))

/// spaceless¶
/// Removes whitespace between HTML tags. This includes tab characters and newlines.
/// 
/// Example usage:
/// 
/// {% spaceless %}
///     <p>
///         <a href="foo/">Foo</a>
///     </p>
/// {% endspaceless %}
/// This example would return this HTML:
/// 
/// <p><a href="foo/">Foo</a></p>
/// Only space between tags is removed -- not space between tags and text. In this example, the space around Hello won't be stripped:
/// 
/// {% spaceless %}
///     <strong>
///         Hello
///     </strong>
/// {% endspaceless %}

    type SpacelessTag() =
        let spaces_re = new Regex("(?'spaces'>\s+<)", RegexOptions.Compiled)
        interface ITag with
            member this.Perform token provider tokens =
                match token.Args with
                | [] ->
                    let node_list, remaining = (provider :?> IParser).Parse (Some token) tokens ["endspaceless"]
                    ({
                        new TagNode(provider, token)
                        with 
                            override this.walk manager walker =
                                let reader = 
                                    new NDjango.ASTWalker.Reader(manager,{walker with parent=None; nodes=node_list; context=walker.context})
                                let buf = spaces_re.Replace(reader.ReadToEnd(), "><")
                                {walker with buffer = buf}
                            override this.nodes with get() = node_list
                    } :> INodeImpl), remaining

                | _ -> raise (SyntaxError ("'spaceless' tag should not have any arguments"))
                
                
    ///Output one of the syntax characters used to compose template tags.
    ///
    ///Since the template system has no concept of "escaping", to display one of the bits used in template tags, you must use the {% templatetag %} tag.
    ///
    ///The argument tells which template bit to output:
    ///
    ///Argument Outputs 
    ///openblock {% 
    ///closeblock %} 
    ///openvariable {{ 
    ///closevariable }} 
    ///openbrace { 
    ///closebrace } 
    ///opencomment {# 
    ///closecomment #} 

    type TemplateTag() =
        interface ITag with
            member this.Perform token provider tokens =
                let buf = 
                    match token.Args with
                        | "openblock"::[] -> "{%"
                        | "closeblock"::[] -> "%}"
                        | "openvariable"::[] -> "{{"
                        | "closevariable"::[] -> "}}"
                        | "openbrace"::[] -> "{"
                        | "closebrace"::[] -> "}"
                        | "opencomment"::[] -> "{#"
                        | "closecomment"::[] -> "#}"
                        | _ -> raise (SyntaxError ("invalid format for 'template' tag"))
                let variables = token.Args |> List.map (fun (name) -> new FilterExpression(provider, Block token, name))
                ({
                    new TagNode(provider, token)
                    with 
                        override this.walk manager walker =
                            {walker with buffer = buf}
                } :> INodeImpl), tokens


    /// For creating bar charts and such, this tag calculates the ratio of a given
    /// value to a maximum value, and then applies that ratio to a constant.
    ///
    /// For example::
    ///
    ///     <img src='bar.gif' height='10' width='{% widthratio this_value max_value 100 %}' />
    ///
    /// Above, if ``this_value`` is 175 and ``max_value`` is 200, the the image in
    /// the above example will be 88 pixels wide (because 175/200 = .875;
    /// 
    type WidthRatioTag() =
        interface ITag with
            member this.Perform token provider tokens =

                let toFloat (v:obj option) =
                    match v with 
                    | None -> raise (SyntaxError ("'widthratio' cannot convert empty value to a numeric"))
                    | Some value ->
                        try 
                            System.Convert.ToDouble(value)
                        with |_ -> raise (SyntaxError (sprintf "'widthratio' cannot convert value %s to a numeric" (System.Convert.ToString(value))))
        
                match token.Args with
                | value::maxValue::maxWidth::[] ->
                    let value = new FilterExpression(provider, Block token, value)
                    let maxValue = new FilterExpression(provider, Block token, maxValue)
                    let width = try System.Int32.Parse(maxWidth) |> float with | _  -> raise (SyntaxError ("'widthratio' 3rd argument must be integer"))
                    (({
                        new TagNode(provider, token) with
                            override this.walk manager walker = 
                                let ratio = toFloat (fst <| value.Resolve walker.context false)
                                            / toFloat (fst <| maxValue.Resolve walker.context false) 
                                            * width + 0.5
                                {walker with buffer = ratio |> int |> string}
                       } :> INodeImpl), 
                       tokens)
                | _ -> raise (SyntaxError ("'widthratio' takes three arguments"))

    /// Adds a value to the context (inside of this block) for caching and easy
    /// access.
    ///
    /// For example::
    ///
    ///     {% with person.some_sql_method as total %}
    ///         {{ total }} object{{ total|pluralize }}
    ///     {% endwith %}
    type WithTag() =
        interface ITag with
            member this.Perform token provider tokens =
                match token.Args with
                | var::"as"::name::[] ->
                    let nodes, remaining = (provider :?> IParser).Parse (Some token) tokens ["endwith"]
                    let expression = new FilterExpression(provider, Block token, var)
                    (({
                        new TagNode(provider, token) with
                            override this.walk manager walker = 
                                let context = 
                                    match fst <| expression.Resolve walker.context false with
                                    | Some v -> walker.context.add(name, v)
                                    | None -> walker.context
                                {walker with 
                                    parent=Some walker; 
                                    context = context; 
                                    nodes=nodes}
                            override this.nodes with get() = nodes
                       } :> INodeImpl), 
                       remaining)
                | _ -> raise (SyntaxError ("'with' expected format is 'value as name'"))

module Abstract =
    /// Returns an absolute URL matching given view with its parameters.
    ///
    /// This is an adaptation of the standard django URL tag. Since this implementation
    /// isn't tied to a particular controller implementation, the structure of the parameters
    /// cannot be defined here, and is delegated to a consuming integration project.
    /// 
    /// Supporting integration projects need to subclass the UrlTag class, and implement
    /// the 'string GenerateUrl(string, string[])' method to perform the necessary logic.
    /// This tag is not registered by default as the default implementation is abstract.
    [<AbstractClass>]    
    type UrlTag() =
        let rec parseArgs token parser args = 
            let instantiate arg = [new FilterExpression(parser, Block token, String.trim [' '] arg)]
            match args with
            | arg::"as"::name::[] -> instantiate arg, (Some name)
            | arg::[] -> instantiate arg, None
            | arg::tail ->  
                let list, var = parseArgs token parser tail
                instantiate arg @ list, var
            | _ -> [], None
    
        abstract member GenerateUrl: string * string array * IContext -> string
        
        interface ITag with 
            member this.Perform token provider tokens =
                let path, argList, var =
                    match token.Args with
                    | [] -> raise (SyntaxError ("'url' tag requires at least one parameter"))
                    | path::args -> 
                        let argList, var = parseArgs token provider (List.fold (fun state elem -> 
                                                                                                    match String.trim [','] elem with
                                                                                                    | "" -> state | _ as trimmed -> trimmed::state ) [] <| List.rev args)
                        new FilterExpression(provider, Block token, path), argList, var
                
                (({
                    new TagNode(provider, token) with
                        override x.walk manager walker =
                            let shortResolve (expr: FilterExpression) = 
                                match fst <| expr.Resolve walker.context false with
                                | Some v -> Convert.ToString(v)
                                | None -> System.String.Empty

                            let url = this.GenerateUrl((shortResolve path), List.to_array <| List.map (fun (elem: FilterExpression) -> shortResolve elem) argList, walker.context)
                            match var with 
                            | None -> { walker with buffer = url }
                            | Some v -> { walker with context = walker.context.add(v, (url :> obj)) }
                            } :> INodeImpl),
                    tokens)
