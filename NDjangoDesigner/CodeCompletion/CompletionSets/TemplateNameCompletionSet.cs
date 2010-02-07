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
        readonly static char[] delims = { '"', '\'' };

        internal static CompletionSet Create(NodeProvider nodeProvider, SnapshotPoint point)
        {
            return Create<TemplateNameCompletionSet>(
                nodeProvider, point, 
                n => 
                    n.NodeType == NodeType.TemplateName
                    && delims.Contains(n.SnapshotSpan.GetText()[0])
                    );

        }

        internal static CompletionSet Create<T>
            (NodeProvider nodeProvider, SnapshotPoint point, Predicate<DesignerNode> filter)
            where T: AbstractCompletionSet
        {
            // Get a list of all nodes of template name type
            var node = nodeProvider.GetNodes(point, filter).FindLast(n => true);
            if (node == null)
                return null;
            return (T)Activator.CreateInstance(typeof(T), System.Reflection.BindingFlags.NonPublic, node, point) ;
        }

        char quote_char;
        private TemplateNameCompletionSet(DesignerNode node, SnapshotPoint point)
            : base (node, point)
        { this.quote_char = node.SnapshotSpan.GetText()[0]; }

        protected override int FilterOffset { get { return 1; } }

        List<string> values = new List<string>(new string[] { });
        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(values, quote_char.ToString(), quote_char.ToString() + ' ');
        }
    }
}
