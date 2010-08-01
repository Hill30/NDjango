using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Design;

namespace NDjango.Designer
{
    [Export]
    public class GlobalServices
    {

        private static GlobalServices handler = new GlobalServices();
        
        public static IVsRunningDocumentTable RDT {get; private set;}

        public static TaskProvider TaskList {get; private set;}

        public static DynamicTypeService TypeService { get; private set; }

        public static IVsMonitorSelection SelectionTracker { get { return (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection)); } }

        public static IVsSolution Solution { get; private set; }

        private static SVsServiceProvider serviceProvider;

        [Import]
        private SVsServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set
            {
                serviceProvider = value;
                TaskList = new TaskProvider(serviceProvider);
                RDT = GetService<IVsRunningDocumentTable>(typeof(SVsRunningDocumentTable));
                TypeService = GetService<DynamicTypeService>();
                Solution = GetService<IVsSolution>();
            }
        }

        public T GetService<T>()
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }

        public T GetService<T>(Type serviceType)
        {
            return (T)ServiceProvider.GetService(serviceType);
        }

    }
}
