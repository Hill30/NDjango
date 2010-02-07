using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    /// <summary>
    /// Represents a list of completions for available filter names
    /// </summary>
    class FilterCompletionSet : AbstractCompletionSet
    {
        internal static CompletionSet Create(NodeProvider nodeProvider, SnapshotPoint point)
        {
            DesignerNode node =
                // Get the list of all nodes with non-empty value lists
                nodeProvider.GetNodes(point, n => n.Values.GetEnumerator().MoveNext())
                // out of the list get the last parsing context
                .FindLast(n => n.NodeType == NDjango.Interfaces.NodeType.ParsingContext);
            if (node == null)
                return null;
            return new FilterCompletionSet(node, point);
        }

        private FilterCompletionSet(DesignerNode node, SnapshotPoint point)
            : base(node, point)
        { }

        protected override int FilterOffset { get { return 1; } }


        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(Node.ParsingContext.Filters, "|", "");
        }

    }
}
