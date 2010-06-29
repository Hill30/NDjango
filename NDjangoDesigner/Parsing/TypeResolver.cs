using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace NDjango.Designer.Parsing
{
    public class TypeResolver : NDjango.TypeResolver.AbstractTypeResolver
    {
        ITypeResolutionService type_resolver;
        public TypeResolver(ITypeResolutionService type_resolver)
        {
            this.type_resolver = type_resolver;
        }

        public override Type GetType(string type_name)
        {
            return type_resolver.GetType(type_name);
        }

    }
}
