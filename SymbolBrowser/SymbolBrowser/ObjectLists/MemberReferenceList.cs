using EnvDTE;
using EnvDTE80;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    public class MemberReferenceList : ResultList
    {
        public MemberReferenceList(string text, string fName)
            : base(text, fName, 0, LibraryNodeType.Members)
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
        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            //foreach(SymbolBrowserPackage.DTE2Obj.Solution.Projects.Count
            
        }
    }
}
