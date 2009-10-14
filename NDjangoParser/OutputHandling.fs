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

    /// <summary>
    /// determines whether the given string is either single or double quoted, escaped or un-escaped.
    /// returnes whether it is quoted, and the number of characters the quote string occupies
    /// </summary>
    let internal is_quoted (text: string) = 
        if text.Length < 2 then (false, 0)
        else
            match text.[0] with
            | '\\' when ("\"'".Contains(string text.[1]) && text.[..1] = text.[text.Length-2..]) -> (true, 2)
            | '\'' | '"' when text.[0] = text.[text.Length-1] -> (true, 1)
            | _ -> (false, 0)
        
    /// <summary>
    /// determines whether the given string is marked as requiring translation
    /// </summary>
    let internal is_i18n (text: string) = 
        if text.Length < 3 then false
        else
            match text.[..1] with
            | "_(" -> true
            | _ -> false
            
    let internal unescape_quotes (text: string) = text.Replace("\\'", "'").Replace("\\\"", "\"")
            
    /// <summary>
    /// strips off translation and outer quote characters. returns the stripped string,
    /// was the string quoted and was it marked for internationalization
    /// </summary>
    let internal strip_markers (text:string) = 
        let v, was_i18n = if is_i18n text then text.[2..text.Length-2], true else text, false
        let quoted, count = is_quoted v

        if quoted 
        then None, Some (unescape_quotes v.[count..v.Length-2*count] :> obj), was_i18n
        else Some v, None , was_i18n

    /// <summary>
    /// escapes html sensitive elements from the string
    /// </summary>
    let internal escape text = 
        (string text).Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;").Replace("'","&#39;").Replace("\"","&quot;")    

    /// used to set defaults for optional parameters. retunrs o.Value if o.IsSome, v otherwise
    let internal defaultArg o v = match o with | Some o -> o | _ -> v

      
    

