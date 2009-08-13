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
open System.Text.RegularExpressions

module OutputHandling =

    /// Matches strings that start with the pattern
    let internal (|StartsWith|_|) (pattern: string) (v: string) = if v.StartsWith(pattern) then Some v else None

    /// Matches strings that end with the pattern
    let internal (|EndsWith|_|) (pattern: string) (v: string) = if v.EndsWith(pattern) then Some v else None

    /// Matches strings that contain the pattern
    let internal (|Contains|_|) (pattern: string) (v: string) = if v.Contains(pattern) then Some v else None

    let smart_split_re = new Regex(@"(""(?:[^""\\]*(?:\\.[^""\\]*)*)""|'(?:[^'\\]*(?:\\.[^'\\]*)*)'|[^\s]+)", RegexOptions.Compiled)

    /// Generator that splits a string by spaces, leaving quoted phrases together.
    /// Supports both single and double quotes, and supports escaping quotes with
    /// backslashes. In the output, strings will keep their initial and trailing
    /// quote marks.
    /// 
    /// >>> list(smart_split(r'This is "a person\'s" test.'))
    /// [u'This', u'is', u'"a person\\\'s"', u'test.']
    /// >>> list(smart_split(r"Another 'person\'s' test.")) 
    /// [u'Another', u"'person's'", u'test.']
    /// >>> list(smart_split(r'A "\"funky\" style" test.')) 
    /// [u'A', u'""funky" style"', u'test.']
    let smart_split text = 
        [for m in smart_split_re.Matches(text) -> 
            let bit = m.Groups.[0].Value
            if bit.[0] = '"' && bit.[bit.Length-1] = '"' then
                "\"" + bit.[1..bit.Length-2].Replace("\\\"", "\"").Replace("\\\\", "\\") + "\""
            elif bit.[0] = '\'' && bit.[bit.Length-1] = '\'' then
                "'" + bit.[1..bit.Length-2].Replace("\\'", "'").Replace("\\\\", "\\") + "'"
            else
              bit
        ]
        
    /// smart-splits the token, also keeping intact requests for translation, e.g.
    ///
    /// >>> list(split_token_contents(r'This is _("a person\'s" test.)'))
    /// [u'This', u'is', u'_("a person\\\'s", test.)']
    /// >>> list(split_token_contents(r"Another 'person\'s' test.")) 
    /// [u'Another', u"'person's'", u'test.']
    /// >>> list(split_token_contents(r'A "\"funky\" style" test.')) 
    /// [u'A', u'""funky" style"', u'test.']
    let split_token_contents token =
        let join_token_split = fun (acc: string list * string option) elem ->
            let lst, sentinel = acc
            match sentinel with
            | Some s ->
                match elem with
                | EndsWith s v -> ([(List.hd lst) + " " + elem] @ (List.tl lst), None)
                | _ -> ([(List.hd lst) + " " + elem] @ (List.tl lst), Some s)
            | None ->
                match elem with
                // you can have the scenario when v is a single token, so it will contain
                // both the _( and the )
                | StartsWith "_(\"" v | StartsWith "_('" v when not (v.EndsWith(v.[2].ToString() + ")")) -> 
                    ([elem] @ lst, Some (v.[2].ToString() + ")"))
                | _ -> ([elem] @ lst, None)

        fst <| List.fold join_token_split ([], None) (smart_split token) |> List.rev
        
    /// determines whether the given string is either single or double quoted, escaped or un-escaped.
    /// returnes whether it is quoted, and the number of characters the quote string occupies
    let internal is_quoted (text: string) = 
        if text.Length < 2 then (false, 0)
        else
            match text.[0] with
            | '\\' when ("\"'".Contains(string text.[1]) && text.[..1] = text.[text.Length-2..]) -> (true, 2)
            | '\'' | '"' when text.[0] = text.[text.Length-1] -> (true, 1)
            | _ -> (false, 0)
        
    /// determines whether the given string is marked as requiring translation
    let internal is_i18n (text: string) = 
        if text.Length < 3 then false
        else
            match text.[..1] with
            | "_(" -> true
            | _ -> false
            
    let internal unescape_quotes (text: string) = text.Replace("\\'", "'").Replace("\\\"", "\"")
            
    /// strips off translation and outer quote characters. returns the stripped string,
    /// was the string quoted and was it marked for internationalization
    let internal strip_markers text = 
        let v, was_i18n = if is_i18n text then text.[2..text.Length-2], true else text, false
        let quoted, count = is_quoted v
        
        if quoted then
            (unescape_quotes v.[count..v.Length-2*count]), quoted, was_i18n
        else
            (unescape_quotes v), quoted, was_i18n

    /// escapes html sensitive elements from the string
    let internal escape text = 
        (string text).Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;").Replace("'","&#39;").Replace("\"","&quot;")    

    let internal django_ns = "__ndjango__variable:"

    /// Produces a variable name placed into the django variable namespace
    let internal django_var var = django_ns + var

    /// used to set defaults for optional parameters. retunrs o.Value if o.IsSome, v otherwise
    let internal defaultArg o v = match o with | Some o -> o | _ -> v

//    /// extends a template syntax error message with token position information, if any is present
//    let private extend_syntax_error_message msg position length = 
//        match position with 
//        | None -> msg
//        | Some v ->
//            sprintf "%s (at %s" msg ( sprintf "%d%s" v (match length with | None -> ")" | Some l -> sprintf "/%d)" l))


    /// Error message
    type Error(severity:int, message:string) =
        /// indicates the severity of the error with 0 being the information message
        /// negative severity is used to mark a dummy message ("No messages" message) 
        member x.Severity = severity
        member x.Message = message

    /// Exception raised when a template syntax error is encountered
    type SyntaxError (message) = 
        inherit System.ApplicationException(message)
    

