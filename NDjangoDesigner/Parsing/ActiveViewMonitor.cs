using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.Parsing
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    [ContentType(Constants.NDJANGO)]
    class ActiveViewMonitor : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView, IEnvironment context)
        {
            textView.GotAggregateFocus += new EventHandler(textView_GotAggregateFocus);
        }

        void textView_GotAggregateFocus(object sender, EventArgs e)
        {
            IWpfTextView view = sender as IWpfTextView;
            if (view == null)
                return;

            NodeProvider provider;
            foreach (ITextBuffer buffer in view.BufferGraph.GetTextBuffers(b => true))
                if (buffer.Properties.TryGetProperty(typeof(NodeProvider), out provider))
                {
                    provider.ShowDiagnostics();
                    return;
                }

        }
    }
}
