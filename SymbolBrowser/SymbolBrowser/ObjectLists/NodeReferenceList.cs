using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Runtime.InteropServices;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    /// <summary>
    /// A node representing a reference of the symbol
    /// </summary>
    public class NodeReferenceList : SymbolNode
    {
        public NodeReferenceList(string text, string prefix, string fName, int lineNumber, int columnNumber) :
            base(text, prefix, fName, lineNumber, columnNumber, LibraryNodeType.Hierarchy)
        {
            // set for search results reference list
            memberAccess = 0;
            memberType = 0;
            modifierType = 0;
            visibility = 0;
            hierarchyType = 0;
            memberInheritance = 0;
            phisContainerType = 0;
            srchMatchType = 0; 
        }
    }        
}
