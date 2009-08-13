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

    type TextToken(text:string, pos:int, line:int, linePos:int) =
        member x.Text = text
        member x.Position = pos
        member x.Line = line
        member x.LinePos = linePos
        member x.Length = text.Length
        override x.ToString() = sprintf " in token: \"%s\" at line %d pos %d " text line linePos

    /// Exception raised when template syntax errors are encountered
    and SyntaxErrorException (message: string, token: TextToken) =
        inherit System.ApplicationException(message + token.ToString())
        
        member x.Token = token
        member x.ErrorMessage = message  
    
    type BlockToken(text, pos, line, linePos) =
        inherit TextToken(text, pos, line, linePos)
        let verb, args = 
            match smart_split (text.[Constants.BLOCK_TAG_START.Length..text.Length-Constants.BLOCK_TAG_END.Length-1].Trim()) with
            | verb::args -> verb, args 
            | _ -> raise (SyntaxError("Empty tag block"))
        member x.Verb = verb 
        member x.Args = args
    
    type ErrorToken(text, error:string, pos, line, linePos) =
        inherit TextToken(text, pos, line, linePos)
          
        member x.ErrorMessage = error
    
    type VariableToken(text:string, pos, line, linePos) =
        inherit TextToken(text, pos, line, linePos)
        let expression = text.[Constants.VARIABLE_TAG_START.Length..text.Length-Constants.VARIABLE_TAG_END.Length-1].Trim()
            
        member this.Expression = 
            if expression.Equals("") then
                raise (SyntaxError("Empty variable block"))
            expression 
    
    type CommentToken(text, pos, line, linePos) =
        inherit TextToken(text, pos, line, linePos) 
    
    /// A lexer token produced through the tokenize function
    type Token =
        | Block of BlockToken
        | Variable of VariableToken
        | Comment of CommentToken
        | Error of ErrorToken
        | Text of TextToken
        
    let get_textToken = function
    | Block b -> b :> TextToken
    | Error e -> e :> TextToken
    | Variable v -> v :> TextToken
    | Comment c -> c :> TextToken
    | Text t -> t
        
    /// <summary> generates sequence of tokens out of template TextReader </summary>
    /// <remarks>the type implements the IEnumerable interface (sequence) of templates
    /// It reads the template text from the text reader one buffer at a time and 
    /// returns tokens in batches - a batch is a sequence of the tokens generated 
    /// off the content of the buffer </remarks>
    type private Tokenizer (template:TextReader) =
        let mutable current: Token list = []
        let mutable line = 0
        let mutable pos = 0
        let mutable linePos = 0
        let mutable tail = ""
        let buffer = Array.create 4096 ' '
        
        let create_token in_tag text = 
            in_tag := not !in_tag
            let currentPos = pos
            let currentLine = line
            let currentLinePos = linePos
            Seq.iter 
                (fun ch -> 
                    pos <- pos + 1
                    if ch = '\n' then 
                        line <- line + 1 
                        linePos <- 0
                    else linePos <- linePos + 1
                    ) 
                text
            if not !in_tag then
                Text(new TextToken(text, currentPos, currentLine, currentLinePos))
            else
                try
                    match text.[0..1] with
                    | "{{" -> Variable (new VariableToken(text, currentPos, currentLine, currentLinePos))
                    | "{%" -> Block (new BlockToken(text, currentPos, currentLine, currentLinePos))
                    | "{#" -> Comment (new CommentToken(text, currentPos, currentLine, currentLinePos))
                    | _ -> Text (new TextToken(text, currentPos, currentLine, currentLinePos))
                with
                | :? SyntaxError as ex -> 
                    Error (new ErrorToken(text, ex.Message, currentPos, currentLine, currentLinePos))
                | _ -> rethrow()
        
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
