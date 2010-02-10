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

        public FilterCompletionSet() { }

        protected override int FilterOffset { get { return 1; } }


        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(Node.ParsingContext.Filters, "|", "");
        }

    }
}
