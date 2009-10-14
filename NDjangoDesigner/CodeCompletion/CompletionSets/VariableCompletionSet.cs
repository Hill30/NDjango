using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion
{
    class VariableCompletionSet : CompletionSet
    {
        internal static CompletionSet Create(List<DesignerNode> nodes, SnapshotPoint point)
        {
            DesignerNode node = nodes.FindLast(n => n.NodeType == NDjango.Interfaces.NodeType.ParsingContext);
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

        protected override List<Completion> NodeCompletions
        {
            get
            {
                return new List<Completion>(BuildCompletions(new List<string> ( new string[] {" }}"})));
            }
        }

        private IEnumerable<Completion> BuildCompletions(IEnumerable<string> values)
        {
            return BuildCompletions(values, "{ ", "");
        }
    }
}
