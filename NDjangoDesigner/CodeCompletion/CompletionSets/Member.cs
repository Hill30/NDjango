using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    class Member : AbstractMember
    {
        /// <summary>
        /// returns the text to prepend to the name of the selected member
        /// </summary>
        protected override string Prefix { get { return ""; } }

        /// <summary>
        /// returns the text to append to the name of the selected member
        /// </summary>
        protected override string Suffix { get { return ""; } }
    }
}
