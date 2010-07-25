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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text.Adornments;

namespace NDjango.Designer.Tags
{
    class Tagger : ITagger<ErrorTag>
    {
        private NodeProvider nodeProvider;

        public Tagger(INodeProviderBroker nodeProviderBroker, ITextBuffer buffer)
        {
            nodeProvider = nodeProviderBroker.GetNodeProvider(buffer);
            nodeProvider.NodesChanged += new NodeProvider.SnapshotEvent(provider_TagsChanged);
        }

        void provider_TagsChanged(SnapshotSpan snapshotSpan)
        {
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(snapshotSpan));
        }

        /// <summary>
        /// Gets a list of tags related to a span
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
        public IEnumerable<ITagSpan<ErrorTag>> GetTags(Microsoft.VisualStudio.Text.NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan span in spans)
            {
                foreach (DesignerNode node in nodeProvider.GetNodes(span, node => node.NodeType != NDjango.Interfaces.NodeType.ParsingContext))
                {
                    switch (node.ErrorMessage.Severity)
                    {
                        case -1:
                        case 0:
                            continue;
                        case 1:
                            yield return new TagSpan<ErrorTag>(node.SnapshotSpan, new ErrorTag(PredefinedErrorTypeNames.Warning));
                            break;
                        default:
                            yield return new TagSpan<ErrorTag>(node.SnapshotSpan, new ErrorTag(PredefinedErrorTypeNames.SyntaxError));
                            break;
                    }
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    }
}
