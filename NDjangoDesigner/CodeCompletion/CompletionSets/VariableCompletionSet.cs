using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class VariableCompletionSet : ReferenceCompletionSet
    {

        protected override string Prefix { get { return "{"; } }

        protected override string Suffix { get { return "}}"; } }
    }
}
