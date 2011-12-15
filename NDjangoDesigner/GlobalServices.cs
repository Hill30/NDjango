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

        public static readonly IVsRunningDocumentTable RDT = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable)); 

        public static TaskProvider TaskList {get; private set;}

        public static readonly DynamicTypeService TypeService = (DynamicTypeService)Package.GetGlobalService(typeof(DynamicTypeService)); 

        public static readonly IVsMonitorSelection SelectionTracker = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));

        public static readonly IVsSolution Solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));

        public static readonly IVsObjectManager2 ObjectManager = (IVsObjectManager2)Package.GetGlobalService(typeof(SVsObjectManager));

        private static SVsServiceProvider serviceProvider;

        [Import]
        private SVsServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set
            {
                serviceProvider = value;
                TaskList = new TaskProvider(serviceProvider);
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
