using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    class NamespaceReferenceList : ResultList
    {
        public NamespaceReferenceList(string text, string preffix, string fName)
            : base(text, fName, preffix, 0, 0, LibraryNodeType.Namespaces)
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

        protected override VSTREEDISPLAYDATA DisplayData
        {
            get
            {
                return new VSTREEDISPLAYDATA
                {
                    ForceSelectLength = 5,
                    ForceSelectStart = 0,
                    hImageList = IntPtr.Zero,
                    Image = 90,
                    SelectedImage = 90,
                    Mask = (uint)_VSTREEDISPLAYMASK.TDM_IMAGE, //?!
                    State = (uint)0,
                    StateMask = (uint)0
                };
            }
        }

        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            // do nothing - not supported for namespaces
        }
    }
}
