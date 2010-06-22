using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class VariableCompletionSet : AbstractCompletionSet
    {
        public VariableCompletionSet() { }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            var vars = new List<string>();
            Node.ParsingContext.Variables
                .ToList()
                .ForEach(var => vars.Add(var.Name));
            return BuildCompletions(vars, "{", "}}");
        }
    }
}
