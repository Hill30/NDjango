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

module internal Constants =

    /// escapes the regular expression-specific elements of the parameter. !!parm == Regex.Escape(parm)
    let (!!) parm = Regex.Escape(parm)

    // template syntax constants
    let FILTER_SEPARATOR = "|"
    let FILTER_ARGUMENT_SEPARATOR = ":"
    let VARIABLE_ATTRIBUTE_SEPARATOR = "."
    let BLOCK_TAG_START = "{%"
    let BLOCK_TAG_END = "%}"
    let VARIABLE_TAG_START = "{{"
    let VARIABLE_TAG_END = "}}"
    let COMMENT_TAG_START = "{#"
    let COMMENT_TAG_END = "#}"
    let SINGLE_BRACE_START = "{"
    let SINGLE_BRACE_END = "}"
    let I18N_OPEN = "_("
    let I18N_CLOSE = ")"

    //let ALLOWED_VARIABLE_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_."

    // what to report as the origin for templates that come from non-loader sources
    // (e.g. strings)
    let UNKNOWN_SOURCE = "&lt;unknown source&gt;"

    let tag_re = 
        new Regex(
            "(" + !!BLOCK_TAG_START + ".*?" + !!BLOCK_TAG_END + "|"
             + !!VARIABLE_TAG_START + ".*?" + !!VARIABLE_TAG_END + "|"
              + !!COMMENT_TAG_START + ".*?" + !!COMMENT_TAG_END + ")",
            RegexOptions.Compiled)

    let word_split_re = new Regex("\s+", RegexOptions.Compiled)
            
  
    // This expression was modified from the original python version
    // to allow $ as the first character of variable names. Such names will 
    // be used for internal purposes          
    let filter_raw_string =
        @"
        ^(?P<var>%(i18n_open)s`%(str)s`%(i18n_close)s|
                 `%(str)s`|
                 '%(strs)s'|
                 \$?[%(var_chars)s]+)|
         (?:%(filter_sep)s
             (?P<filter_name>\w+)
                 (?:%(arg_sep)s
                     (?:
                      (?P<arg>%(i18n_open)s`%(str)s`%(i18n_close)s|
                              `%(str)s`|
                              '%(strs)s'|
                              [%(var_chars)s]+)
                     )
                 )?
         )"
    (*
    let filter_raw_string =
        @"
        ^%(i18n_open)s`(?P<i18n_constant>%(str)s)`%(i18n_close)s|
        ^`(?P<constant>%(str)s)`|
        ^(?P<var>[%(var_chars)s]+)|
         (?:%(filter_sep)s
             (?P<filter_name>\w+)
                 (?:%(arg_sep)s
                     (?:
                      %(i18n_open)s`(?P<i18n_arg>%(str)s)`%(i18n_close)s|
                      `(?P<constant_arg>%(str)s)`|
                      (?P<var_arg>[%(var_chars)s]+)
                     )
                 )?
         )"
    *)
     
    let filter_re =
        new Regex(
            filter_raw_string.
                Replace("`", "\"").
                Replace("\r", "").
                Replace("\n", "").
                Replace("\t", "").
                Replace(" ", "").
                Replace("?P<", "?'"). // keep the source regex in Python format for better source code comparability
                Replace(">", "'"). // ditto [^"\]*
                Replace("%(str)s", "[^\"\\\\]*(?:\\\\.[^\"\\\\]*)*").
                Replace("%(strs)s", "[^'\\\\]*(?:\\\\.[^'\\\\]*)*").
                Replace("%(var_chars)s", @"\w\.-"). // apparently the python \w also captures the (-) sign, whereas the .net doesn't. including this here means we also need to check for it in Variable as an illegal first character
                Replace("%(filter_sep)s", "(\s*)" + !!FILTER_SEPARATOR + "(\s*)").
                Replace("%(arg_sep)s", !!FILTER_ARGUMENT_SEPARATOR).
                Replace("%(i18n_open)s", !!I18N_OPEN).
                Replace("%(i18n_close)s", !!I18N_CLOSE), RegexOptions.Compiled)

   