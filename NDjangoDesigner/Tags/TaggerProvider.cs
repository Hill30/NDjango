/****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Designer.
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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Tags
{
    /// <summary>
    /// Provides tags for text buffrers
    /// </summary>
    /// <remarks> Imports parser object in order to generate tokenizer for tagger</remarks>
    [Export(typeof(ITaggerProvider))]
    [ContentType(Constants.NDJANGO)]
    [TagType(typeof(SquiggleTag))]
    class TaggerProvider : ITaggerProvider
    {
        [Import]
        internal INodeProviderBroker nodeProviderBroker { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer, IEnvironment context) where T : ITag
        {
            if (nodeProviderBroker.IsNDjango(buffer, context))
                return (ITagger<T>)new Tagger(nodeProviderBroker, buffer);
            else
                return null;
        }

    }
}
