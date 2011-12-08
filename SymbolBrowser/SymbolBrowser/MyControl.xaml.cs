using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class MyControl : UserControl
    {
        public MyControl()
        {
            InitializeComponent();
        }

        Library library;
        private uint libCookie;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var objectManager = SymbolBrowserPackage.GetGlobalService(typeof(SVsObjectManager)) as IVsObjectManager2;
            //if (library == null)
            //{
            library = new Library();
            objectManager.RegisterSimpleLibrary(library, out libCookie);
            //}

            IVsCombinedBrowseComponentSet extras;
            ErrorHandler.Succeeded(objectManager.CreateCombinedBrowseComponentSet(out extras));

            var solution = SymbolBrowserPackage.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

            IEnumHierarchies hiers;
            ErrorHandler.Succeeded(solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, Guid.Empty, out hiers));
            var projects = new IVsHierarchy[20];
            uint actualCount;
            ErrorHandler.Succeeded(hiers.Next((uint)projects.Length, projects, out actualCount));

            foreach (var project in projects)
            {
                IVsSimpleBrowseComponentSet subset;
                ErrorHandler.Succeeded(objectManager.CreateSimpleBrowseComponentSet(
                    (uint)_BROWSE_COMPONENT_SET_TYPE.BCST_EXCLUDE_LIBRARIES,
                    null, 0, out subset));

                ErrorHandler.Succeeded(subset.put_Owner(project));

                ErrorHandler.Succeeded(extras.AddSet(subset));
            }

            IVsEnumLibraries2 libs;
            ErrorHandler.Succeeded(objectManager.EnumLibraries(out libs));
            var libArray = new IVsLibrary2[20];
            uint fetched;
            libs.Next((uint)libArray.Length, libArray, out fetched);

            treeView1.Items.Clear();
            foreach (var lib in libArray)
            {
                if (lib == null)
                    continue;

                // adding symbol to C# library
                Guid g;
                ((IVsSimpleLibrary2)lib).GetGuid(out g);
                if (g.CompareTo(new Guid("58F1BAD0-2288-45b9-AC3A-D56398F7781D")) == 0)
                {
                    // search criteria
                    IVsObjectList2 outList;

                    Logger.Log("Symbol details for Class1");

                    //lib.GetList2(
                    //    (uint)_LIB_LISTTYPE.LLT_CLASSES,
                    //    (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                    //    new[]{
                    //            new VSOBSEARCHCRITERIA2
                    //                {
                    //                    eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
                    //                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
                    //                    szName = "Class1"
                    //                }
                    //        },
                    //        out outList);
                    outList = GetListFromLib(_LIB_LISTTYPE.LLT_CLASSES, VSOBSEARCHTYPE.SO_SUBSTRING, lib, "Class1", false);
                    Logger.Log(GetListDetails(outList));

                    Logger.Log("Symbol details for GetBlaBlaBla function");

                    //lib.GetList2(
                    //    (uint)_LIB_LISTTYPE.LLT_MEMBERS,
                    //    (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                    //    new[]{
                    //            new VSOBSEARCHCRITERIA2
                    //                {
                    //                    eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
                    //                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
                    //                    szName = "GetBlaBlaBla*"
                    //                }
                    //        },
                    //        out outList);
                    outList = GetListFromLib(_LIB_LISTTYPE.LLT_MEMBERS, VSOBSEARCHTYPE.SO_SUBSTRING, lib, "GetBlaBlaBla*", false);
                    Logger.Log(GetListDetails(outList));

                   
                    #region ...
                    //string projRef = string.Empty;
                    //solution.GetProjrefOfProject(projects[0], out projRef);

                    //VSCOMPONENTSELECTORDATA[] data = new VSCOMPONENTSELECTORDATA[]{
                    //    new VSCOMPONENTSELECTORDATA{
                    //        bstrFile = @"C:\Users\sivanov\documents\visual studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs",
                    //        bstrTitle = "TestItem",                    
                    //        dwSize = 16,
                    //        bstrProjRef = projRef
                    //    }
                    //};

                    //uint pgfrOptions = (uint)_LIB_ADDREMOVEOPTIONS.LARO_NONE;
                    //lib.AddBrowseContainer(data, ref pgfrOptions);
                    #endregion
                }

                AddLibrary(lib, extras);
            }

        }

        private void AddLibrary(IVsLibrary2 lib, IVsCombinedBrowseComponentSet extras)
        {
            if (lib == null)
                return;

            var simpleLib = lib as IVsSimpleLibrary2;


            Guid libGuid;
            ErrorHandler.Succeeded(simpleLib.GetGuid(out libGuid));

            var libRoot = new TreeViewItem { Header = "guid=(" + libGuid + ")" };
            var expander = new TreeViewItem();
            libRoot.Items.Add(expander);
            libRoot.Expanded += (sender, args) => AddLibraryContent(libRoot, expander, extras, lib);
            treeView1.Items.Add(libRoot);
        }

        private void AddLibraryContent(TreeViewItem libRoot, TreeViewItem expander, IVsCombinedBrowseComponentSet extras, IVsLibrary2 lib)
        {
            if (lib == null)
                return;

            if (libRoot.Items.Count != 1 || libRoot.Items[0] != expander)
                return;
            libRoot.Items.Clear();

            var simpleLib = lib as IVsSimpleLibrary2;

            uint libFlags;
            ErrorHandler.Succeeded(lib.GetLibFlags2(out libFlags));

            var flags = "";
            if ((libFlags & (uint)_LIB_FLAGS.LF_EXPANDABLE) != 0)
                flags += "|LF_EXPANDABLE";
            if ((libFlags & (uint)_LIB_FLAGS.LF_GLOBAL) != 0)
                flags += "|LF_GLOBAL";
            if ((libFlags & (uint)_LIB_FLAGS.LF_HIDEINLIBPICKER) != 0)
                flags += "|LF_HIDEINLIBPICKER";
            if ((libFlags & (uint)_LIB_FLAGS.LF_PROJECT) != 0)
                flags += "|LF_PROJECT";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSBASETYPES) != 0)
                flags += "|LF_SUPPORTSBASETYPES";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSCALLBROWSER) != 0)
                flags += "|LF_SUPPORTSCALLBROWSER";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSCLASSDESIGNER) != 0)
                flags += "|LF_SUPPORTSCLASSDESIGNER";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSDERIVEDTYPES) != 0)
                flags += "|LF_SUPPORTSDERIVEDTYPES";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSFILTERING) != 0)
                flags += "|LF_SUPPORTSFILTERING";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSFILTERINGWITHEXPANSION) != 0)
                flags += "|LF_SUPPORTSFILTERINGWITHEXPANSION";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSINHERITEDMEMBERS) != 0)
                flags += "|LF_SUPPORTSINHERITEDMEMBERS";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSLISTREFERENCES) != 0)
                flags += "|LF_SUPPORTSLISTREFERENCES";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSPRIVATEMEMBERS) != 0)
                flags += "|LF_SUPPORTSPRIVATEMEMBERS";
            if ((libFlags & (uint)_LIB_FLAGS2.LF_SUPPORTSPROJECTREFERENCES) != 0)
                flags += "|LF_SUPPORTSPROJECTREFERENCES";
            flags = flags.Substring(1);


            Guid libGuid;
            ErrorHandler.Succeeded(simpleLib.GetGuid(out libGuid));

            IVsNavInfo navInfo = null;

            var rc = VSConstants.E_NOTIMPL;
            //for (var i = (uint)0; navInfo==null && i<32 && rc != VSConstants.S_OK; i++)
            //{
            //    //rc = extras.CreateNavInfo(
            //    //    libGuid,
            //    //    new[]
            //    //        {
            //    //            new SYMBOL_DESCRIPTION_NODE {dwType = (uint)(0), pszName = "ClassLibrary12"},
            //    //        },
            //    //    1,
            //    //    out navInfo
            //    //    );
            //    //if (rc == VSConstants.S_OK)
            //    //    break;
            rc = extras.CreateNavInfo(
                libGuid,
                new[]
                        {
                            new SYMBOL_DESCRIPTION_NODE {dwType = (uint)_LIB_LISTTYPE.LLT_NAMESPACES, pszName = "ClassLibrary3"},
                        },
                1,
                out navInfo
                );
            //}

            //IVsObjectList2 someList;
            //extras.GetList2(
            //    (uint)_LIB_LISTTYPE.LLT_PHYSICALCONTAINERS,
            //    (uint)_LIB_LISTFLAGS.LLF_NONE, 
            //    null, 
            //    (IVsObjectList2)new Library().Root, 
            //    out someList);

            var navInfoRoot = new TreeViewItem { Header = "NavInfo (rc=" + rc + ")" };
            if (rc == VSConstants.S_OK)
            {
                Guid symbolGuid;
                ErrorHandler.Succeeded(navInfo.GetLibGuid(out symbolGuid));
                navInfoRoot.Items.Add("Guid=" + symbolGuid);
                uint symbolType;
                ErrorHandler.Succeeded(navInfo.GetSymbolType(out symbolType));
                var symbolTypeString = Enum.GetName(typeof(_LIB_LISTTYPE), symbolType);
                if (symbolTypeString != null)
                {
                    navInfoRoot.Items.Add("Type = _LIB_LISTTYPE." + symbolTypeString);
                }
                else
                {
                    symbolTypeString = Enum.GetName(typeof(_LIB_LISTTYPE2), symbolType);
                    if (symbolTypeString != null)
                    {
                        navInfoRoot.Items.Add("Type = _LIB_LISTTYPE2." + symbolTypeString);
                    }
                    else
                    {
                        navInfoRoot.Items.Add("Type = " + symbolType);
                    }
                }

                IVsEnumNavInfoNodes infoNodes;
                ErrorHandler.Succeeded(navInfo.EnumCanonicalNodes(out infoNodes));
                var navInfoNodesArray = new IVsNavInfoNode[20];
                uint fetched;
                ErrorHandler.Succeeded(infoNodes.Next((uint)navInfoNodesArray.Length, navInfoNodesArray, out fetched));
                if (fetched > 0)
                {
                    var navNodes = new TreeViewItem { Header = "Nodes" };
                    foreach (var node in navInfoNodesArray)
                    {
                        if (node == null)
                            continue;
                        string nodeName;
                        ErrorHandler.Succeeded(node.get_Name(out nodeName));
                        uint nodeType;
                        ErrorHandler.Succeeded(node.get_Type(out nodeType));
                        var nodeTypeString = Enum.GetName(typeof(_LIB_LISTTYPE), nodeType);
                        if (nodeTypeString != null)
                        {
                            navNodes.Items.Add(nodeName + "(_LIB_LISTTYPE." + nodeTypeString + ")");
                        }
                        else
                        {
                            nodeTypeString = Enum.GetName(typeof(_LIB_LISTTYPE2), symbolType);
                            if (symbolTypeString != null)
                            {
                                navNodes.Items.Add(nodeName + "(_LIB_LISTTYPE." + nodeTypeString + ")");
                            }
                            else
                            {
                                navNodes.Items.Add(nodeName + "(" + symbolType + ")");
                            }
                        }
                    }
                }

            }
            libRoot.Items.Add(navInfoRoot);

            libRoot.Items.Add("Flags=" + flags);

            IVsLiteTreeList globalLibs;
            ErrorHandler.Succeeded(lib.GetLibList(LIB_PERSISTTYPE.LPT_GLOBAL, out globalLibs));
            AddLibList(libRoot, "Global", globalLibs);

            IVsLiteTreeList projectLibs;
            ErrorHandler.Succeeded(lib.GetLibList(LIB_PERSISTTYPE.LPT_PROJECT, out projectLibs));
            AddLibList(libRoot, "Project", globalLibs);

            AddNested(lib, libRoot, _LIB_LISTTYPE.LLT_NAMESPACES);

            AddNested(lib, libRoot, _LIB_LISTTYPE.LLT_CLASSES);

            AddNested(lib, libRoot, _LIB_LISTTYPE.LLT_MEMBERS);

            AddNested(lib, libRoot, _LIB_LISTTYPE.LLT_REFERENCES);
            //expander.Items.Add(libRoot);


        }

        private void AddNested(IVsLibrary2 lib, TreeViewItem libRoot, _LIB_LISTTYPE listType)
        {
            var timestamp = DateTime.Now;
            IVsObjectList2 objects;
            ErrorHandler.Succeeded(lib.GetList2(
                (uint)listType,
                (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                new[]
                    {
                        new VSOBSEARCHCRITERIA2
                            {
                                eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
                                grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
                                szName = "*"
                            }
                    },
                out objects
                                        ));

            if (objects == null)
                return;

            // Yet dunno why but this crashes the VStudio
            //Logger.Log("Details for symbols form library " + libRoot.Header);
            //GetListDetails(objects);

            var root = new TreeViewItem { Header = listType };
            uint libFlags;
            ErrorHandler.Succeeded(objects.GetCapabilities2(out libFlags));

            var flags = "";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWDELETE) != 0)
                flags += "|LLC_ALLOWDELETE";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWDRAGDROP) != 0)
                flags += "|LLC_ALLOWDRAGDROP";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWRENAME) != 0)
                flags += "|LLC_ALLOWRENAME";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWSCCOPS) != 0)
                flags += "|LLC_ALLOWSCCOPS";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_HASBROWSEOBJ) != 0)
                flags += "|LLC_HASBROWSEOBJ";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_HASCOMMANDS) != 0)
                flags += "|LLC_HASCOMMANDS";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_HASDESCPANE) != 0)
                flags += "|LLC_HASDESCPANE";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_NONE) != 0)
                flags += "|LLC_NONE";
            if ((libFlags & (uint)_LIB_LISTCAPABILITIES2.LLC_ALLOWELEMENTSEARCH) != 0)
                flags += "|LLC_ALLOWELEMENTSEARCH";
            if (flags != "")
            {
                flags = flags.Substring(1);
                root.Items.Add("flags = " + flags);
            }

            uint count;
            ErrorHandler.Succeeded(objects.GetItemCount(out count));
            root.Items.Add("Items count: " + count);
            string filePath;

            for (var i = (uint)0; i < count; i++)
            {
                object propValue;
                string text;
                //objects.GetText(i, VSTREETEXTOPTIONS.TTO_BASETEXT, out text);
                Type objType = objects.GetType();

                ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_LEAFNAME, out propValue));
                var item = new TreeViewItem { Header = (string)propValue };
                //var item = new TreeViewItem { Header = "TExt" };
                item.Items.Add("Type: " + objType);

                ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME, out propValue));
                item.Items.Add("Full Name " + (string)propValue);
                ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_COMPONENTPATH, out propValue));
                item.Items.Add("Path " + (string)propValue);

                IVsObjectList2 nestedObjects;
                ErrorHandler.Succeeded(objects.GetList2(
                    i,
                    (uint)(_LIB_LISTTYPE.LLT_CLASSES),
                    (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                    new[]
                        {
                            new VSOBSEARCHCRITERIA2
                                {
                                    eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
                                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
                                    szName = "*"
                                }
                        },
                    out nestedObjects
                                            ));
                AddNested(item, nestedObjects);

                root.Items.Add(item);
            }

            root.Items.Insert(0, "Elapsed " + (DateTime.Now - timestamp).TotalMilliseconds);
            libRoot.Items.Add(root);


        }

        private void AddNested(TreeViewItem parent, IVsObjectList2 objects)
        {
            uint count;
            ErrorHandler.Succeeded(objects.GetItemCount(out count));

            for (var i = (uint)0; i < count; i++)
            {
                object propValue;
                ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_LEAFNAME, out propValue));
                var item = new TreeViewItem { Header = (string)propValue };
                ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME, out propValue));
                item.Items.Add("Full Name " + (string)propValue);
                ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_COMPONENTPATH, out propValue));
                item.Items.Add("Path " + (string)propValue);

            }

        }
        #region commented out
        //private void AddContent(TreeViewItem parent, IVsSimpleObjectList2 objects, string name, string path)
        //{
        //    if (objects == null)
        //        return;

        //    var root = new TreeViewItem { Header = name };

        //    if (path != null)
        //        root.Items.Add("Path = " + path);

        //    uint count;
        //    ErrorHandler.Succeeded(objects.GetItemCount(out count));
        //    root.Items.Add("Items count " + count);

        //    uint libFlags;
        //    ErrorHandler.Succeeded(objects.GetCapabilities2(out libFlags));

        //    var flags = "";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWDELETE) != 0)
        //        flags += "|LLC_ALLOWDELETE";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWDRAGDROP) != 0)
        //        flags += "|LLC_ALLOWDRAGDROP";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWRENAME) != 0)
        //        flags += "|LLC_ALLOWRENAME";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWSCCOPS) != 0)
        //        flags += "|LLC_ALLOWSCCOPS";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_HASBROWSEOBJ) != 0)
        //        flags += "|LLC_HASBROWSEOBJ";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_HASCOMMANDS) != 0)
        //        flags += "|LLC_HASCOMMANDS";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_HASDESCPANE) != 0)
        //        flags += "|LLC_HASDESCPANE";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES.LLC_NONE) != 0)
        //        flags += "|LLC_NONE";
        //    if ((libFlags & (uint)_LIB_LISTCAPABILITIES2.LLC_ALLOWELEMENTSEARCH) != 0)
        //        flags += "|LLC_ALLOWELEMENTSEARCH";
        //    if (flags != "")
        //    {
        //        flags = flags.Substring(1);
        //        root.Items.Add("Flags " + flags);
        //    }

        //    for (var i = (uint)0; i < count; i++)
        //    {
        //        AddNested(root, objects, i, (uint)_LIB_LISTTYPE.LLT_CLASSES);

        //        //IVsSimpleObjectList2 nestedObjects;
        //        //ErrorHandler.Succeeded(objects.GetList2(
        //        //    i,
        //        //    (uint)(_LIB_LISTTYPE.LLT_CLASSES),
        //        //    (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
        //        //    new[]
        //        //        {
        //        //            new VSOBSEARCHCRITERIA2
        //        //                {
        //        //                    eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
        //        //                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
        //        //                    szName = "*"
        //        //                }
        //        //        },
        //        //    out nestedObjects
        //        //                            ));

        //        //object propValue;
        //        //ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME, out propValue));
        //        //var fullName = (string) propValue;
        //        //ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_COMPONENTPATH, out propValue));
        //        //var componentPath = (string)propValue;
        //        //AddContent(root, nestedObjects, fullName, componentPath);

        //        //ErrorHandler.Succeeded(objects.GetList2(
        //        //    i,
        //        //    (uint)(_LIB_LISTTYPE.LLT_NAMESPACES),
        //        //    (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
        //        //    new[]
        //        //        {
        //        //            new VSOBSEARCHCRITERIA2
        //        //                {
        //        //                    eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
        //        //                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
        //        //                    szName = "*"
        //        //                }
        //        //        },
        //        //    out nestedObjects
        //        //                            ));

        //        //ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME, out propValue));
        //        //fullName = (string)propValue;
        //        //ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_COMPONENTPATH, out propValue));
        //        //componentPath = (string)propValue;
        //        ////AddContent(root, nestedObjects, fullName, componentPath);
        //    }
        //    parent.Items.Add(root);
        //}

        //void AddNested(TreeViewItem parent, IVsSimpleObjectList2 objects, uint index, uint listType)
        //{
        //    IVsSimpleObjectList2 nestedObjects;
        //    ErrorHandler.Succeeded(objects.GetList2(
        //        index,
        //        (uint)(_LIB_LISTTYPE.LLT_CLASSES),
        //        (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
        //        new[]
        //                {
        //                    new VSOBSEARCHCRITERIA2
        //                        {
        //                            eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
        //                            grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
        //                            szName = "*"
        //                        }
        //                },
        //        out nestedObjects
        //                                ));

        //    object propValue;
        //    ErrorHandler.Succeeded(objects.GetProperty(index, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME, out propValue));
        //    var fullName = (string)propValue;
        //    ErrorHandler.Succeeded(objects.GetProperty(index, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_COMPONENTPATH, out propValue));
        //    var componentPath = (string)propValue;
        //    AddContent(parent, nestedObjects, fullName, componentPath);
        //}
        #endregion

        private void AddLibList(TreeViewItem parent, string header, IVsLiteTreeList theList)
        {
            if (theList == null)
                return;
            uint count;
            ErrorHandler.Succeeded(theList.GetItemCount(out count));
            var root = new TreeViewItem { Header = header + " count = " + count };
            parent.Items.Add(root);
            //for (var i = 0; i< count; i++)
            //{
            //    string item;
            //    var rc = theList.GetText((uint) i, VSTREETEXTOPTIONS.TTO_DEFAULT, out item);
            //    root.Items.Add(item);
            //}
        }

        /// <summary>
        /// Searches the supplied library for something
        /// </summary>
        /// <param name="searchForType">Type of the item to search for</param>
        /// <param name="searchType">Type of the search to perform</param>
        /// <param name="lib">Library to search</param>
        /// <param name="param">Text to search for</param>
        /// <param name="caseSensitive">Is the search to be a case-sensitive or not</param>
        /// <returns></returns>
        IVsObjectList2 GetListFromLib(_LIB_LISTTYPE searchForType, VSOBSEARCHTYPE searchType, IVsLibrary2 lib, string param, bool caseSensitive)
        {
            IVsObjectList2 outList;

            lib.GetList2(
                (uint)searchForType,
                (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                new[]{
                                new VSOBSEARCHCRITERIA2
                                    {
                                        eSrchType = searchType,
                                        grfOptions = (uint)(caseSensitive? _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE : _VSOBSEARCHOPTIONS.VSOBSO_NONE),
                                        szName = param
                                    }
                            },
                    out outList);
            return outList;
        }

        /// <summary>
        /// Gets details on a symbol list (IVsObjectList2)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private string GetListDetails(IVsObjectList2 list)
        {
            IVsSimpleObjectList2 simpleList = list as IVsSimpleObjectList2;
            string res = string.Empty; // string for output

            // CATEGORIES
            uint temp; // uint for flags     
            list.GetItemCount(out temp);
            res += "\r\n>>>NESTED ITEMS\r\n";
            res += "Count: " + temp + "\r\n";
                        
            res += "\r\n>>>CAPABILITIES\r\n";
            simpleList.GetCapabilities2(out temp);
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWDELETE) != 0) res += "|LLC_ALLOWDELETE";
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWDRAGDROP) != 0) res += "|LLC_ALLOWDRAGDROP";
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWRENAME) != 0) res += "|LLC_ALLOWRENAME";
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_ALLOWSCCOPS) != 0) res += "|LLC_ALLOWSCCOPS";
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_HASBROWSEOBJ) != 0) res += "|LLC_HASBROWSEOBJ";
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_HASCOMMANDS) != 0) res += "|LLC_HASCOMMANDS";
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_HASDESCPANE) != 0) res += "|LLC_HASDESCPANE";
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_HASSOURCECONTEXT) != 0) res += "|LLC_HASSOURCECONTEXT";
            if ((temp & (uint)_LIB_LISTCAPABILITIES.LLC_NONE) != 0) res += "|LLC_NONE";

            res += "\r\n>>>CATEGORIES\r\n";

            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_ACTIVEPROJECT, out temp);
            res += string.Format("LC_ACTIVEPROJECT: _LIBCAT_ACTIVEPROJECT.{0}\r\n",
                Enum.GetName(typeof(_LIBCAT_ACTIVEPROJECT), temp));

            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_CLASSACCESS, out temp);
            res += string.Format("LC_CLASSACCESS: _LIBCAT_CLASSACCESS.{0}\r\n",
                Enum.GetName(typeof(_LIBCAT_CLASSACCESS), temp));

            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_CLASSTYPE, out temp);
            res += string.Format("LC_CLASSTYPE as _LIBCAT_CLASSTYPE: {0}\r\n",
                Enum.GetName(typeof(_LIBCAT_CLASSTYPE), temp));
            res += string.Format("LC_CLASSTYPE as _LIBCAT_CLASSTYPE2: {0}\r\n",
                Enum.GetName(typeof(_LIBCAT_CLASSTYPE2), temp));

            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_LISTTYPE, out temp);
            res += string.Format("LC_LISTTYPE as _LIB_LISTTYPE: {0}\r\n",
                Enum.GetName(typeof(_LIB_LISTTYPE), temp));
            res += string.Format("LC_LISTTYPE as _LIB_LISTTYPE2: {0}\r\n",
                Enum.GetName(typeof(_LIB_LISTTYPE2), temp));

            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_MEMBERACCESS, out temp);
            res += string.Format("LC_MEMBERACCESS: _LIBCAT_MEMBERACCESS. {0}\r\n",
                Enum.GetName(typeof(_LIBCAT_MEMBERACCESS), temp));

            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_MEMBERTYPE, out temp);
            res += string.Format("LC_MEMBERTYPE as _LIBCAT_MEMBERTYPE: {0}\r\n",
                Enum.GetName(typeof(_LIBCAT_MEMBERTYPE), temp));
            res += string.Format("LC_MEMBERTYPE as _LIBCAT_MEMBERTYPE2: {0}\r\n",
                Enum.GetName(typeof(_LIBCAT_MEMBERTYPE2), temp));


            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_MODIFIER, out temp);
            res += string.Format("LC_MODIFIER as _LIBCAT_MODIFIERTYPE: {0}\r\n",
                Enum.GetName(typeof(_LIBCAT_MODIFIERTYPE), temp));

            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_NODETYPE, out temp);
            res += string.Format("LC_NODETYPE as _LIBCAT_NODETYPE: {0}\r\n",
                Enum.GetName(typeof(_LIBCAT_NODETYPE), temp));

            simpleList.GetCategoryField2(0, (int)LIB_CATEGORY.LC_VISIBILITY, out temp);
            res += string.Format("LC_VISIBILITY as _LIBCAT_VISIBILITY: {0}\r\n",
                Enum.GetName(typeof(_LIBCAT_VISIBILITY), temp));

            res += ">>>Tree display data as VSTREEDISPLAYDATA[]\r\n";

            var dispData = new VSTREEDISPLAYDATA[1];
            simpleList.GetDisplayData(0, dispData);

            res += string.Format("ForceSelectLength: {0}\r\n", dispData[0].ForceSelectLength);
            res += string.Format("ForceSelectStart: {0}\r\n", dispData[0].ForceSelectStart);
            res += string.Format("hImageList: {0}\r\n", dispData[0].hImageList);
            res += string.Format("Image: {0}\r\n", dispData[0].Image);
            res += string.Format("Mask: {0}\r\n", dispData[0].Mask);
            res += string.Format("SelectedImage: {0}\r\n", dispData[0].SelectedImage);
            res += string.Format("State: {0}\r\n", dispData[0].State);
            res += string.Format("StateMask: {0}\r\n", dispData[0].StateMask);

            res += "\r\n>>>FLAGS\r\n";
            simpleList.GetFlags(out temp);

            if ((temp & (uint)_VSTREEFLAGS.TF_NOCOLORS) != 0)
                res += "|TF_NOCOLORS";
            if ((temp & (uint)_VSTREEFLAGS.TF_NOEFFECTS) != 0)
                res += "|TF_NOEFFECTS";
            if ((temp & (uint)_VSTREEFLAGS.TF_NOEVERYTHING) != 0)
                res += "|TF_NOEVERYTHING";
            if ((temp & (uint)_VSTREEFLAGS.TF_NOEXPANSION) != 0)
                res += "|TF_NOEXPANSION";
            if ((temp & (uint)_VSTREEFLAGS.TF_NOINSERTDELETE) != 0)
                res += "|TF_NOINSERTDELETE";
            if ((temp & (uint)_VSTREEFLAGS.TF_NOREALIGN) != 0)
                res += "|TF_NOREALIGN";
            if ((temp & (uint)_VSTREEFLAGS.TF_NORELOCATE) != 0)
                res += "|TF_NORELOCATE";
            if ((temp & (uint)_VSTREEFLAGS.TF_NOSTATECHANGE) != 0)
                res += "|TF_NOSTATECHANGE";
            if ((temp & (uint)_VSTREEFLAGS.TF_NOUPDATES) != 0)
                res += "|TF_NOUPDATES";
            if ((temp & (uint)_VSTREEFLAGS.TF_OWNERDRAWALL) != 0)
                res += "|TF_OWNERDRAWALL";
            if ((temp & (uint)_VSTREEFLAGS.TF_OWNERDRAWTEXT) != 0)
                res += "|TF_OWNERDRAWTEXT";


            res += "\r\n>>>NAV INFO\r\n";

            IVsNavInfo ni;
            simpleList.GetNavInfo(0, out ni);
            
            res += "\r\n>>>NAV INFO / EnumCanonicalNodes\r\n";
            if (ni == null)
                res += "Nav info is null";
            else
            {
                IVsEnumNavInfoNodes enumNodes;
                int nodeCount = ni.EnumCanonicalNodes(out enumNodes);
                res += string.Format("EnumCanonicalNodes count: {0}", nodeCount);
                if (nodeCount > 0)
                {
                    var nodeArr = new IVsNavInfoNode[nodeCount];
                    uint fetched = 0;
                    enumNodes.Next((uint)nodeCount, nodeArr, out fetched);
                    nodeArr.ToList().ForEach(n =>
                    {
                        string name = string.Empty;
                        uint type = 0;

                        n.get_Name(out name);
                        n.get_Type(out type);

                        res += string.Format("n.i. get_name: {0}\r\n", name);
                        res += string.Format("n.i. get_type as _LIB_LISTTYPE: {0}\r\n", Enum.GetName(typeof(_LIB_LISTTYPE), type));
                        res += string.Format("n.i. get_type as _LIB_LISTTYPE2: {0}\r\n", Enum.GetName(typeof(_LIB_LISTTYPE2), type));

                    });
                }
                res += "\r\n>>>NAV INFO / EnumPresentationNodes\r\n";
                /*
                 *   LLF_NONE No flags are specified.  
                     LLF_IGNORESUBSET Ignore subsets in the search. For class view requests only.  
                     LLF_TRUENESTING Search true nested items. For class view requests only.  
                     LLF_PROJECTONLY Search only the project. For class view requests only.  
                     LLF_USESEARCHFILTER Use a VSOBSEARCHCRITERIA parameter to limit information selection. For symbol search only.  
                     LLF_DONTUPDATELIST Don't update the symbol list. For find symbol only  
                     LLF_RESOURCEVIEW Search in resource view. For symbol search only.  
                 * */
                nodeCount = ni.EnumPresentationNodes((uint)_LIB_LISTFLAGS.LLF_NONE, out enumNodes);
                res += string.Format("EnumPresentationNodes count: {0}", nodeCount);
                if (nodeCount > 0)
                {
                    var nodeArr = new IVsNavInfoNode[nodeCount];
                    uint fetched = 0;
                    enumNodes.Next((uint)nodeCount, nodeArr, out fetched);
                    nodeArr.ToList().ForEach(n =>
                    {
                        string name = string.Empty;
                        uint type = 0;

                        n.get_Name(out name);
                        n.get_Type(out type);

                        res += string.Format("n.i. get_name: {0}\r\n", name);
                        res += string.Format("n.i. get_type as _LIB_LISTTYPE: {0}\r\n", Enum.GetName(typeof(_LIB_LISTTYPE), type));
                        res += string.Format("n.i. get_type as _LIB_LISTTYPE2: {0}\r\n", Enum.GetName(typeof(_LIB_LISTTYPE2), type));

                    });
                }

                uint symbolType = 0;
                ni.GetSymbolType(out symbolType);
                res += string.Format("\r\nSYMBOL TYPE as _LIB_LISTTYPE2: {0}\r\n", Enum.GetName(typeof(_LIB_LISTTYPE2), symbolType));
                res += string.Format("\r\nSYMBOL TYPE as _LIB_LISTTYPE: {0}\r\n", Enum.GetName(typeof(_LIB_LISTTYPE), symbolType));
            }
            // end of Nav Info
            res += "\r\n>>>PROPERTIES\r\n";
            //_VSOBJLISTELEMPROPID
            var props = Enum.GetValues(typeof(_VSOBJLISTELEMPROPID));
            object propValue;
            foreach (var p in props)
            {
                simpleList.GetProperty(0, (int)p, out propValue);
                res += string.Format("{0} : {1}\r\n",
                    p.ToString(),
                    propValue);
            }

            res += "\r\n>>>GetSourceContextWithOwnership\r\n";

            string fName;
            uint lineNum;
            simpleList.GetSourceContextWithOwnership(0, out fName, out lineNum);

            res += string.Format("fName: {0} \r\nLine Number: {1}\r\n",
                    fName,
                    lineNum);

            res += "\r\n>>>GetTextWithOwnership\r\n";
            props = Enum.GetValues(typeof(VSTREETEXTOPTIONS));
            string propText;
            foreach (var p in props)
            {
                simpleList.GetTextWithOwnership(0, (VSTREETEXTOPTIONS)p, out propText);
                res += string.Format("{0} : {1}\r\n",
                    p.ToString(),
                    propText);
            }

            res += "\r\n>>>GetTipTextWithOwnership\r\n";
            props = Enum.GetValues(typeof(VSTREETOOLTIPTYPE));
            foreach (var p in props)
            {
                simpleList.GetTipTextWithOwnership(0, (VSTREETOOLTIPTYPE)p, out propText);
                res += string.Format("{0} : {1}\r\n",
                    p.ToString(),
                    propText);
            }

            res += "\r\n>>>GetUserContext\r\n";
            object userContext;
            simpleList.GetUserContext(0, out userContext);
            res += userContext == null ?
                res += "Context is null\r\n" :
                string.Format("user context type : {0}\r\n",
                    userContext.GetType());

            return res;
        }
    }
}