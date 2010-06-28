using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using System.Runtime.Remoting.Messaging;

namespace NDjango.Designer.Parsing
{
    public class TypeResolver : NDjango.Interfaces.ITypeResolver
    {

        #region ITypeResolver Members

        public IEnumerable<Interfaces.IDjangoType> Resolve(string type_name)
        {
            var type_resolver = (ITypeResolutionService)CallContext.GetData(typeof(TypeResolver).FullName);
            var type = type_resolver.GetType(type_name);
            foreach (var member in type.GetMembers())
                yield return new NDjango.TypeResolver.ValueDjangoType(member.Name);
        }

        #endregion
    }
}
