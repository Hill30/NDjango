using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    /// <summary>
    /// Builds completion sets for 'django variable' constructs
    /// </summary>
    class VariableCompletionSet : AbstractMember
    {
        protected override string Prefix { get { return "{ "; } }

        protected override string Suffix { get { return " }}"; } }
    }
}
