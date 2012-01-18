using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    class RootNode:SymbolNode
    {
        public RootNode(string text, string fName, string preffix, int lineNumber, int columnNumber)
            : base(text, fName, preffix, lineNumber, columnNumber, SymbolNode.LibraryNodeType.Hierarchy)
        {
            classType = _LIBCAT_CLASSTYPE.LCCT_NSPC;
            memberAccess = _LIBCAT_MEMBERACCESS.LCMA_PUBLIC;
            memberType = _LIBCAT_MEMBERTYPE.LCMT_METHOD;
            modifierType = _LIBCAT_MODIFIERTYPE.LCMDT_VIRTUAL;
        }

        
    }
}
