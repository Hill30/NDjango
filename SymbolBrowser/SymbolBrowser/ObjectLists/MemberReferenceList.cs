using EnvDTE;
using EnvDTE80;
using System;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    public class MemberReferenceList : ResultList
    {
        public MemberReferenceList(string text, string fName, uint lineNumber)
            : base(text, fName, lineNumber, LibraryNodeType.Members)
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
        protected override bool CanDelete { get { return true; } }

        /// <summary>
        /// pfCatField = (uint)_LIBCAT_MEMBERACCESS.LCMA_PUBLIC;
        /// </summary>
        public override bool IsPrivate{ get{ return true; } }

        /// <summary>
        /// pfCatField = (uint)_LIBCAT_MEMBERACCESS.LCMA_PROTECTED;
        /// </summary>
        public override bool IsProtected { get { return true; } }
        
        /// <summary>
        /// pfCatField = (uint)_LIBCAT_MEMBERACCESS.LCMA_PROTECTED | (uint)_LIBCAT_MEMBERACCESS.LCMA_PACKAGE;
        /// </summary>
        public override bool IsAssembly { get { return false; } }

        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            //foreach(SymbolBrowserPackage.DTE2Obj.Solution.Projects.Count
            //return null;
            throw new NotImplementedException();
        }
    }
}
