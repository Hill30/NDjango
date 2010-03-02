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

        protected override void Initialize(CompletionContext context, DesignerNode node, SnapshotPoint point)
        {
            base.Initialize(context, node, point);
            switch (context)
            {
                case CompletionContext.QuotedString:
                    quote_char = '"';
                    break;
                case CompletionContext.AposString:
                    quote_char = '\'';
                    break;
                default:
                    System.Diagnostics.Debug.Assert(true, "Contexts other than QuotedString and AposString are not allowed, context=" + context.ToString());
                    break;
            }
        }

        protected override int FilterOffset { get { return 1; } }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(Node.Values, quote_char.ToString(), quote_char.ToString() + ' ');
        }
    }
}
