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

open System
open System.IO
open System.Collections
open System.Collections.Generic

open NDjango.OutputHandling

module Lexer =

    type TextToken(text:string, line:int, pos:int) =
        member this.Text = text
        override this.ToString() = sprintf " in token: \"%s\" at line %d pos %d " text line pos

    type BlockToken(text, line, pos) =
        inherit TextToken(text, line, pos)
        let verb, args = 
            match smart_split (text.[Constants.BLOCK_TAG_START.Length..text.Length-Constants.BLOCK_TAG_END.Length-1].Trim()) with
            | verb::args -> verb, args 
            | _ -> raise (TemplateSyntaxError ("Empty tag block", Some (TextToken(text, line, pos):>obj)))
        member this.Verb = verb 
        member this.Args = args
    
    type VariableToken(text:string, line, pos) =
        inherit TextToken(text, line, pos)
        let expression = text.[Constants.VARIABLE_TAG_START.Length..text.Length-Constants.VARIABLE_TAG_END.Length-1].Trim()
            
        member this.Expression = 
            if expression.Equals("") then
                raise (TemplateSyntaxError ("Empty variable block", Some (TextToken(text, line, pos):>obj)))
            expression 
    
    type CommentToken(text, line, pos) =
        inherit TextToken(text, line, pos) 
    
    /// A lexer token produced through the tokenize function
    type Token =
        | Block of BlockToken
        | Variable of VariableToken
        | Comment of CommentToken
        | Text of TextToken
        
    let get_textToken = function
    | Block b -> Some (b :> obj)
    | Variable v -> Some (v :> obj)
    | Comment c -> Some (c :> obj)
    | Text t -> Some (t :> obj)
        
    /// <summary> generates sequence of tokens out of template TextReader </summary>
    /// <remarks>the type implements the IEnumerable interface (sequence) of templates
    /// It reads the template text from the text reader one buffer at a time and 
    /// returns tokens in batches - a batch is a sequence of the tokens generated 
    /// off the content of the buffer </remarks>
    type private Tokenizer (template:TextReader) =
        let mutable current: Token list = []
        let mutable line = 0
        let mutable pos = 0
        let mutable tail = ""
        let buffer = Array.create 4096 ' '
        
        let create_token in_tag text = 
            in_tag := not !in_tag
            let currentLine = line
            let currentPos = pos
            Seq.iter 
                (fun ch -> 
                    if ch = '\n' then 
                        line <- line + 1 
                        pos <- 0
                    else pos <- pos + 1
                    ) 
                text
            if not !in_tag then
                Text(new TextToken(text, currentLine, currentPos))
            else
                match text.[0..1] with
                | "{{" -> Variable (new VariableToken(text, currentLine, currentPos))
                | "{%" -> Block (new BlockToken(text, currentLine, currentPos))
                | "{#" -> Comment (new CommentToken(text, currentLine, currentPos))
                | _ -> Text (new TextToken(text, currentLine, currentPos))
        
        interface IEnumerator<Token seq> with
            member this.Current = Seq.of_list current
        
        interface IEnumerator with
            member this.Current = Seq.of_list current :> obj
            
            member this.MoveNext() =
                match tail with
                | null -> 
                    false
                | _ -> 
                    let count = template.ReadBlock(buffer, 0, buffer.Length)
                    let strings = (Constants.tag_re.Split(tail + new String(buffer, 0, count)))
                    let t, strings = 
                        if (count > 0) then strings.[strings.Length-1], strings.[0..strings.Length - 2]
                        else null, strings
                    
                    tail <- t
                    let in_tag = ref true
                    current <- strings |> List.of_array |> List.map (create_token in_tag)
                    true
                
            member this.Reset() = failwith "Reset is not supported by Tokenizer"
        
        interface IEnumerable<Token seq> with
            member this.GetEnumerator():IEnumerator<Token seq> =
                 this :> IEnumerator<Token seq>

        interface IEnumerable with
            member this.GetEnumerator():IEnumerator =
                this :> IEnumerator

        interface IDisposable with
            member this.Dispose() = ()

    /// Produces a sequence of token objects based on the template text
    let internal tokenize (template:TextReader) =
        LazyList.of_seq <| Seq.fold (fun (s:Token seq) (item:Token seq) -> Seq.append s item) (seq []) (new Tokenizer(template))
