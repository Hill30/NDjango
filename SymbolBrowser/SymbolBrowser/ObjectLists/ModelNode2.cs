using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    /// <summary>
    /// Represents a class in a reference search results
    /// </summary>
    public class ModelNode2 : SymbolNode2
    {
        public ModelNode2(string text, string fName, string preffix, int lineNumber, int columnNumber)
            : base(text, fName, preffix, lineNumber, columnNumber, LibraryNodeType.Hierarchy)
        {
            classType = _LIBCAT_CLASSTYPE.LCCT_CLASS;
            memberAccess = _LIBCAT_MEMBERACCESS.LCMA_PRIVATE;
            memberType = _LIBCAT_MEMBERTYPE.LCMT_FUNCTION;
            modifierType = _LIBCAT_MODIFIERTYPE.LCMDT_PUREVIRTUAL;
            visibility = _LIBCAT_VISIBILITY.LCV_VISIBLE;
            hierarchyType = _LIBCAT_HIERARCHYTYPE.LCHT_UNKNOWN;
            memberInheritance = _LIBCAT_MEMBERINHERITANCE.LCMI_IMMEDIATE;

            phisContainerType = 0;
            srchMatchType = 0;
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
        protected override bool CanDelete { get { return true; } }

        protected override VSTREEDISPLAYDATA DisplayData
        {
            get
            {
                return new VSTREEDISPLAYDATA
                {
                    ForceSelectLength = 0,
                    ForceSelectStart = 0,
                    hImageList = IntPtr.Zero,
                    Image = 0,
                    SelectedImage = 0,
                    Mask = (uint)_VSTREEDISPLAYMASK.TDM_IMAGE, //?!
                    State = 0,
                    StateMask = 0
                };
            }
        }

        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
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
