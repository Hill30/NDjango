using System;
using EnvDTE;
using Microsoft.SymbolBrowser.ObjectLists;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;

namespace Microsoft.SymbolBrowser
{
    [Guid("D918B9AC-1574-47BC-8CE8-3CFFD4073E88")]
    public class Library : IVsSimpleLibrary2
    {
        private const string SUPPORTED_EXT = ".django";
        private SymbolNode root;
        private RootNode objRoot;
        SymbolNode namespaceNode;
        ModelNode classNode;
        MemberNode memberNode;

        public Library()
        {
            CreateSearchNodes();
            CreateObjectManagerNodes();
            
            //GetSupportedFileList();
        }

        private void CreateObjectManagerNodes()
        {
            objRoot = new RootNode("Test template", "", "testTemplace.zzz", 0, 0);
            objRoot.Children.Add(new SymbolNode("ClassLibrary1", "", "Class1.cs", 7, 0,
                SymbolNode.LibraryNodeType.Namespaces));
            objRoot.Children[0].Children.Add(new SymbolNode("Class1", "ClassLibrary1.", "Class1.cs", 7, 17,
                SymbolNode.LibraryNodeType.Classes));
            objRoot.Children[0].Children[0].Children.Add(new SymbolNode("GetBlaBlaBla", "ClassLibrary1.Class1.", "Class1.cs",
                9, 22, SymbolNode.LibraryNodeType.Members));
            objRoot.isObjectBrowserNode = objRoot.Children[0].isObjectBrowserNode = objRoot.Children[0].Children[0].isObjectBrowserNode = true;
        }

        private void CreateSearchNodes()
        {
            // "flat" structure
            root = new SymbolNode("Test template", "", "testTemplace.zzz", 0, 0, SymbolNode.LibraryNodeType.Hierarchy);

            namespaceNode = new NamespaceNode("ClassLibrary1", "", "Class1.cs", 7, 0);
            classNode = new ModelNode("Class1", "ClassLibrary1.", "Class1.cs", 7, 17);
            memberNode = new MemberNode("GetBlaBlaBla", "ClassLibrary1.Class1.", "Class1.cs", 9, 22);

            ModelReferenceList
                classReferenceNode = new ModelReferenceList(
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs - (8, 18) : public class Class1(NDjango symbol)",
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs",
                    "", 8, 18),
                classReferenceNode1 = new ModelReferenceList(
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class2.cs - (7, 13) : Class1 c1 = new Class1();(NDjango symbol)",
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class2.cs",
                    "", 7, 13);
            classNode.AddChild(classReferenceNode);
            classNode.AddChild(classReferenceNode1);

            MemberReferenceList
                methodReferenceNode = new MemberReferenceList(
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs - (10, 23) : public Class1 GetBlaBlaBla()(NDjango symbol)",
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs",
                    "", 10, 23),
                methodReferenceNode1 = new MemberReferenceList(
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs - (0, 0) : public Class1 GetBlaBlaBla()(NDjango symbol)",
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs",
                    "", 0, 0),
                methodReferenceNode2 = new MemberReferenceList(
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs - (0, 0) : public Class1 GetBlaBlaBla()(NDjango symbol)",
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs",
                    "", 0, 0);

            memberNode.AddChild(methodReferenceNode);
            memberNode.AddChild(methodReferenceNode1);
            memberNode.AddChild(methodReferenceNode2);

            NamespaceReferenceList
                nsl1 = new NamespaceReferenceList(
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs - (2, 2) (NDjango symbol)",
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs",
                    "", 0, 0),
                nsl2 = new NamespaceReferenceList(
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class2.cs - (3, 3) (NDjango symbol)",
                    @"C:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class2.cs",
                    "", 0, 0);

            namespaceNode.AddChild(nsl1);
            namespaceNode.AddChild(nsl2);

            root.AddChild(memberNode);
            root.AddChild(classNode);
            root.AddChild(namespaceNode);
        }

        public void AddExternalReference(string symbol, IVsObjectList2 listToUse)
        {
            bool found = false;
            foreach (var c in root.Children)
            {
                if (string.Compare(symbol, c.UniqueName, false) == 0)
                {
                    c.ListToReference = listToUse;
                    found = true;
                    break;
                }
                foreach (var c2 in c.Children)
                {
                    if (string.Compare(symbol, c2.UniqueName, false) == 0)
                    {
                        c2.ListToReference = listToUse;
                        found = true;
                        break;
                    }
                }
            }
            if(!found)
                
                throw new IndexOutOfRangeException(String.Format("Could not find symbol with text {0}", symbol));
        }

