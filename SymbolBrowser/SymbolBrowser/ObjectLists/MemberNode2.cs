using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    public class MemberNode2 : SymbolNode2
    {
        public MemberNode2(string text, string fName, string preffix, int lineNumber, int columnNumber)
            : base(text, fName, preffix, lineNumber, columnNumber, LibraryNodeType.Hierarchy)
        {
            classType = _LIBCAT_CLASSTYPE.LCCT_NSPC;
            memberAccess = _LIBCAT_MEMBERACCESS.LCMA_PUBLIC;
            memberType = _LIBCAT_MEMBERTYPE.LCMT_METHOD;
            modifierType = _LIBCAT_MODIFIERTYPE.LCMDT_VIRTUAL;
            visibility = _LIBCAT_VISIBILITY.LCV_VISIBLE;
            hierarchyType = _LIBCAT_HIERARCHYTYPE.LCHT_UNKNOWN;
            memberInheritance = _LIBCAT_MEMBERINHERITANCE.LCMI_IMMEDIATE;
            phisContainerType = 0;
            srchMatchType = 0;
        }

        /// <summary>
        /// True if children count > 0
        /// </summary>
        protected override bool IsExpandable
        {
            get { return (Children.Count > 0); }
        }

        public override bool CanGoToSource
        {
            get
            {
                return true; // members can go to source
            }
        }
        protected override bool CanDelete { get { return true; } }

        protected override VSTREEDISPLAYDATA DisplayData
        {
            get
            {
                return new VSTREEDISPLAYDATA
                {
                    ForceSelectLength = 5,
                    ForceSelectStart = 0,
                    hImageList = IntPtr.Zero,
                    Image = 72,
                    SelectedImage = 72,
                    Mask = (uint)_VSTREEDISPLAYMASK.TDM_IMAGE, //?!
                    State = 0,
                    StateMask = 0
                };
            }
        }
        
        protected override void GotoSource(VSOBJGOTOSRCTYPE gotoType)
        {

           // We do not support the "Goto Reference"
            if (VSOBJGOTOSRCTYPE.GS_REFERENCE == gotoType)
            {
                return;
            }

            base.OpenSourceFile();
        }

        
    }
}
