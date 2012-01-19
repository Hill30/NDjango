using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;

namespace NDjango.Designer.SymbolLibrary.ObjectLists
{
    public class ModelReferenceList : NodeReferenceList
    {
        public ModelReferenceList(string text, string fName, string preffix, int lineNumber, int columnNumber)
            : base(text, fName, preffix, lineNumber, columnNumber)
        {
        }

        protected override bool IsExpandable
        {
            get { return false; }
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
                    Image = 208,
                    SelectedImage = 208,
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
