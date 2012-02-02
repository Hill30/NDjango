using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace ndjango.wizards
{
    class FormWizard:IWizard
    {
        private bool shouldAddProjectItem;

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
            FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            Assembly a = Assembly.LoadFile(fi.DirectoryName+"\\NDjango.Designer.dll");
            Object command = a.CreateInstance("NDjango.Designer.Commands.AddViewCommand");
            ((IWizard)command).RunStarted(automationObject, replacementsDictionary, runKind, customParams);

            // Calling with empty string as the implementation does not require path to give the answer
            shouldAddProjectItem = ((IWizard)command).ShouldAddProjectItem(string.Empty); 
        }

        bool IWizard.ShouldAddProjectItem(string filePath)
        {
            return shouldAddProjectItem;
        }
    }
}
