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

        internal static CompletionSet Create(NodeProvider nodeProvider, SnapshotPoint point)
        {
            DesignerNode node = 
                // Get the list of all nodes with non-empty value lists
                nodeProvider.GetNodes(point, n => n.Values.GetEnumerator().MoveNext())
                // out of the list get the last parsing context
                .FindLast(n => n.NodeType == NDjango.Interfaces.NodeType.ParsingContext);
            if (node == null)
                return null;
            return new TagCompletionSet(node, point);
        }

        private TagCompletionSet(DesignerNode node, SnapshotPoint point)
            : base (node, point)
        { }

        private IEnumerable<Completion> BuildCompletions(IEnumerable<string> values)
        {
            return BuildCompletions(values, "% ", " %}");
        }

        protected override int FilterOffset { get { return 1; } }

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
