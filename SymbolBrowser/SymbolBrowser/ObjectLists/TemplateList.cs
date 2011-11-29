using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    /// <summary>
    /// Node of PhisicalContainer type
    /// </summary>
    public class TemplateList:ResultList
    {
        // file list
        public TemplateList(string text, string fName)
            : base(text, fName, 0, LibraryNodeType.PhysicalContainer)
        {
        }
    }
}
