using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    abstract class AbstractMember : AbstractCompletionSet
    {
        string[] facets;

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(
                buildMemberList(facets.ToList(), Node.Context.Variables).OrderBy(var => var.Name).Select(var => var.Name),
                Prefix, Suffix);
        }

        protected override int FilterOffset { get { return 0;} }

        protected virtual string GetSpanText(string span_text)
        {
            return span_text;
        }

        protected virtual int GetCompletionOffset(string span_text)
        {
            return span_text.LastIndexOf('.') + 1;
        }

        protected override int InitializeFilters(string existing)
        {
            facets = GetSpanText(existing).Split('.');
            return GetCompletionOffset(existing);
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
        protected abstract string Prefix { get; }


        /// <summary>
        /// returns the text to append to the name of the selected member
        /// </summary>
        protected abstract string Suffix { get; }
    }
}
