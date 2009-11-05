using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace NDjango.Designer.Classifiers
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    [ContentType(Constants.NDJANGO)] 
    internal sealed class Highlighter : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Caret.PositionChanged += new EventHandler<CaretPositionChangedEventArgs>(Caret_PositionChanged);
        }

        void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            NodeProvider provider;
            if (!e.TextView.TextBuffer.Properties.TryGetProperty(typeof(NodeProvider), out provider))
                return;
            Highlight(provider, e.NewPosition);
        }

        private void Highlight(NodeProvider provider, CaretPosition position)
        {
            SnapshotPoint point = position.BufferPosition;

            List<DesignerNode> tags = provider.GetNodes(point, node => node.NodeType == NDjango.Interfaces.NodeType.TagName);
            DesignerNode selected = tags.Count == 0 ? null : tags[0];

            DesignerNode highlighted = null;
            point.Snapshot.TextBuffer.Properties.TryGetProperty<DesignerNode>(typeof(Highlighter), out highlighted);
            if (selected != highlighted)
            {
                point.Snapshot.TextBuffer.Properties[typeof(Highlighter)] = selected;
                provider.RaiseNodesChanged(point.Snapshot);
            }
        }
    }
}
