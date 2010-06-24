using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class ReferenceCompletionSet : AbstractCompletionSet
    {
        int completion_offset;
        string completion_prefix;
        string[] facets;

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(
                buildMemberList(facets.ToList(), Node.Context.Variables).Select(var => var.Name),
                Prefix, Suffix);
        }

        protected override int FilterOffset { get { return 0;} }

        protected override string GetPrefix() { return completion_prefix; }

        protected override Span InitializeSpan(string span_text, Span span)
        {
            facets = span_text.Split('.');
            //span_text += '.';
            completion_offset = span_text.LastIndexOf('.') + 1;
            completion_prefix = span_text.Substring(completion_offset);
            return new Span(span.Start + completion_offset - 1, span.Length - completion_offset + 1);
        }

        private IEnumerable<Interfaces.IDjangoType> buildMemberList(List<string> facets, IEnumerable<Interfaces.IDjangoType> members)
        {
            var instance = Node.Context.Variables.FirstOrDefault(member => member.Name == facets[0]);
            if (instance == null)
                return new List<Interfaces.IDjangoType>();
            facets.RemoveAt(0);
            if (facets.Count == 0)
                return instance.Members;
            return buildMemberList(facets, instance.Members);
        }

        protected virtual string Prefix { get { return ""; } }

        protected virtual string Suffix { get { return ""; } }
    }
}
