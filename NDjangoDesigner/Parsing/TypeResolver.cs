using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;

namespace NDjango.Designer.Parsing
{
    public class TypeResolver : NDjango.TypeResolver.ITypeResolver
    {

        private SVsServiceProvider serviceProvider;

        private ITypeResolutionService type_resolver;

        [Import]
        internal SVsServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            private set
            {
                serviceProvider = value;
                type_resolver = GetService<ITypeResolutionService>();
            }
        }

        private T GetService<T>()
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }

        private T GetService<T>(Type serviceType)
        {
            return (T)ServiceProvider.GetService(serviceType);
        }


        #region ITypeResolver Members

        public IEnumerable<Interfaces.IDjangoType> Resolve(string type_name)
        {
            var type = type_resolver.GetType(type_name);
            foreach (var member in type.GetMembers())
                yield return new NDjango.TypeResolver.ValueType(member.Name);
        }

        #endregion
    }
}
