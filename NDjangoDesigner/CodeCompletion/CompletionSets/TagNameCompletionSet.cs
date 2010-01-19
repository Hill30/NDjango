using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion
{
    class TagNameCompletionSet : CompletionSet
    {
        internal TagNameCompletionSet(DesignerNode node, SnapshotPoint point)
            : base(node, point)
        {
        }

        protected override List<Completion> NodeCompletions
        {
            get { return new List<Completion>(BuildCompletions(Node.ParsingContext.Tags)); }
        }

        protected override List<Completion> NodeCompletionBuilders
        {
            get { return new List<Completion>(BuildCompletions(Node.ParsingContext.TagClosures)); }
        }

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
