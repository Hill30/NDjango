using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.ComponentModel.Design;
using System.Xml;
using System.Runtime.InteropServices;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using NewViewGenerator;

namespace NewViewGenerator.AddView
{
    class AddViewCommand : OleMenuCommand
    {
        private static AddViewDlg viewDialog;

        public AddViewCommand()
            : base(Execute, new CommandID(GuidList.guidNewViewGeneratorCmdSet, (int)GuidList.cmdidNewViewGenerator))
        {
            BeforeQueryStatus += new EventHandler(QueryStatus);
            viewDialog = new AddViewDlg();
        }
        void QueryStatus(object sender, EventArgs e) 
        { 
            int active;
            ViewWizard.SelectionService.IsCmdUIContextActive(ViewWizard.contextCookie,out active);
            ((OleMenuCommand)sender).Visible = active == 1;
        }
        private static void Execute(object sender, EventArgs e)
        {
            viewDialog.FillDialogControls();
            viewDialog.ShowDialog();
        }
    }
}
