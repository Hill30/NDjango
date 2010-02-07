using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class ValueCompletionSet : AbstractCompletionSet
    {
        public ValueCompletionSet(DesignerNode node, SnapshotPoint point)
            : base(node, point)
        { }

        protected override IEnumerable<Microsoft.VisualStudio.Language.Intellisense.Completion> BuildNodeCompletions()
        {
            return BuildCompletions(Node.Values, "", "");
        }
    }
}
