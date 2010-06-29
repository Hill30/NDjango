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
    class NewMember : AbstractMember
    {

        protected override string GetSpanText(string span_text)
        {
            // for new members we need to take the '.' into account
            return span_text + '.';
        }

        protected override int GetCompletionOffset(string span_text)
        {
            // extra +1 is removed to compensate for extra '.' (see the base)
            return (span_text + '.').LastIndexOf('.');
        }

        protected override string Prefix { get { return "."; } }

        /// <summary>
        /// returns the text to append to the name of the selected member
        /// </summary>
        protected override string Suffix { get { return ""; } }
    }
}
