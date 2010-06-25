using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    abstract class AbstractMemberCompletionSet : AbstractCompletionSet
    {
        string completion_prefix;
        string[] facets;

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(
                buildMemberList(facets.ToList(), Node.Context.Variables).Select(var => var.Name),
                Prefix, Suffix);
        }

        protected override int FilterOffset { get { return 0;} }

        protected override string GetFilterPrefix() { return completion_prefix; }

        protected override Span InitializeSpan(string span_text, Span span)
        {
            facets = span_text.Split('.');
            int completion_offset = span_text.LastIndexOf('.') + 1;
            completion_prefix = span_text.Substring(completion_offset);
            return new Span(span.Start + completion_offset, span.Length - completion_offset);
        }

        private IEnumerable<Interfaces.IDjangoType> buildMemberList(List<string> facets, IEnumerable<Interfaces.IDjangoType> members)
        {
            if (facets.Count == 1)
                return members;
            var instance = Node.Context.Variables.FirstOrDefault(member => member.Name == facets[0]);
            if (instance == null)
                return new List<Interfaces.IDjangoType>();
            facets.RemoveAt(0);
            return buildMemberList(facets, instance.Members);
        }


        /// <summary>
        /// returns the text to prepend to the name of the selected member
        /// </summary>
        protected virtual string Prefix { get { return ""; } }


        /// <summary>
        /// returns the text to append to the name of the selected member
        /// </summary>
        protected virtual string Suffix { get { return ""; } }
    }
}
