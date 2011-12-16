using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    class NamespaceReferenceList : ResultList
    {
        public NamespaceReferenceList(string text, string fName)
            : base(text, fName, 0, 0, LibraryNodeType.Namespaces)
        {
            // class list
        }

        protected override bool IsExpandable
        {
            get { return true; }
        }
        public override bool CanGoToSource
        {
            get
            {
                return false; // Namespaces can not go to source I think )
            }
        }
        protected override bool CanDelete { get { return false; } }

        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            // do nothing - not supported for namespaces
        }
    }
}
