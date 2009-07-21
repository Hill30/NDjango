/****************************************************************************
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
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace NDjango.FiltersCS
{
    /// <summary>
    ///     Class used to register filters.
    /// </summary>
    public class FilterManager
    {

        private static FilterManager instance = new FilterManager();
        public static FilterManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void Initialize()
        {
            NDjango.Template.Manager.RegisterFilter("add", new NDjango.FiltersCS.AddFilter());
            NDjango.Template.Manager.RegisterFilter("get_digit", new NDjango.FiltersCS.GetDigit());
            NDjango.Template.Manager.RegisterFilter("default", new NDjango.FiltersCS.DefaultFilter());
            NDjango.Template.Manager.RegisterFilter("divisibleby", new NDjango.FiltersCS.DivisibleByFilter());
            NDjango.Template.Manager.RegisterFilter("addslashes", new NDjango.FiltersCS.AddSlashesFilter());
            NDjango.Template.Manager.RegisterFilter("capfirst", new NDjango.FiltersCS.CapFirstFilter());
            NDjango.Template.Manager.RegisterFilter("escapejs", new NDjango.FiltersCS.EscapeJSFilter());
            NDjango.Template.Manager.RegisterFilter("fix_ampersands", new NDjango.FiltersCS.FixAmpersandsFilter());
            NDjango.Template.Manager.RegisterFilter("floatformat", new NDjango.FiltersCS.FloatFormatFilter());
            NDjango.Template.Manager.RegisterFilter("linenumbers", new NDjango.FiltersCS.LineNumbersFilter());
            NDjango.Template.Manager.RegisterFilter("lower", new NDjango.FiltersCS.LowerFilter());
            NDjango.Template.Manager.RegisterFilter("upper", new NDjango.FiltersCS.UpperFilter());
            NDjango.Template.Manager.RegisterFilter("make_list", new NDjango.FiltersCS.MakeListFilter());
            NDjango.Template.Manager.RegisterFilter("wordcount", new NDjango.FiltersCS.WordCountFilter());
            NDjango.Template.Manager.RegisterFilter("ljust", new NDjango.FiltersCS.LJustFilter());
            NDjango.Template.Manager.RegisterFilter("rjust", new NDjango.FiltersCS.RJustFilter());
            NDjango.Template.Manager.RegisterFilter("center", new NDjango.FiltersCS.CenterFilter());
            NDjango.Template.Manager.RegisterFilter("cut", new NDjango.FiltersCS.CutFilter());
            NDjango.Template.Manager.RegisterFilter("title", new NDjango.FiltersCS.TitleFilter());
            NDjango.Template.Manager.RegisterFilter("removetags", new NDjango.FiltersCS.RemoveTagsFilter());
            NDjango.Template.Manager.RegisterFilter("first", new NDjango.FiltersCS.FirstFilter());
            NDjango.Template.Manager.RegisterFilter("last", new NDjango.FiltersCS.LastFilter());
            NDjango.Template.Manager.RegisterFilter("length", new NDjango.FiltersCS.LengthFilter());
            NDjango.Template.Manager.RegisterFilter("length_is", new NDjango.FiltersCS.LengthIsFilter());
            NDjango.Template.Manager.RegisterFilter("random", new NDjango.FiltersCS.RandomFilter());
            NDjango.Template.Manager.RegisterFilter("slice", new NDjango.FiltersCS.SliceFilter());
        }
    }
}
