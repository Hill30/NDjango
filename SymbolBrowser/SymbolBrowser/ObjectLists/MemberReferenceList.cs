using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    public class MemberReferenceList : ResultList
    {
        public MemberReferenceList(string text, string fName, int lineNumber, int columnNumber)
            : base(text, fName, lineNumber, columnNumber, LibraryNodeType.Members)
        {
            // class list
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