        #region ...
        //SI: This should be performed using NDjango means
        //private ProjectItems GetSupportedFileList()
        //{
        //    foreach (Project p in SymbolBrowserPackage.DTE2Obj.Solution.Projects)
        //    {

        //        Logger.Log("Project: " + p.FullName);
        //        foreach (ProjectItem pi in p.ProjectItems)
        //        {
        //            Logger.Log("Project item");
        //            Logger.Log("File count: " + pi.FileCount);
        //            Logger.Log("File names: ");

        //            for (short i = 0; i < pi.FileCount; i++)
        //                Logger.Log(pi.FileNames[i]);
        //        }

        //        // This cast resulted in an exception
        //        //var a = ((IVsHierarchy) p).GetHashCode();

        //        //("*.django");
        //        /*
        //         * Project item
        //            File count: 1
        //            File names: 
        //            C:\projects\NDjango_copy\NDjangoDesigner\GlobalServices.cs
        //            Project item
        //            File count: 1
        //            File names: 
        //            C:\projects\NDjango_copy\NDjangoDesigner\GlobalSuppressions.cs
        //            Project item
        //            File count: 1
        //            File names: 
        //            C:\projects\NDjango_copy\NDjangoDesigner\ItemTemplates\

        //         * */
        //    }
        //    return null;
        //} 
        #endregion

        #region IVsSimpleLibrary2 Members

        public int AddBrowseContainer(VSCOMPONENTSELECTORDATA[] pcdComponent, ref uint pgrfOptions, out string pbstrComponentAdded)
        {
            throw new NotImplementedException();
        }

