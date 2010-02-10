using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using NDjango.Interfaces;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class TemplateNameCompletionSet: AbstractCompletionSet
    {
        char quote_char;
        public TemplateNameCompletionSet() { }

        protected override void Initialize(DesignerNode node, SnapshotPoint point)
        {
            base.Initialize(node, point);
            this.quote_char = node.SnapshotSpan.GetText()[0]; 
        }

        protected override int FilterOffset { get { return 1; } }

        List<string> values = new List<string>(new string[] { });
        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(values, quote_char.ToString(), quote_char.ToString() + ' ');
        }
    }
}
