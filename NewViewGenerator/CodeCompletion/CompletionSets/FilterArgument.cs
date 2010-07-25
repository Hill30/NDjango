using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    /// <summary>
    /// Builds completion sets for filter arguments - in response to the user enetering ':' 
    /// </summary>
    class FilterArgument : AbstractMember
    {

        protected override string GetSpanText(string span_text)
        {
            return "";
        }

        protected override int GetCompletionOffset(string span_text)
        {
            // extra +1 is removed to compensate for extra '.' (see the base)
            return span_text.Length;
        }

        protected override string Prefix { get { return ":"; } }

        /// <summary>
        /// returns the text to append to the name of the selected member
        /// </summary>
        protected override string Suffix { get { return ""; } }
    }
}
