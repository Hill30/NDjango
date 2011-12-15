using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using System.Reflection;
using Microsoft.VisualStudio.Shell.Interop;
using NDjango.Interfaces;
using System.IO;

namespace NDjango.Designer.Parsing
{
    public class TypeResolver : NDjango.TypeResolver.ITypeResolver, IDisposable
    {
        Microsoft.VisualStudio.Shell.Design.ProjectTypeResolutionService type_resolver;
        IDisposable container;

        public TypeResolver(IVsHierarchy hier)
        {
            container = GlobalServices.TypeService.GetContextTypeResolver(hier);
            type_resolver = (Microsoft.VisualStudio.Shell.Design.ProjectTypeResolutionService)GlobalServices.TypeService.GetTypeResolutionService(hier);

            string path = typeof(TemplateManagerProvider).Assembly.CodeBase;
            if (path.StartsWith("file:///"))
                foreach (string file in
                    Directory.EnumerateFiles(
                        Path.GetDirectoryName(path.Substring(8)),
                        "*.NDjangoExtension40.dll",
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

        readonly List<Tag> tags = new List<Tag>();
        readonly List<Filter> filters = new List<Filter>();

        private static void CreateEntry<T>(List<T> list, Type t) where T : class
        {
            if (t.IsAbstract)
                return;
            if (t.IsInterface)
                return;

            var attrs = t.GetCustomAttributes(typeof(NameAttribute), false) as NameAttribute[];
            if (attrs != null && attrs.Length == 0)
                return;

            if (t.GetConstructor(new Type[] { }) == null)
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
            NDjangoType type = new NDjangoType();
            var libs = GetIVsLibraries();
            foreach (var vsLibrary2 in libs)
            {
                try
                {
                    IVsObjectList2 list;
                    bool searchSucceed = ErrorHandler.Succeeded(vsLibrary2.GetList2((uint)(_LIB_LISTTYPE.LLT_MEMBERS),
                                        (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                                        new[]
                                        {
                                            new VSOBSEARCHCRITERIA2
                                                {
                                                    eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
                                                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_NONE,
                                                    szName = "*"
                                                }
                                        }, out list));


                    if (searchSucceed && list != null)
                    {
                        uint count;
                        ErrorHandler.Succeeded(list.GetItemCount(out count));
                        for (var i = (uint)0; i < count; i++)
                        {
                            object symbol;

                            ErrorHandler.Succeeded(list.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME, out symbol));

                            if (symbol != null)
                            {
                                string sSym = symbol.ToString();
                                if (sSym.StartsWith(classname))
                                {
                                    type.AddMember(sSym.Replace(classname, string.Empty));
                                }
                            }


                        }
                    }
                }
                catch (AccessViolationException accessViolationException) {/* eat this type of exception */}

            }
            return type;
        }


        public static readonly Guid CSharpLibrary = new Guid("58f1bad0-2288-45b9-ac3a-d56398f7781d");
        public static readonly Guid VisualBasicLibrary = new Guid("414AC972-9829-4B6A-A8D7-A08152FEB8AA");

        public static IVsLibrary2[] GetIVsLibraries()
        {
            List<IVsLibrary2> libraries = new List<IVsLibrary2>();

            IVsLibrary2 cSharpLibrary = GetIVsLibrary2(CSharpLibrary);
            if (cSharpLibrary != null)
                libraries.Add(cSharpLibrary);

            IVsLibrary2 vbLibrary = GetIVsLibrary2(VisualBasicLibrary);
            if (vbLibrary != null)
                libraries.Add(vbLibrary);

            return libraries.ToArray();
        }


        public static IVsLibrary2 GetIVsLibrary2(Guid guid)
        {
            IVsObjectManager2 objectManager = GlobalServices.ObjectManager;
            IVsLibrary2 library;
            objectManager.FindLibrary(ref guid, out library);
            return library;
        }



        #endregion
    }
}
