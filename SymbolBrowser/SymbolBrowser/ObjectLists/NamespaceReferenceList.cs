using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    class NamespaceReferenceList : ResultList
    {
        public NamespaceReferenceList(string text, string fName)
            : base(text, fName, 0, LibraryNodeType.Namespaces)
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
                return true; // models can go to source
            }
        }
        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            //foreach(SymbolBrowserPackage.DTE2Obj.Solution.Projects.Count
            
        }
    }
}
