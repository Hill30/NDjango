using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class ReferenceCompletionSet : AbstractCompletionSet
    {
        public ReferenceCompletionSet() { }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            var facets = getPrefix().Split('.');
            return BuildCompletions(
                buildMemberList(facets.ToList(), Node.Context.Variables).Select(var => var.Name),
                Prefix, Suffix);
        }

        private IEnumerable<Interfaces.IDjangoType> buildMemberList(List<string> facets, IEnumerable<Interfaces.IDjangoType> members)
        {
            var instance = Node.Context.Variables.First(member => member.Name == facets[0]);
            if (instance == null)
                return null;
            facets.RemoveAt(0);
            if (facets.Count == 0)
                return instance.Members;
            return buildMemberList(facets, instance.Members);
        }

        protected virtual string Prefix { get { return ""; } }

        protected virtual string Suffix { get { return ""; } }
    }
}
