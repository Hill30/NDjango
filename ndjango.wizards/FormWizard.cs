
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;

namespace ndjango.wizards
{
    class FormWizard:IWizard
    {
        private bool shouldAddProjectItem;

        #region ...
        //private AddViewDlg viewDialog;
        //public IVsMonitorSelection SelectionTracker = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
        //private Project curProject;
        //private TemplateDir templatesDir;
        //private string curFolder = string.Empty;
        //private string ViewsFolderName = string.Empty;

        ///// <summary>
        ///// 
        ///// </summary>
        //private bool ExecuteForNewItem(string newFileName)
        //{
        //    viewDialog = new AddViewDlg();
        //    viewDialog.FillDialogControls(ViewsFolderName);

        //    viewDialog.ViewName = newFileName;
        //    viewDialog.ViewNameEnabled = false;
        //    viewDialog.WriteItemDirectly = false;

        //    GetCurrentProject();

        //    return (viewDialog.ShowDialog() == DialogResult.OK);
        //}

        //private void GetCurrentProject()
        //{
        //    IntPtr 
        //        ppHier = IntPtr.Zero,
        //        ppSC = IntPtr.Zero;

        //    uint pitemid;
        //    IVsMultiItemSelect ppMIS;

        //    try
        //    {
        //        SelectionTracker.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC);
        //    }
        //    finally
        //    {
        //        if(ppHier != IntPtr.Zero)
        //        Marshal.Release(ppHier);
        //    }
        //    var o = (IVsHierarchy)Marshal.GetObjectForIUnknown(ppHier);
        //    object pvar;
        //    if (o.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out pvar) == VSConstants.S_OK)
        //    {
        //        curProject = pvar as Project;
        //    }

        //    if (o.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out pvar) == VSConstants.S_OK)
        //    { // Replacement for Update method
        //        templatesDir = new TemplateDir(pvar as string);
        //    }

        //    /*
        //    if (parser == null)
        //        parser = InitializeParser(); - static
        //    if (templatesDir == null)
        //    {
        //        templatesDir = new TemplateDirectory(projectDir); - inited above
        //    }
        //     */
        //}

        //public string GenerateName(string targetDir)
        //{
        //    int viewCounter = 1;
        //    var templates = GetTemplates("");
        //    while (true)
        //    {
        //        bool fileExist = false;
        //        var name = "ViewPage" + viewCounter.ToString();
        //        foreach (var template in templates)
        //            if (template.ToLower() == (GetFolderName() + name + ".django").ToLower())
        //            {
        //                viewCounter++;
        //                fileExist = true;
        //                break;
        //            }
        //        if (!fileExist)
        //            return name;
        //    }
        //}

        ///// <summary>
        ///// Gets a list of block names recursively from base template using ASTNode - BlockNameNode 
        ///// </summary>
        ///// <param name="template">temporary template to parse or file path to that template</param>
        ///// <returns></returns>
        //public List<string> GetTemplateBlocks(string template)
        //{
        //    var blocks = SimpleTemplateParser.GetTemplateBlocks(template);
        //    return blocks;
        //}

        //public IEnumerable<string> GetTemplates(string root)
        //{
        //    return templatesDir.GetTemplates(root);
        //} 
        #endregion

        void IWizard.BeforeOpeningFile(ProjectItem projectItem)
        {
            // Do nothing
        }

        void IWizard.ProjectFinishedGenerating(Project project)
        {
            // Do nothing
        }

        void IWizard.ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            // Do nothing
        }

        void IWizard.RunFinished()
        {
            // Do nothing
        }

        void IWizard.RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {   
            //Assembly a = Assembly.Load("NDjango.Designer");
            Assembly a = Assembly.LoadFile(@"c:\Users\sivanov\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\Hill30\NDjango Template Editor\1.0\NDjango.Designer.dll");
            Object command = a.CreateInstance("NDjango.Designer.Commands.AddViewCommand");
            ((IWizard)command).RunStarted(automationObject, replacementsDictionary, runKind, customParams);

            //SI: calling with empty string as the implementation does not require path to give the answer
            shouldAddProjectItem = ((IWizard)command).ShouldAddProjectItem(""); 
        }

        bool IWizard.ShouldAddProjectItem(string filePath)
        {
            return shouldAddProjectItem;
        }
    }
}
