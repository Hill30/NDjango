using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TemplateWizard;
using System.ComponentModel.Design;
using System.Xml;
using System.Runtime.InteropServices;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using NDjangoDesigner;

using EnvDTE;
using EnvDTE80;
namespace NDjango.Designer.Commands
{
    class AddViewCommand : OleMenuCommand, IWizard
    {
        private static AddViewDlg viewDialog;

        public AddViewCommand()
            : base(Execute, new CommandID(Constants.guidNDjangoDesignerCmdSet, (int)Constants.cmdidNDjangoDesigner))
        {
            BeforeQueryStatus += new EventHandler(QueryStatus);
        }

        void QueryStatus(object sender, EventArgs e) 
        { 
            int active;
            ViewWizard.SelectionService.IsCmdUIContextActive(ViewWizard.contextCookie,out active);
            ((OleMenuCommand)sender).Visible = active == 1;
        }
        private static void Execute(object sender, EventArgs e)
        {
            viewDialog = new AddViewDlg();
            viewDialog.FillDialogControls();
            viewDialog.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void ExecuteForNewItem(string newFileName, string viewsFolderName)
        {
            viewDialog = new AddViewDlg();            
            viewDialog.FillDialogControls(viewsFolderName);

            viewDialog.ViewName = newFileName;
            viewDialog.ViewNameEnabled = false;
            viewDialog.WriteItemDirectly = false;

            viewDialog.ShowDialog();
        }

        public static void viewDialog_OnAddPressed(object sender, EventArgs e)
        {
            // Do nothing
        }

        void IWizard.BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {   
            // Do nothing
        }

        void IWizard.ProjectFinishedGenerating(EnvDTE.Project project)
        {
            // Do nothing
        }

        void IWizard.ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
            // Do nothing
        }

        void IWizard.RunFinished()
        {
            // Do nothing
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="automationObject">???</param>
        /// <param name="replacementsDictionary">Dictionary containing variables that will be replaced in the template. Addable.</param>
        /// <param name="runKind">Specifies constants that define the different templates the template wizard can create.
        /// AsNewItem / AsNewProject / AsMultiProject</param>
        /// <param name="customParams">The custom parameters with which to perform parameter replacement in the project.</param>
        void IWizard.RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            List<string> l1 = new List<string>();

            foreach (UIHierarchyItem item in (((DTE2)automationObject).ToolWindows.SolutionExplorer.SelectedItems as Array))
            {
                ProjectItem prjItem = item.Object as ProjectItem;
                l1.Add(prjItem.Properties.Item("FullPath").Value.ToString());
            }
            
            // "$rootname$" is always present
            ExecuteForNewItem(replacementsDictionary["$rootname$"], l1[0]);

            // NOTE! 
            // 1 - Item in template XML must have its parameter ReplaceParameters being set to "true" in order for this to function!
            // 2 - Strings to replace should end up with the line break
            if (viewDialog.SelectedModel != string.Empty)
                replacementsDictionary.Add("$model$", "{% model Model:" + viewDialog.SelectedModel + " %}\r\n");
            else
                replacementsDictionary.Add("$model$", string.Empty);

            if (viewDialog.ModelToExtend != string.Empty)
                replacementsDictionary.Add("$extends$", "{% extends \"~\\" + viewDialog.ModelToExtend + "\" %}\r\n");
            else
                replacementsDictionary.Add("$extends$", string.Empty);

            
            replacementsDictionary.Add("$pregenerated$", viewDialog.PreGeneratedTemplateText + "\r\n");
        }

        bool IWizard.ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
