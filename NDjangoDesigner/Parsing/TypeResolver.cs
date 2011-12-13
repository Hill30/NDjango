using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel.Composition;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using Microsoft.VisualStudio.Shell.Interop;
using NDjango.Interfaces;
using System.IO;
using NDjangoDesigner;

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
        
        #region IDisposable Members

        public void Dispose()
        {
            container.Dispose();
        }

        #endregion

        #region ITypeResolver Members

        public Type Resolve(string type_name)
        {
            /**
             * get info from symbol lib (C# and VBet c)
             * 
             */
            //SearchLibaries(type_name);
            //return GetDummyTypeInfo();
            return ((ITypeResolutionService)type_resolver).GetType(type_name);
        }

        public NDjangoType GetDummyTypeInfo()
        {
            NDjangoType type = new NDjangoType();
            type.AddMember("UserName");
            type.AddMember("Test");
            return type;
        }

        public static void SearchLibaries(string text)
        {
            var libs = GetIVsLibraries(NDjangoDesignerPackage.DTE2Obj);
            foreach (var vsLibrary2 in libs)
            {
                try
                {
                    var list = SearchIVsLibrary(vsLibrary2, text, VSOBSEARCHTYPE.SO_ENTIREWORD);
                    if (list != null)
                    {
                        uint count;
                        ErrorHandler.Succeeded(list.GetItemCount(out count));
                        for (var i = (uint) 0; i < count; i++)
                        {
                            object propValue;
                            Type objType;
                            objType = list.GetType();
                            ErrorHandler.Succeeded(list.GetProperty(i,(int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME,out propValue));

                            IVsObjectList2 nestedObjects;
                            ErrorHandler.Succeeded(list.GetList2(
                                i,
                                (uint)(_LIB_LISTTYPE.LLT_HIERARCHY),
                                (uint) _LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                                new[]
                                    {
                                        new VSOBSEARCHCRITERIA2
                                            {
                                                eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
                                                grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_NONE,
                                                szName = "*"
                                            }
                                    },
                                out nestedObjects
                                                       ));

                            uint nestedCount;
                            ErrorHandler.Succeeded(nestedObjects.GetItemCount(out nestedCount));

                            for (var n = (uint) 0; n < nestedCount; n++)
                            {
                                object nestedPropValue;
                                ErrorHandler.Succeeded(nestedObjects.GetProperty(n,
                                                                                 (int)
                                                                                 _VSOBJLISTELEMPROPID.
                                                                                     VSOBJLISTELEMPROPID_FULLNAME,
                                                                                 out nestedPropValue));


                            }



                        }
                    }
                }
                catch (AccessViolationException accessViolationException){/* eat this type of exception */}
                
            }
        }

        public static IVsObjectList2 SearchIVsLibrary(IVsLibrary2 library, string keyword, VSOBSEARCHTYPE searchType)
        {
            try
            {
            
                VSOBSEARCHCRITERIA2[] searchCriteria = new VSOBSEARCHCRITERIA2[1];
                searchCriteria[0].eSrchType = searchType;
                searchCriteria[0].szName = keyword;

                IVsObjectList2 objectList = null;
                library.GetList2((uint)_LIB_LISTTYPE.LLT_CLASSES, (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER, searchCriteria, out objectList);
                return objectList;
            }
            catch (AccessViolationException accessViolationException) {/* eat this type of exception */}
            return null;
        }

        public static readonly Guid CSharpLibrary = new Guid("58f1bad0-2288-45b9-ac3a-d56398f7781d");
        public static readonly Guid VBLibrary = new Guid("414AC972-9829-4B6A-A8D7-A08152FEB8AA");

        public static IVsLibrary2[] GetIVsLibraries(DTE2 dte)
        {
            List<IVsLibrary2> libraries = new List<IVsLibrary2>();

            IVsLibrary2 cSharpLibrary = GetIVsLibrary2(dte, CSharpLibrary);
            if (cSharpLibrary != null)
                libraries.Add(cSharpLibrary);

            IVsLibrary2 vbLibrary = GetIVsLibrary2(dte, VBLibrary);
            if (vbLibrary != null)
                libraries.Add(vbLibrary);

            return libraries.ToArray();
        }


        public static IVsLibrary2 GetIVsLibrary2(DTE2 dte, Guid guid)
        {
            if (dte == null)
                throw new ArgumentNullException("dte");

            IVsObjectManager2 objectManager = GetIVsObjectManager2(dte);
            IVsLibrary2 library = null;
            objectManager.FindLibrary(ref guid, out library);
            return library;
        }

        private static IVsObjectManager2 GetIVsObjectManager2(DTE2 dte)
        {
            if (dte == null)
                throw new ArgumentNullException("dte");

            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte;
            Guid iid = typeof(IVsObjectManager2).GUID;
            Guid service = typeof(SVsObjectManager).GUID;
            IntPtr pUnk;
            sp.QueryService(ref service, ref iid, out pUnk);
            IVsObjectManager2 manager = (IVsObjectManager2)Marshal.GetObjectForIUnknown(pUnk);
            return manager;
        }

        #endregion
    }
}
