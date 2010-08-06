using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using NDjango.Interfaces;
using System.Web.SessionState;

namespace NDjango.Designer.Parsing
{
    public class ASPNETMVCProjectHandler : ProjectHandler
    {
        public ASPNETMVCProjectHandler(NodeProviderBroker broker, IVsHierarchy hier, string project_directory)
            : base(broker, hier, project_directory)
        { }

        protected override IEnumerable<IDjangoType> GetDefaultModel(string filename)
        {
            return new IDjangoType[] 
            {
                new NDjango.TypeResolver.CLRTypeDjangoType("Session", typeof(HttpSessionState))
            };
        }
    }
}
