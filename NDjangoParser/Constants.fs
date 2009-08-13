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

module Constants =

    /// escapes the regular expression-specific elements of the parameter. !!parm == Regex.Escape(parm)
    let internal (!!) parm = Regex.Escape(parm)

    // template syntax constants
    let internal FILTER_SEPARATOR = "|"
    let internal FILTER_ARGUMENT_SEPARATOR = ":"
    let internal VARIABLE_ATTRIBUTE_SEPARATOR = "."
    let internal BLOCK_TAG_START = "{%"
    let internal BLOCK_TAG_END = "%}"
    let internal VARIABLE_TAG_START = "{{"
    let internal VARIABLE_TAG_END = "}}"
    let internal COMMENT_TAG_START = "{#"
    let internal COMMENT_TAG_END = "#}"
    let internal SINGLE_BRACE_START = "{"
    let internal SINGLE_BRACE_END = "}"
    let internal I18N_OPEN = "_("
    let internal I18N_CLOSE = ")"

    //let ALLOWED_VARIABLE_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_."

    // what to report as the origin for templates that come from non-loader sources
    // (e.g. strings)
    let internal UNKNOWN_SOURCE = "&lt;unknown source&gt;"

    let internal tag_re = 
        new Regex(
            "(" + !!BLOCK_TAG_START + ".*?" + !!BLOCK_TAG_END + "|"
             + !!VARIABLE_TAG_START + ".*?" + !!VARIABLE_TAG_END + "|"
              + !!COMMENT_TAG_START + ".*?" + !!COMMENT_TAG_END + ")",
            RegexOptions.Compiled)

    let internal word_split_re = new Regex("\s+", RegexOptions.Compiled)
            
  
    // This expression was modified from the original python version
    // to allow $ as the first character of variable names. Such names will 
    // be used for internal purposes          
    let internal filter_raw_string =
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
     
    let internal filter_re =
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


    /// Standard settings
    let DEFAULT_AUTOESCAPE = "settings.DEFAULT_AUTOESCAPE"
    let TEMPLATE_STRING_IF_INVALID = "settings.TEMPLATE_STRING_IF_INVALID"
    let RELOAD_IF_UPDATED = "settings.RELOAD_IF_UPDATED"
    let EXCEPTION_IF_ERROR = "settings.EXCEPTION_IF_ERROR"
    
    /// <summary>
    /// List nodes representing the elements of the tag itself, including 
    /// markers, tag name, tag paremeters, etc
    /// </summary>
    let NODELIST_TAG_ELEMENTS = "standard.elements";
    
    /// <summary>
    /// Stadard list of nodes representing child tags
    /// </summary>
    let NODELIST_TAG_CHILDREN = "standard.children";
    
    /// <summary>
    /// List of nodes representing the <b>true</b> branch of the if tag and similar tags
    /// </summary>
    let NODELIST_IFTAG_IFTRUE = "if.true.children";
    
    /// <summary>
    /// List of nodes representing the <b>false</b> branch of the if tag and similar tags
    /// </summary>
    let NODELIST_IFTAG_IFFALSE = "if.false.children";
