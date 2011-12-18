using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using System.Reflection;
using Microsoft.VisualStudio.Shell.Interop;
using NDjango.Designer.Parsing.TypeLibrary;
using NDjango.Interfaces;
using System.IO;

namespace NDjango.Designer.Parsing
{
    public class TypeResolver : NDjango.TypeResolver.ITypeResolver, IDisposable
    {
        Microsoft.VisualStudio.Shell.Design.ProjectTypeResolutionService typeResolver;
        readonly IDisposable container;

        public TypeResolver(IVsHierarchy hier)
        {
            container = GlobalServices.TypeService.GetContextTypeResolver(hier);
            typeResolver = (Microsoft.VisualStudio.Shell.Design.ProjectTypeResolutionService)GlobalServices.TypeService.GetTypeResolutionService(hier);

            string path = typeof(TemplateManagerProvider).Assembly.CodeBase;
            if (path.StartsWith("file:///"))
                foreach (string file in
                    Directory.EnumerateFiles(
// ReSharper disable AssignNullToNotNullAttribute
                        Path.GetDirectoryName(path.Substring(8)),
// ReSharper restore AssignNullToNotNullAttribute
                        "*.NDjangoExtension40.dll",
                        SearchOption.AllDirectories))
                {
                    var name = new AssemblyName {CodeBase = file};
                    foreach (var t in Assembly.Load(name).GetExportedTypes())
                    {
                        if (typeof(ITag).IsAssignableFrom(t))
                            CreateEntry(tags, t);
                        if (typeof(ISimpleFilter).IsAssignableFrom(t))
                            CreateEntry(filters, t);
                    }
                }

        }

        readonly List<Tag> tags = new List<Tag>();
        readonly List<Filter> filters = new List<Filter>();

        private static void CreateEntry<T>(List<T> list, Type t) where T : class
        {
            if (t.IsAbstract)
                return;
            if (t.IsInterface)
                return;

            if (t.GetConstructor(new Type[] { }) == null)
                return;

            var attrs = t.GetCustomAttributes(typeof(NameAttribute), false) as NameAttribute[];
            if (attrs == null || attrs.Length == 0)
                return;

            list.Add((T)Activator.CreateInstance(typeof(T), attrs[0].Name, Activator.CreateInstance(t)));
        }

        public IEnumerable<Tag> Tags { get { return tags; } }

        public IEnumerable<Filter> Filters { get { return filters; } }

        #region IDisposable Members

        public void Dispose()
        {
            container.Dispose();
        }

        #endregion

        #region ITypeResolver Members

        public Type Resolve(string typeName)
        {
            return SearchLibaries(typeName);
        }

        public static NDjangoType SearchLibaries(string classname)
        {
            var type = new NDjangoType(classname);
            foreach (var library in GetIVsLibraries())
            {
                IVsSimpleObjectList2 list;
                if (!ErrorHandler.Succeeded(library.GetList2((uint)(_LIB_LISTTYPE.LLT_MEMBERS),
                                    (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                                    new[]
                                {
                                    new VSOBSEARCHCRITERIA2
                                        {
                                            eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
                                            grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_NONE,
                                            szName = "*"
                                        }
                                }, out list)))
                    continue;
                if (list == null) continue;

                uint count;
                ErrorHandler.ThrowOnFailure(list.GetItemCount(out count));
                for (var i = (uint)0; i < count; i++)
                {
                    object symbol;
                    ErrorHandler.ThrowOnFailure(list.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME, out symbol));

                    if (symbol != null)
                    {
                        string sSym = symbol.ToString();
                        if (sSym.StartsWith(classname))
                        {
                            type.AddMember(typeof(string), sSym.Split('.').Last());
                        }
                    }

                }
            }
            return type;
        }

        public static readonly Guid CSharpLibrary = new Guid("58f1bad0-2288-45b9-ac3a-d56398f7781d");
        public static readonly Guid VisualBasicLibrary = new Guid("414AC972-9829-4B6A-A8D7-A08152FEB8AA");

        public static IEnumerable<IVsSimpleLibrary2> GetIVsLibraries()
        {
            return new [] {CSharpLibrary /*, VisualBasicLibrary */}.Select(GetIVsLibrary).Where(lib => lib != null);
        }


        public static IVsSimpleLibrary2 GetIVsLibrary(Guid guid)
        {
            IVsLibrary2 library;
            if (!ErrorHandler.Succeeded(GlobalServices.ObjectManager.FindLibrary(ref guid, out library)))
                return null;
            return library as IVsSimpleLibrary2;
        }



        #endregion
    }
}
