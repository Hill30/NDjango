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
    /// Represents a list of completions for available tag names
    /// </summary>
    class TagNameCompletionSet : AbstractCompletionSet
    {
        internal TagNameCompletionSet(Source source, DesignerNode node, SnapshotPoint point)
            : base(source, node, point)
        { }

        private IEnumerable<Completion> BuildCompletions(IEnumerable<string> values)
        {
            return BuildCompletions(values, "", "");
        }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(Node.ParsingContext.Tags); 
        }

        protected override IEnumerable<Completion> BuildNodeCompletionBuilders()
        {
            return BuildCompletions(Node.ParsingContext.TagClosures);
        }
    }
}
