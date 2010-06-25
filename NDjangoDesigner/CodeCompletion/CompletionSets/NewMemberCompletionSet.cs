using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    /// <summary>
    /// Builds completion sets for new member on a variable - in response to the user enetering '.' 
    /// </summary>
    class NewMemberCompletionSet : AbstractMemberCompletionSet
    {
        protected override Span InitializeSpan(string span_text, Span span)
        {
            // for new members we need to take the '.' into account
            return base.InitializeSpan(span_text + '.', span);
        }

        protected override string Prefix { get { return "."; } }
    }
}
