using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;
using System.Web.SessionState;

namespace NDjango.Designer.Parsing
{
    public class ASPNETMVCProjectHandler : ProjectHandler
    {
        public ASPNETMVCProjectHandler(NodeProviderBroker broker, IVsHierarchy hier, string projectDirectory)
            : base(broker, hier, projectDirectory)
        { }

        protected override IEnumerable<NDjango.TypeResolver.IDjangoType> GetDefaultModel(string filename)
        {
            return new NDjango.TypeResolver.IDjangoType[] 
            {
                new NDjango.TypeResolver.CLRTypeDjangoType("Session", typeof(HttpSessionState))
            };
        }
    }
}
