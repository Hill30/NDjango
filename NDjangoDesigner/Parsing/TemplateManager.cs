using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.IO;
using System.Runtime.InteropServices;

namespace NDjango.Designer.Parsing
{
    public interface ITemplateManager
    {
        IEnumerable<string> GetTemplates(string root);
        IEnumerable<string> Recent5Templates { get; }
        void RegisterInserted(string inserted);
    }

    [Export(typeof(ITemplateManager))]
    public class TemplateManager : ITemplateManager
    {
        public TemplateManager() { }

        [Import]
        private SVsServiceProvider serviceProvider;


        #region ITemplateManager Members

        public IEnumerable<string> GetTemplates(string root)
        {
            var result = new List<string>();
            var selectionTracker = (IVsMonitorSelection)serviceProvider.GetService(typeof(SVsShellMonitorSelection));
            IntPtr ppHier;
            uint pitemid;
            IVsMultiItemSelect ppMIS;
            IntPtr ppSC;
            object directory = ""; 
            if (ErrorHandler.Succeeded(selectionTracker.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC)))
            {
                try
                {
                    IVsHierarchy hier = (IVsHierarchy)Marshal.GetObjectForIUnknown(ppHier);
                    if (ErrorHandler.Succeeded(hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out directory)))
                    {
                        result.AddRange(Directory.EnumerateFiles(directory + root, "*.django", SearchOption.AllDirectories));
                        result.AddRange(Directory.EnumerateFiles(directory + root, "*.htm", SearchOption.AllDirectories));
                        result.AddRange(Directory.EnumerateFiles(directory + root, "*.html", SearchOption.AllDirectories));
                    }
                }
                finally
                {
                    Marshal.Release(ppHier);
                    Marshal.Release(ppSC);
                }
            }
            return result.ConvertAll(file => file.Substring((directory + root).Length)); 
        }

        List<string> recent5 = new List<string>();

        public IEnumerable<string> Recent5Templates
        {
            get { return recent5; }
        }

        public void RegisterInserted(string inserted)
        {
            recent5.Remove(inserted);
            recent5.Insert(0, inserted);
            while (recent5.Count > 5)
                recent5.RemoveAt(5);
        }

        #endregion
    }
}