        public int CreateNavInfo(SYMBOL_DESCRIPTION_NODE[] rgSymbolNodes, uint ulcNodes, out IVsNavInfo ppNavInfo)
        {
            ppNavInfo = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetBrowseContainersForHierarchy(IVsHierarchy pHierarchy, uint celt, VSBROWSECONTAINER[] rgBrowseContainers, uint[] pcActual = null)
        {
            throw new NotImplementedException();
        }

        public int GetGuid(out Guid pguidLib)
        {
            pguidLib = GetType().GUID;
            return VSConstants.S_OK;
        }

        public int GetLibFlags2(out uint pgrfFlags)
        {
            // Same set as C# library has
            pgrfFlags =
                (uint)_LIB_FLAGS.LF_EXPANDABLE
                |(uint)_LIB_FLAGS.LF_PROJECT
                | (uint)_LIB_FLAGS2.LF_SUPPORTSBASETYPES
                | (uint)_LIB_FLAGS2.LF_SUPPORTSCLASSDESIGNER
                | (uint)_LIB_FLAGS2.LF_SUPPORTSFILTERING
                | (uint)_LIB_FLAGS2.LF_SUPPORTSINHERITEDMEMBERS
                | (uint)_LIB_FLAGS2.LF_SUPPORTSLISTREFERENCES
                | (uint)_LIB_FLAGS2.LF_SUPPORTSPRIVATEMEMBERS
                | (uint)_LIB_FLAGS2.LF_SUPPORTSPROJECTREFERENCES;
            return VSConstants.S_OK;
        }

        public int GetList2(uint ListType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2)
        {
            #region ...
            /*
            string strSearchCriteria = pobSrch[0].szName;
            uint grfOptions = pobSrch[0].grfOptions;
            IVsNavInfo NavInfo = pobSrch[0].pIVsNavInfo;

            // Return generated list of symbols to the object manager.
            ResultList resultsList = new RootMethodsList(this, grfOptions);
            ppIVsSimpleObjectList2 = (Microsoft.VisualStudio.Shell.Interop.IVsSimpleObjectList2)(resultsList);
            string strFullNameFromNavInfo;

            if (((uint)(grfOptions & (uint)Microsoft.VisualStudio.Shell.Interop._VSOBSEARCHOPTIONS2.VSOBSO_CALLSFROM) > 0) &&
                ((uint)(grfOptions & (uint)Microsoft.VisualStudio.Shell.Interop._VSOBSEARCHOPTIONS2.VSOBSO_CALLSTO) > 0))
            {
                // Initial view with VSOBSO_CALLSFROM and VSOBSO_CALLSTO flags set simultaneously.
                // Generate list of all methods in the container.

                foreach (CallInstance call in m_CallGraph)
                {
                    resultsList.AddMethod(call.m_Source);
                    resultsList.AddMethod(call.m_Target);
                }

            }
            // Generate CALLFROM list for Call graph.
            else if ((uint)(grfOptions & (uint)Microsoft.VisualStudio.Shell.Interop._VSOBSEARCHOPTIONS2.VSOBSO_CALLSFROM) > 0)
            {
                System.Collections.Generic.List<CallInstance> Calls = null;

                if (NavInfo != null)
                {
                    strFullNameFromNavInfo = CallBrowserNavInfo.GetFullNameFromNavInfo(NavInfo);
                    Calls = m_CallGraph.GetCallGraph(strFullNameFromNavInfo);
                }
                else if (strSearchCriteria.Length > 0)
                {
                    Calls = m_CallGraph.GetCallGraph(strSearchCriteria);
                }

                for (int i = 0; i < Calls.Count; i++)
                {
                    Method method = Calls[i].m_Source;
                    resultsList.AddMethod(method);
                }
            }
            // Generate CALLTO list for Callers graph.

            else if ((uint)(grfOptions & (uint)Microsoft.VisualStudio.Shell.Interop._VSOBSEARCHOPTIONS2.VSOBSO_CALLSTO) > 0)
            {
                System.Collections.Generic.List<CallInstance> Callers = null; // = new System.Collections.Generic.List<CallInstance>();

                if (NavInfo != null)
                {
                    strFullNameFromNavInfo = CallBrowserNavInfo.GetFullNameFromNavInfo(NavInfo);
                    Callers = m_CallGraph.GetCallersGraph(strFullNameFromNavInfo);
                }
                else if (strSearchCriteria.Length > 0)
                {
                    Callers = m_CallGraph.GetCallersGraph(strSearchCriteria);
                }

                for (int i = 0; i < Callers.Count; i++)
                {
                    Method method = Callers[i].m_Target;
                    resultsList.AddMethod(method);
                }
            }

            return VSConstants.S_OK;
            */
            
            #endregion

            var listTypeName = Enum.GetName(typeof(_LIB_LISTTYPE), ListType);
            if (listTypeName == "")
                listTypeName = Enum.GetName(typeof(_LIB_LISTTYPE2), ListType);

            Logger.Log(string.Format("GetList2 : Library ListType:{0}({1}) flags: {2}",
                Enum.GetName(typeof(_LIB_LISTTYPE2), ListType), 
                Enum.GetName(typeof(_LIB_LISTTYPE), ListType),
                Enum.GetName(typeof(_LIB_LISTFLAGS), flags)));
            if (pobSrch != null)
                ppIVsSimpleObjectList2 = root.FilterView((SymbolNode.LibraryNodeType)ListType, pobSrch);
            else
                ppIVsSimpleObjectList2 = objRoot;
            return VSConstants.S_OK;


            #region ...
            //for (var i = 0; i < root.Children.Count; i++) {
            //    if (root.Children[i].NodeType == (ResultList.LibraryNodeType)ListType)
            //      root.GetList2(i, ListType, flags, pobSrch, out ppIVsSimpleObjectList2);
            //}


            //if (pobSrch != null)
            //{
            //    /*
            //    if (pobSrch != null)
            //    {
            //    //var txt = pobSrch[0].szName;
            //    //string temp = string.Empty;                

            //    //root.GetTextWithOwnership(0, VSTREETEXTOPTIONS.TTO_DEFAULT, out temp);
            //    //if (string.Compare(temp, txt, true) == 0)
            //    //    ppIVsSimpleObjectList2 = root;
            //    //else
            //    //{
            //    //    namespaceNode.GetTextWithOwnership(0, VSTREETEXTOPTIONS.TTO_DEFAULT, out temp);
            //    //    if (string.Compare(temp, txt, true) == 0)
            //    //        ppIVsSimpleObjectList2 = namespaceNode;
            //    //    else
            //    //    {
            //    //        classNode.GetTextWithOwnership(0, VSTREETEXTOPTIONS.TTO_DEFAULT, out temp);
            //    //        if (string.Compare(temp, txt, true) == 0)
            //    //            ppIVsSimpleObjectList2 = classNode;
            //    //        else
            //    //        {
            //    //            ppIVsSimpleObjectList2 = null;
            //    //            return VSConstants.E_FAIL;
            //    //        }
            //    //    }
            //    //}             
            //    for (uint i = 0; i < (uint)root.Children.Count; i++)
            //    { 
            //        if (string.Compare(root.Children[(int)i].SymbolText, pobSrch[0].szName, true) == 0)
            //        {
            //            IVsSimpleObjectList2 list;
            //            root.GetList2(i, ListType, flags, null, out list);
            //            ppIVsSimpleObjectList2 = list;
            //            return VSConstants.S_OK;
            //        }
            //    }

            //    ppIVsSimpleObjectList2 = null;
            //    return VSConstants.E_FAIL;
            //}
            //     * */
            //    var txt = pobSrch[0].szName;
            //    string temp = string.Empty;                

            //    root.GetTextWithOwnership(0, VSTREETEXTOPTIONS.TTO_DEFAULT, out temp);
            //    if (string.Compare(temp, txt, true) == 0)
            //        ppIVsSimpleObjectList2 = root;
            //    else
            //    {
            //        namespaceNode.GetTextWithOwnership(0, VSTREETEXTOPTIONS.TTO_DEFAULT, out temp);
            //        if (string.Compare(temp, txt, true) == 0)
            //            ppIVsSimpleObjectList2 = namespaceNode;
            //        else
            //        {
            //            classNode.GetTextWithOwnership(0, VSTREETEXTOPTIONS.TTO_DEFAULT, out temp);
            //            if (string.Compare(temp, txt, true) == 0)
            //                ppIVsSimpleObjectList2 = classNode;
            //            else
            //            {
            //                ppIVsSimpleObjectList2 = null;
            //                return VSConstants.E_FAIL;
            //            }
            //        }
            //    }

            //    return VSConstants.S_OK;
            //}
            //else
            //{
            //    ppIVsSimpleObjectList2 = root;
            //    return VSConstants.S_OK;
            //}

            //switch (ListType)
            //{
            //    case (uint)_LIB_LISTTYPE.LLT_PHYSICALCONTAINERS: //16
            //        ppIVsSimpleObjectList2 = root;
            //        return VSConstants.S_OK;
            //    case (uint)_LIB_LISTTYPE.LLT_NAMESPACES: //2
            //        ppIVsSimpleObjectList2 = null;
            //        return VSConstants.S_OK;
            //    case (uint)_LIB_LISTTYPE.LLT_CLASSES: //??
            //        ppIVsSimpleObjectList2 = null;
            //        return VSConstants.S_OK;
            //    case (uint)_LIB_LISTTYPE.LLT_MEMBERS: //8
            //        ppIVsSimpleObjectList2 = null;
            //        return VSConstants.E_FAIL;
            //    case (uint)_LIB_LISTTYPE.LLT_REFERENCES: //8192
            //        ppIVsSimpleObjectList2 = null;
            //        return VSConstants.E_FAIL;

            //    default:
            //        ppIVsSimpleObjectList2 = null;
            //        return VSConstants.E_FAIL;
            //} 
            #endregion
        }

        public int GetSeparatorStringWithOwnership(out string pbstrSeparator)
        {
            pbstrSeparator = ".";
            return VSConstants.S_OK;
        }

        public int GetSupportedCategoryFields2(int Category, out uint pgrfCatField)
        {
            // Copied from C# library
            #region Log part
            /*
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_ACTIVEPROJECT - LCAP_SHOWALWAYS(1)
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_CLASSACCESS - (47)
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_CLASSTYPE - (574)
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_CLASSTYPE as _LIBCAT_CLASSTYPE2 - (574)
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_LISTTYPE - (31)
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_MEMBERACCESS - (47)
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_MEMBERTYPE - (25)
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_MODIFIER - (24)
            1/11/2012 8:44 PM: C# LIB_CATEGORY LC_VISIBILITY - (3)
            1/11/2012 8:44 PM: C# LIB_CATEGORY2 LC_HIERARCHYTYPE - (12)
            1/11/2012 8:44 PM: C# LIB_CATEGORY2 LC_HIERARCHYTYPE as _LIBCAT_HIERARCHYTYPE2 - (12)
            1/11/2012 8:44 PM: C# LIB_CATEGORY2 LC_Last2 - 0(0)
            1/11/2012 8:44 PM: C# LIB_CATEGORY2 LC_MEMBERINHERITANCE - (33)
            1/11/2012 8:44 PM: C# LIB_CATEGORY2 LC_NIL - 0(0)
            1/11/2012 8:44 PM: C# LIB_CATEGORY2 LC_PHYSICALCONTAINERTYPE - (7)
            1/11/2012 8:44 PM: C# LIB_CATEGORY2 LC_SEARCHMATCHTYPE - (0)
             */
            #endregion
            switch (Category)
            {
                case (int)LIB_CATEGORY.LC_MEMBERTYPE:
                    pgrfCatField = 25;
                    break;

                case (int)LIB_CATEGORY.LC_MEMBERACCESS:
                    pgrfCatField = 47;
                    break;

                case (int)LIB_CATEGORY.LC_LISTTYPE:
                    // 31
                    pgrfCatField = (uint)(_LIB_LISTTYPE.LLT_PHYSICALCONTAINERS | _LIB_LISTTYPE.LLT_PACKAGE | _LIB_LISTTYPE.LLT_MEMBERS | _LIB_LISTTYPE.LLT_CLASSES | _LIB_LISTTYPE.LLT_NAMESPACES | _LIB_LISTTYPE.LLT_HIERARCHY);
                    break;

                case (int)LIB_CATEGORY.LC_VISIBILITY:
                    // 3
                    pgrfCatField = (uint)(_LIBCAT_VISIBILITY.LCV_VISIBLE | _LIBCAT_VISIBILITY.LCV_HIDDEN);
                    break;
                case (int)_LIB_CATEGORY2.LC_HIERARCHYTYPE:
                    // 12
                    pgrfCatField = (uint)(_LIBCAT_HIERARCHYTYPE.LCHT_PROJECTREFERENCES | _LIBCAT_HIERARCHYTYPE.LCHT_BASESANDINTERFACES);
                    break;
                case (int)LIB_CATEGORY.LC_CLASSTYPE:
                    pgrfCatField = (uint)574; // _LIBCAT_CLASSTYPE.LCCT_DELEGATE | _LIBCAT_CLASSTYPE.LCCT_ENUM |  _LIBCAT_CLASSTYPE.LCCT_UNION | _LIBCAT_CLASSTYPE.LCCT_STRUCT | _LIBCAT_CLASSTYPE.LCCT_INTERFACE | _LIBCAT_CLASSTYPE.LCCT_CLASS
                    break;
                case (int)LIB_CATEGORY.LC_CLASSACCESS:
                    pgrfCatField = (uint)47; //_LIBCAT_CLASSACCESS.LCCA_SEALED | _LIBCAT_CLASSACCESS.LCCA_PUBLIC | _LIBCAT_CLASSACCESS.LCCA_PROTECTED | _LIBCAT_CLASSACCESS.LCCA_PRIVATE | _LIBCAT_CLASSACCESS.LCCA_PACKAGE;
                    break;
                case (int)_LIB_CATEGORY2.LC_MEMBERINHERITANCE:
                    pgrfCatField = (uint)33; //_LIBCAT_MEMBERINHERITANCE.LCMI_INHERITED | _LIBCAT_MEMBERINHERITANCE.LCMI_IMMEDIATE;
                    break;
                case (int)_LIB_CATEGORY2.LC_PHYSICALCONTAINERTYPE:
                    pgrfCatField = (uint)7; //_LIBCAT_PHYSICALCONTAINERTYPE.LCPT_GLOBAL | _LIBCAT_PHYSICALCONTAINERTYPE.LCPT_PROJECTREFERENCE | _LIBCAT_PHYSICALCONTAINERTYPE.LCPT_PROJECT;
                    break;
                default:
                    pgrfCatField = 0;
                    return VSConstants.E_FAIL;
            }
            return VSConstants.S_OK;
        }

        public int LoadState(VisualStudio.OLE.Interop.IStream pIStream, LIB_PERSISTTYPE lptType)
        {
            throw new NotImplementedException();
        }

        public int RemoveBrowseContainer(uint dwReserved, string pszLibName)
        {
            throw new NotImplementedException();
        }

        public int SaveState(VisualStudio.OLE.Interop.IStream pIStream, LIB_PERSISTTYPE lptType)
        {
            throw new NotImplementedException();
        }

        private uint updateCounter;
        public int UpdateCounter(out uint pCurUpdate)
        {
            pCurUpdate = updateCounter;
            return VSConstants.S_OK;
        }

        #endregion
    }
}
