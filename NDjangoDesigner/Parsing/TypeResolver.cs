using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using Microsoft.VisualStudio.Shell.Interop;
using NDjango.Interfaces;
using System.IO;

namespace NDjango.Designer.Parsing
{
    public class TypeResolver : NDjango.Interfaces.ITypeResolver, IDisposable
    {
        Microsoft.VisualStudio.Shell.Design.ProjectTypeResolutionService type_resolver;
        ITypeDiscoveryService type_discovery;
        IDisposable container;

        public TypeResolver(IVsHierarchy hier)
        {
            container = GlobalServices.TypeService.GetContextTypeResolver(hier);
            type_resolver = (Microsoft.VisualStudio.Shell.Design.ProjectTypeResolutionService)GlobalServices.TypeService.GetTypeResolutionService(hier);
            type_discovery = GlobalServices.TypeService.GetTypeDiscoveryService(hier);

            string path = typeof(TemplateManagerProvider).Assembly.CodeBase;
            if (path.StartsWith("file:///"))
                foreach (string file in
                    Directory.EnumerateFiles(
                        Path.GetDirectoryName(path.Substring(8)),
                        "*.NDjangoExtension.dll",
                        SearchOption.AllDirectories))
                {
                    AssemblyName name = new AssemblyName();
                    name.CodeBase = file;
                    foreach (Type t in Assembly.Load(name).GetExportedTypes())
                    {
                        if (typeof(ITag).IsAssignableFrom(t))
                            CreateEntry<Tag>(tags, t);
                        if (typeof(ISimpleFilter).IsAssignableFrom(t))
                            CreateEntry<Filter>(filters, t);
                    }
                }

        }

        //var tags = new List<Tag>();

        //foreach (var tag in type_resolver.GetTypes(typeof(object), false))
        //{
        //    if (!typeof(ITag).IsAssignableFrom(tag)) continue;
        //    if (tag.IsAbstract) continue;
        //    if (tag.IsInterface) continue;
        //    if (tag.Assembly.FullName == "NDjango.Core") continue;
        //    var attrs = tag.GetCustomAttributes(typeof(NameAttribute), false);
        //    if (attrs.Length == 0) continue;
        //    tags.Add(new Tag(((NameAttribute)attrs[0]).Name, (ITag)Activator.CreateInstance(tag)));
        //}

        List<Tag> tags = new List<Tag>();
        List<Filter> filters = new List<Filter>();

        private static void CreateEntry<T>(List<T> list, Type t) where T : class
        {
            if (t.IsAbstract)
                return;
            if (t.IsInterface)
                return;

            var attrs = t.GetCustomAttributes(typeof(NameAttribute), false) as NameAttribute[];
            if (attrs.Length == 0)
                return;

            if (t.GetConstructor(new Type[] { }) == null)
                return;

            list.Add((T)Activator.CreateInstance(typeof(T), attrs[0].Name, Activator.CreateInstance(t)));
        }

        public IEnumerable<Tag> Tags { get { return tags; } }

        public IEnumerable<Filter> Filters { get { return filters; } }
        
        public IEnumerable<Type> GetTypes(Type base_type, bool excludeGlobalTypes)
        {
            return type_discovery.GetTypes(base_type, excludeGlobalTypes).Cast<Type>();
        }

        #region IDisposable Members

        public void Dispose()
        {
            container.Dispose();
        }

        #endregion

        #region ITypeResolver Members

        public Type Resolve(string type_name)
        {
            return ((ITypeResolutionService)type_resolver).GetType(type_name);
        }

        #endregion
    }
}
