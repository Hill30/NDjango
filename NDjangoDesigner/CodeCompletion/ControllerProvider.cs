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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Editor;

namespace NDjango.Designer.CodeCompletion
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("NDjango Completion Controller")]
    [Order(Before = "Default Completion Controller")]
    [ContentType(Constants.NDJANGO)]
    internal class ControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        [Import]
        internal INodeProviderBroker nodeProviderBroker { get; set; }

        [Import]
        internal IVsEditorAdaptersFactoryService adaptersFactory { get; set; }
        
        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            foreach (ITextBuffer buffer in subjectBuffers)
                if (nodeProviderBroker.IsNDjango(buffer))
                    return new Controller(this, buffer, textView);
            return null;
        }
    }
}
