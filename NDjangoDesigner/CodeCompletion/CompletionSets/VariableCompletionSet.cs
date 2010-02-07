using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class VariableCompletionSet : AbstractCompletionSet
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
            return new VariableCompletionSet(node, point);
        }

        private VariableCompletionSet(DesignerNode node, SnapshotPoint point)
            : base (node, point)
        { }

        public override void SelectBestMatch()
        {
            SelectionStatus = new CompletionSelectionStatus(Completions[0], false, true);
        }

        private IEnumerable<Completion> BuildCompletions(IEnumerable<string> values)
        {
            return BuildCompletions(values, "{ ", "");
        }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(new List<string>(new string[] { " }}" }));
        }
    }
}
