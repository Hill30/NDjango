using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NDjango.Designer.Commands;
using NDjango.Designer.SymbolLibrary;

namespace NDjango.Designer
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //auto load package if UICONTEXT_SolutionExists
    [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
    [Guid(Constants.guidNDjangoDesignerPkgString)]
    public sealed class NDjangoDesignerPackage : Package
    {
        private AddViewDlg viewDialog;

        [Import]
        public ILibraryMgr libraryMgr;

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowAddView(object sender, EventArgs e)
        {
            viewDialog.FillDialogControls();
            viewDialog.ShowDialog();
        }

        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                //CommandID addViewCommandID = new CommandID(GuidList.guidNDjangoDesignerCmdSet, (int)GuidList.cmdidNDjangoDesigner);
                //OleMenuCommand cmd = new OleMenuCommand(ShowAddView, addViewCommandID);
                mcs.AddCommand(new AddViewCommand());
            }
            viewDialog = new AddViewDlg();

            ((IComponentModel)GetGlobalService(typeof(SComponentModel))).DefaultCompositionService.SatisfyImportsOnce(this);

            libraryMgr.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                libraryMgr.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

    }
}
