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
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Collections.ObjectModel;
using NDjango.Interfaces;
using NDjango.Designer.Parsing;
using VSCompletionSet = Microsoft.VisualStudio.Language.Intellisense.CompletionSet;
using NDjango.Designer.CodeCompletion.CompletionSets;

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

            CompletionSet set = CreateCompletionSet(context, point);

            if (set == null)
                return null;

            return new ReadOnlyCollection<VSCompletionSet> (new CompletionSet[] { set });

        }

        private CompletionSet CreateCompletionSet(CompletionContext context, SnapshotPoint point)
        {
            switch (context)
            {
                case CompletionContext.Tag:
                    return AbstractCompletionSet.Create<TagCompletionSet>(
                        nodeProvider, point,
                        n => n.NodeType == NodeType.ParsingContext
                            );

                case CompletionContext.Variable:
                    return AbstractCompletionSet.Create<VariableCompletionSet>(
                        nodeProvider, point, 
                        n => n.NodeType == NodeType.ParsingContext
                            );

                case CompletionContext.FilterName:
                    return AbstractCompletionSet.Create<FilterCompletionSet>(
                        nodeProvider, point, 
                        n => n.NodeType == NodeType.ParsingContext
                            );

                case CompletionContext.Word:
                    // Get the list of all nodes with non-empty value lists
                    List<DesignerNode> nodes = nodeProvider.GetNodes(point, n => n.Values.GetEnumerator().MoveNext());
                    // out of the list get the last node which is not a parsing context
                    DesignerNode node = nodes.FindLast(n => n.NodeType != NodeType.ParsingContext);
                    if (node == null)
                        break;
                    if (node.NodeType == NodeType.TagName)
                        return new TagNameCompletionSet(node, point);
                    return new ValueCompletionSet(node, point);

                default:
                    break;
            }

            return null;
            // for now let us leave the template names alone
            //return AbstractCompletionSet.Create<TemplateNameCompletionSet>(
            //    nodeProvider, point,
            //    n =>
            //        n.NodeType == NodeType.TemplateName
            //        && string_delimiters.Contains(n.SnapshotSpan.GetText()[0])
            //        );
        }

        readonly static char[] string_delimiters = { '"', '\'' };
    }
}
