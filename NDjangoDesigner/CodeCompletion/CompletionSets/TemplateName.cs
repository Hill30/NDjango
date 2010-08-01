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
    class TemplateName: AbstractCompletionSet
    {
        char quote_char;
        public TemplateName() { }

        protected override void Initialize(Source source, DesignerNode node, SnapshotPoint point)
        {
            base.Initialize(source, node, point);
            switch (source.Context)
            {
                case CompletionContext.QuotedString:
                    quote_char = '"';
                    break;
                case CompletionContext.AposString:
                    quote_char = '\'';
                    break;
                default:
                    System.Diagnostics.Debug.Assert(true, "Contexts other than QuotedString and AposString are not allowed, context=" + source.Context.ToString());
                    break;
            }
            source.Session.Committed += new EventHandler(Session_Committed);
        }

        void Session_Committed(object sender, EventArgs e)
        {
            ITrackingSpan trackingSpan;
            if (!Source.Session.Properties.TryGetProperty<ITrackingSpan>(typeof(Controller), out trackingSpan))
                return;
            var inserted = trackingSpan.GetText(trackingSpan.TextBuffer.CurrentSnapshot);
            if (!inserted.StartsWith("\""))
                return;
            inserted = inserted.Substring(1);
            if (!inserted.EndsWith("\""))
                return;
            inserted = inserted.Substring(0, inserted.Length - 1);
            Node.Provider.Project.TemplateDirectory.RegisterInserted(inserted);
        }

        protected override int FilterOffset { get { return 1; } }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(Node.Provider.Project.TemplateDirectory.GetTemplates(null), quote_char.ToString(), quote_char.ToString());
        }

        protected override IEnumerable<Completion> BuildNodeCompletionBuilders()
        {
            return BuildCompletions(Node.Provider.Project.TemplateDirectory.Recent5Templates, quote_char.ToString(), quote_char.ToString());
        }
    }
}
