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

        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            // это "проба пера", сейчас переделывается на работу с COM объектами (как и для моделей)

            SymbolBrowserPackage.DTE2Obj.ItemOperations.OpenFile(
                 @"c:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs",
                 EnvDTE.Constants.vsViewKindCode);
            ((TextSelection)SymbolBrowserPackage.DTE2Obj.ActiveDocument.Selection).GotoLine((int)lineNumber, false);
            ((TextSelection)SymbolBrowserPackage.DTE2Obj.ActiveDocument.Selection).FindText("GetBlaBlaBla");
        }
    }
}
