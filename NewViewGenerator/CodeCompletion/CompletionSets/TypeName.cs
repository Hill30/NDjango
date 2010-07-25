using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class TypeName : AbstractCompletionSet
    {
        public TypeName(Source source, DesignerNode node, SnapshotPoint point)
            : base(source, node, point)
        { }

        protected override IEnumerable<Completion> BuildNodeCompletions()
        {
            return BuildCompletions(new string[] { }, "", "");
        }
    }

}
