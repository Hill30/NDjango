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

        public CompletionContext Context { get; private set; }
        public ICompletionSession Session { get; private set; }

        public void AugmentCompletionSession(ICompletionSession session, IList<VSCompletionSet> completionSets)
        {
            CompletionContext context;
            if (!session.Properties.TryGetProperty<CompletionContext>(typeof(CompletionContext), out context))
                return;
            Context = context;
            Session = session;

            SnapshotPoint point = session.GetTriggerPoint(textBuffer).GetPoint(textBuffer.CurrentSnapshot);

            CompletionSet set = CreateCompletionSet(point);

            if (set == null)
                return;

            completionSets.Add(set);
        }

        public void Dispose()
        { }

        private CompletionSet CreateCompletionSet(SnapshotPoint point)
        {
            switch (Context)
            {

                case CompletionContext.Tag:
                    return AbstractCompletionSet.Create<TagCompletionSet>(
                        this, point,
                            nodeProvider.GetNodes(point, n => n.NodeType == NodeType.ParsingContext).FindLast(n => true)
                            );

                case CompletionContext.Variable:
                    return AbstractCompletionSet.Create<VariableCompletionSet>(
                        this, point,
                            nodeProvider.GetNodes(point, n => n.NodeType == NodeType.ParsingContext).FindLast(n => true)
                            );

                case CompletionContext.FilterName:
                    return AbstractCompletionSet.Create<FilterCompletionSet>(
                        this, point,
                            nodeProvider.GetNodes(point, n => n.NodeType == NodeType.ParsingContext).FindLast(n => true)
                            );

                case CompletionContext.Word:
                    // Get the list of all nodes with non-empty value lists
                    List<DesignerNode> nodes = nodeProvider.GetNodes(point, n => n.NodeType == NodeType.Reference || n.Values.GetEnumerator().MoveNext());
                    // out of the list get the last node which is not a parsing context
                    DesignerNode node = nodes.FindLast(n => n.NodeType != NodeType.ParsingContext);
                    if (node == null)
                        return null;
                    if (node.NodeType == NodeType.Reference)
                        return AbstractCompletionSet.Create<ReferenceCompletionSet>(this, point, node);
                    if (node.NodeType == NodeType.TagName)
                        return new TagNameCompletionSet(this, node, point);
                    return new ValueCompletionSet(this, node, point);

                case CompletionContext.Reference:
                    return AbstractCompletionSet.Create<ReferenceCompletionSet>(
                        this, point,
                        nodeProvider.GetNodes(point, n => n.NodeType == NodeType.Reference).FindLast(n => true));

                case CompletionContext.AposString:
                case CompletionContext.QuotedString:
                    return AbstractCompletionSet.Create<TemplateNameCompletionSet>(
                        this, point, 
                        nodeProvider.GetNodes(point, n => n.NodeType == NodeType.TemplateName).FindLast(n=>true));

                default:
                    return null;
            }

        }

    }
}
