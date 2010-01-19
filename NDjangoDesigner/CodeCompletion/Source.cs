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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Collections.ObjectModel;
using NDjango.Interfaces;
using NDjango.Designer.Parsing;
using VSCompletionSet = Microsoft.VisualStudio.Language.Intellisense.CompletionSet;

namespace NDjango.Designer.CodeCompletion
{

    /// <summary>
    /// Supplies a list of completion values
    /// </summary>
    internal class Source : ICompletionSource
    {
        private NodeProvider nodeProvider;
        private ITextBuffer textBuffer;
        public Source(INodeProviderBroker nodeProviderBroker, ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
            nodeProvider = nodeProviderBroker.GetNodeProvider(textBuffer);
        }
        /// <summary>
        /// Gets the completion information
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        /// <remarks>
        /// The location of the textspan to be replaced with 
        /// the selection so that the entire word would be replaced
        /// </remarks>
        public ReadOnlyCollection<VSCompletionSet> GetCompletionInformation(ICompletionSession session)
        {
            CompletionContext context;
            if (!session.Properties.TryGetProperty<CompletionContext>(typeof(CompletionContext), out context))
                return null;

            SnapshotPoint point = session.GetTriggerPoint(textBuffer).GetPoint(textBuffer.CurrentSnapshot);

            List<DesignerNode> nodes = nodeProvider
                    .GetNodes(point, n => n.Values.GetEnumerator().MoveNext());

            CompletionSet set = CreateCompletionSet(context, nodes, point);

            if (set == null)
                return null;

            return new ReadOnlyCollection<VSCompletionSet> (new CompletionSet[] { set });

        }

        private CompletionSet CreateCompletionSet(CompletionContext context, List<DesignerNode> nodes, SnapshotPoint point)
        {
            switch (context)
            {
                case CompletionContext.Tag:
                    return TagCompletionSet.Create(nodes, point);

                case CompletionContext.Variable:
                    return VariableCompletionSet.Create(nodes, point);

                case CompletionContext.FilterName:
                    return FilterCompletionSet.Create(nodes, point);

                case CompletionContext.Other:
                    DesignerNode node = nodes.FindLast(n => n.NodeType != NodeType.ParsingContext);
                    if (node == null)
                        return null;
                    if (node.NodeType == NodeType.TagName)
                        return new TagNameCompletionSet(node, point);
//                    return new CompletionSet(node, point);
                    return null;

                default:
                    return null;

            }
        }
    }
}
