using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    public class ModelReferenceList : ResultList
    {
        public ModelReferenceList(string text, string fName, uint lineNumber)
            : base(text, fName, lineNumber, LibraryNodeType.Classes)
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

        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            //var fName = @"c:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs";
            //var solution = SymbolBrowserPackage.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            //solution.
            //    .OpenSolutionFile((uint)__VSSLNOPENOPTIONS.SLNOPENOPT_Silent, fName);

            // это "проба пера", сейчас переделывается на работу с COM объектами

            SymbolBrowserPackage.DTE2Obj.ItemOperations.OpenFile(
                @"c:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs",
                EnvDTE.Constants.vsViewKindCode);
            ((TextSelection)SymbolBrowserPackage.DTE2Obj.ActiveDocument.Selection).GotoLine((int)lineNumber, false);
            ((TextSelection)SymbolBrowserPackage.DTE2Obj.ActiveDocument.Selection).FindText("Class1");
        }
    }
}
