using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion
{
    class FilterCompletionSet : CompletionSet
    {
        internal static CompletionSet Create(List<DesignerNode> nodes, SnapshotPoint point)
        {
            DesignerNode node = nodes.FindLast(n => n.NodeType == NDjango.Interfaces.NodeType.ParsingContext);
            if (node == null)
                return null;
            return new FilterCompletionSet(node, point);
        }

        private FilterCompletionSet(DesignerNode node, SnapshotPoint point)
            : base(node, point)
        { }

        protected override List<Completion> NodeCompletions
        {
            get { return new List<Completion>(BuildCompletions(Node.ParsingContext.Filters, "|", "")); }
        }

        protected override int FilterOffset { get { return 1; } }

    }
}
