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
open System.Text.RegularExpressions
open System.Collections.Generic
open System.Globalization
open System

module public Utilities =

    /// valid format of an int    
    let internal int_re = new Regex(@"^-?\d+$", RegexOptions.Compiled)

    /// valid format of a float
    let internal float_re = new Regex(@"^-?\d+(?:\.\d+)?$", RegexOptions.Compiled)

    /// determines whether text is a valid integer
    let internal is_int text = int_re.IsMatch(text)

    /// determines whether text is a valid float
    let internal is_float text = float_re.IsMatch(text)
        
    /// Matches valid integers. if a match, returns Some int, otherwise returns None
    let internal (|Int|_|) text = if is_int text then Some (int text) else None

    /// Matches valid floats. if a match, returns Some float, otherwise returns None
    let internal (|Float|_|) text = if is_float text then Some (float text) else None

    /// Matches keys that are contained in the dictionary. If a match, returns Some dict[key], otherwise returns none
    let internal (|Contains|_|) key (dict: System.Collections.IDictionary) = 
        match key with
        | Some v when dict.Contains(v) -> Some dict.[v] 
        | _ -> None

    /// tries to parse out an integer from the string, returning None if unsuccessful
    let internal try_int = function
        | Int v -> Some v
        | _ -> None
        
    /// tries to parse out a float from the string, returning None is unsuccessful
    let internal try_float = function
        | Float v -> Some v
        | _ -> None
        
    let internal (|Matched|_|) (key: string) (re: System.Text.RegularExpressions.Match) = 
        if re.Groups.[key].Success then
            Some (re.Groups.[key].Value)
        else
            None
            
    // method to convert input value to Int32            
    let get_int (inputVal:obj) =
        let str = Convert.ToString inputVal
        let success,dblOut = Double.TryParse (str,NumberStyles.Any,CultureInfo.CurrentCulture)
        let inRange = ((Int32.MinValue |> Convert.ToDouble) < dblOut && dblOut < (Int32.MaxValue |> Convert.ToDouble))
        if (success && inRange) then
            true,(dblOut |> Convert.ToInt32)
        else
            false,0
            
    // method to convert input value to Double            
    let get_double (inputVal:obj) =
        let str = Convert.ToString inputVal
        Double.TryParse (str,NumberStyles.Any,CultureInfo.CurrentCulture)
        