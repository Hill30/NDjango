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
    /// Reperesents a list of completions for tags as a whole
    /// </summary>
    class TagCompletionSet : AbstractCompletionSet
    {

        public TagCompletionSet() { }

        private IEnumerable<Completion> BuildCompletions(IEnumerable<string> values)
        {
            return BuildCompletions(values, "% ", " %}");
        }

        protected override int FilterOffset { get { return 1; } }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(Node.Context.Tags);
        }

        protected override IEnumerable<Completion> BuildNodeCompletionBuilders()
        {
            return BuildCompletions(Node.Context.TagClosures);
        }
    }
}
