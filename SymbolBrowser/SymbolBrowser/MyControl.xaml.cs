using System;
using System.Collections.Generic;
using System.Linq;
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
            if (library == null)
            {
                library = new Library();
                objectManager.RegisterSimpleLibrary(library, out libCookie);
            }

            // ToDo:
            // OBTAIN A LIST OF MODELS
            string[] typeNames = new string[] { "ClassLibrary1.Class1" };
            string[] methodNames = new string[] { "ClassLibrary1.Class1.GetBlaBlaBla" };
            // creating storage fo rfound results
            Dictionary<string, IVsSimpleObjectList2> foundLists = new Dictionary<string, IVsSimpleObjectList2>();
            foreach (string s in typeNames)
                foundLists.Add(s, null);

            Guid csLibGuid = new Guid("58f1bad0-2288-45b9-ac3a-d56398f7781d");

            IVsLibrary2 csLib;
            if (!ErrorHandler.Succeeded(objectManager.FindLibrary(ref csLibGuid, out csLib)))
            {
                MessageBox.Show("Could not load native C# library");
                return;
            }

            LogLibrarySupportedCategories(csLib);

            // SI: Uncomment one of the blocks to make logger log the data for the corresponding list

            //IVsObjectList2 simpleList;
            //csLib.GetList2((uint)_LIB_LISTTYPE.LLT_PACKAGE, 0, null, out simpleList);
            //ExploreListStructure(simpleList, "Simple list", 2);

            //IVsObjectList2 searchList;
            //csLib.GetList2(
            //    (uint)_LIB_LISTTYPE.LLT_CLASSES, // search for classes - LLT_CLASSES, methods - LLT_MEMBERS
            //    (uint)(_LIB_LISTFLAGS.LLF_USESEARCHFILTER | _LIB_LISTFLAGS.LLF_DONTUPDATELIST),
            //    new[]
            //        {
            //            new VSOBSEARCHCRITERIA2
            //                {
            //                    eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD,
            //                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_LOOKINREFS, // 2                                
            //                    szName = "ClassLibrary1.Class1"
            //                }
            //        },
            //        out searchList);
            //ExploreListStructure(searchList, "Search result list", 2);

            // Obtain a list of corresponding symbols from native C# library
            foreach (var s in typeNames)
            {
                AddSymbolToLibrary(_LIB_LISTTYPE.LLT_CLASSES, s, ref csLib);
            }//foreach

            // Obtain a list of corresponding symbols from native C# library
            foreach (var s in methodNames)
            {
                AddSymbolToLibrary(_LIB_LISTTYPE.LLT_MEMBERS, s, ref csLib);
            }//foreach

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
                if (project == null) continue;
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

                AddLibrary(lib, extras);
            }
        }

        private void AddSymbolToLibrary(_LIB_LISTTYPE symbolType, string symbolText, ref IVsLibrary2 csLib)
        {
            IVsObjectList2 list;
            var success = ErrorHandler.Succeeded(csLib.GetList2(
                (uint)symbolType,
                (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                new[]
                    {
                        new VSOBSEARCHCRITERIA2
                            {
                                eSrchType = VSOBSEARCHTYPE.SO_SUBSTRING,
                                grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_NONE,
                                szName = symbolText
                            }
                    }, out list));
            if (success && list != null)
            {
                uint count = 0;
                list.GetItemCount(out count);
                if (count == 0)
                    MessageBox.Show(String.Format("Error obtaining native symbol for class '{0}'", symbolText));
                else
                    // Merge our symbols with the ones obtained from native lib
                    library.AddExternalReference(symbolText, list);
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

            for (var i = (uint)0; i < count; i++)
            {
                object propValue;
                string text;
                Type objType;
                //objects.GetText(i, VSTREETEXTOPTIONS.TTO_BASETEXT, out text);
                objType = objects.GetType();

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

                parent.Items.Add(item);
            }

        }

        #region ...
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

        #region List structure exploration

        private void LogLibrarySupportedCategories(IVsLibrary2 csLib)
        {
            uint outVal = 0;
            /*
                LC_MEMBERTYPE = 1,
                LC_MEMBERACCESS = 2,
                LC_CLASSTYPE = 3,
                LC_CLASSACCESS = 4,
                LC_ACTIVEPROJECT = 5,
                LC_LISTTYPE = 6,
                LC_VISIBILITY = 7,
                LC_MODIFIER = 8,
                LC_NODETYPE = 9,
             */
            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_ACTIVEPROJECT, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_ACTIVEPROJECT", Enum.GetName(typeof(_LIBCAT_ACTIVEPROJECT), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_CLASSACCESS, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_CLASSACCESS", Enum.GetName(typeof(_LIBCAT_CLASSACCESS), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_CLASSTYPE, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_CLASSTYPE", Enum.GetName(typeof(_LIBCAT_CLASSTYPE), outVal), outVal));
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_CLASSTYPE as _LIBCAT_CLASSTYPE2", Enum.GetName(typeof(_LIBCAT_CLASSTYPE2), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_LISTTYPE, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_LISTTYPE", Enum.GetName(typeof(_LIB_LISTTYPE), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_MEMBERACCESS, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_MEMBERACCESS", Enum.GetName(typeof(_LIBCAT_MEMBERACCESS), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_MEMBERTYPE, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_MEMBERTYPE", Enum.GetName(typeof(_LIBCAT_MEMBERTYPE), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_MODIFIER, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_MODIFIER", Enum.GetName(typeof(_LIBCAT_MODIFIERTYPE), outVal), outVal));

            //            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_NODETYPE, out outVal);
            //Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_NODETYPE", Enum.GetName(typeof()), outVal));

            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_VISIBILITY, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_VISIBILITY", Enum.GetName(typeof(_LIBCAT_VISIBILITY), outVal), outVal));



            csLib.GetSupportedCategoryFields2((int)_LIB_CATEGORY2.LC_HIERARCHYTYPE, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_HIERARCHYTYPE", Enum.GetName(typeof(_LIBCAT_HIERARCHYTYPE), outVal), outVal));
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_HIERARCHYTYPE as _LIBCAT_HIERARCHYTYPE2",
                Enum.GetName(typeof(_LIBCAT_HIERARCHYTYPE2), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)_LIB_CATEGORY2.LC_Last2, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_Last2", outVal, outVal));

            csLib.GetSupportedCategoryFields2((int)_LIB_CATEGORY2.LC_MEMBERINHERITANCE, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_MEMBERINHERITANCE", Enum.GetName(typeof(_LIBCAT_MEMBERINHERITANCE), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)_LIB_CATEGORY2.LC_NIL, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_NIL", outVal, outVal));

            csLib.GetSupportedCategoryFields2((int)_LIB_CATEGORY2.LC_PHYSICALCONTAINERTYPE, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_PHYSICALCONTAINERTYPE", Enum.GetName(typeof(_LIBCAT_PHYSICALCONTAINERTYPE), outVal), outVal));

            csLib.GetSupportedCategoryFields2((int)_LIB_CATEGORY2.LC_SEARCHMATCHTYPE, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_SEARCHMATCHTYPE", Enum.GetName(typeof(_LIBCAT_SEARCHMATCHTYPE), outVal), outVal));
        }

        private void LogListSupportedCategories(uint childIndex, IVsObjectList2 csLib)
        {
            uint outVal = 0;
            string tempStr = string.Empty;
            /*
             * LIB_CATEGORY : 
             * 
                LC_MEMBERTYPE = 1,
                LC_MEMBERACCESS = 2,
                LC_CLASSTYPE = 3,
                LC_CLASSACCESS = 4,
                LC_ACTIVEPROJECT = 5,
                LC_LISTTYPE = 6,
                LC_VISIBILITY = 7,
                LC_MODIFIER = 8,
                LC_NODETYPE = 9,
             */
            Logger.Log("** GetCategoryField2");

            csLib.GetCategoryField2(childIndex, (int)LIB_CATEGORY.LC_ACTIVEPROJECT, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_ACTIVEPROJECT e in Enum.GetValues(typeof(_LIBCAT_ACTIVEPROJECT)))
                if (((_LIBCAT_ACTIVEPROJECT)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_ACTIVEPROJECT), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_ACTIVEPROJECT: (_LIBCAT_ACTIVEPROJECT)({0}){1}", outVal, tempStr));

            csLib.GetCategoryField2(childIndex, (int)LIB_CATEGORY.LC_LISTTYPE, out outVal);
            tempStr = string.Empty;
            foreach (_LIB_LISTTYPE e in Enum.GetValues(typeof(_LIB_LISTTYPE)))
                if (((_LIB_LISTTYPE)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIB_LISTTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_LISTTYPE: (_LIB_LISTTYPE)({0}){1}", outVal, tempStr));

            // crashed VS
            //csLib.GetCategoryField2(childIndex, (int)LIB_CATEGORY.LC_CLASSACCESS, out outVal);
            //Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_CLASSACCESS", Enum.GetName(typeof(_LIBCAT_CLASSACCESS), outVal), outVal));

            tempStr = string.Empty;
            csLib.GetCategoryField2(childIndex, (int)LIB_CATEGORY.LC_CLASSTYPE, out outVal);
            foreach (_LIBCAT_CLASSTYPE2 e in Enum.GetValues(typeof(_LIBCAT_CLASSTYPE2)))
                if (((_LIBCAT_CLASSTYPE2)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_CLASSTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_CLASSTYPE: (_LIBCAT_CLASSTYPE2)({0}){1}", outVal, tempStr));

            tempStr = string.Empty;
            foreach (_LIBCAT_CLASSTYPE e in Enum.GetValues(typeof(_LIBCAT_CLASSTYPE)))
                if (((_LIBCAT_CLASSTYPE)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_CLASSTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_CLASSTYPE: (_LIBCAT_CLASSTYPE)({0}){1}", outVal, tempStr));

            csLib.GetCategoryField2(childIndex, (int)LIB_CATEGORY.LC_MEMBERACCESS, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_MEMBERACCESS e in Enum.GetValues(typeof(_LIBCAT_MEMBERACCESS)))
                if (((_LIBCAT_MEMBERACCESS)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_CLASSTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_MEMBERACCESS: (_LIBCAT_MEMBERACCESS)({0}){1}", outVal, tempStr));

            csLib.GetCategoryField2(childIndex, (int)LIB_CATEGORY.LC_MEMBERTYPE, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_MEMBERTYPE e in Enum.GetValues(typeof(_LIBCAT_MEMBERTYPE)))
                if (((_LIBCAT_MEMBERTYPE)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_CLASSTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_MEMBERTYPE: (_LIBCAT_MEMBERTYPE)({0}){1}", outVal, tempStr));

            csLib.GetCategoryField2(childIndex, (int)LIB_CATEGORY.LC_MODIFIER, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_MODIFIERTYPE e in Enum.GetValues(typeof(_LIBCAT_MODIFIERTYPE)))
                if (((_LIBCAT_MODIFIERTYPE)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_CLASSTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_MODIFIER: (_LIBCAT_MODIFIERTYPE)({0}){1}", outVal, tempStr));

            //            csLib.GetSupportedCategoryFields2((int)LIB_CATEGORY.LC_NODETYPE, out outVal);
            //Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_NODETYPE", Enum.GetName(typeof()), outVal));

            csLib.GetCategoryField2(childIndex, (int)LIB_CATEGORY.LC_VISIBILITY, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_VISIBILITY e in Enum.GetValues(typeof(_LIBCAT_VISIBILITY)))
                if (((_LIBCAT_VISIBILITY)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_CLASSTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_VISIBILITY: (_LIBCAT_VISIBILITY)({0}){1}", outVal, tempStr));



            csLib.GetCategoryField2(childIndex, (int)_LIB_CATEGORY2.LC_HIERARCHYTYPE, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_HIERARCHYTYPE2 e in Enum.GetValues(typeof(_LIBCAT_HIERARCHYTYPE2)))
                if (((_LIBCAT_HIERARCHYTYPE2)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_HIERARCHYTYPE2), e);
            Logger.Log(string.Format("LIB_CATEGORY._LIBCAT_HIERARCHYTYPE2: (_LIBCAT_HIERARCHYTYPE2)({0}){1}", outVal, tempStr));

            tempStr = string.Empty;
            foreach (_LIBCAT_HIERARCHYTYPE e in Enum.GetValues(typeof(_LIBCAT_HIERARCHYTYPE)))
                if (((_LIBCAT_HIERARCHYTYPE)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_CLASSTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY._LIBCAT_HIERARCHYTYPE: (_LIBCAT_HIERARCHYTYPE)({0}){1}", outVal, tempStr));


            csLib.GetCategoryField2(childIndex, (int)_LIB_CATEGORY2.LC_Last2, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_Last2", outVal, outVal));

            csLib.GetCategoryField2(childIndex, (int)_LIB_CATEGORY2.LC_MEMBERINHERITANCE, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_MEMBERINHERITANCE e in Enum.GetValues(typeof(_LIBCAT_MEMBERINHERITANCE)))
                if (((_LIBCAT_MEMBERINHERITANCE)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_MEMBERINHERITANCE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_MEMBERINHERITANCE: (_LIBCAT_MEMBERINHERITANCE)({0}){1}", outVal, tempStr));


            csLib.GetCategoryField2(childIndex, (int)_LIB_CATEGORY2.LC_NIL, out outVal);
            Logger.Log(string.Format("C# LIB_CATEGORY {0} - {1}({2})", "LC_NIL", outVal, outVal));

            csLib.GetCategoryField2(childIndex, (int)_LIB_CATEGORY2.LC_PHYSICALCONTAINERTYPE, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_PHYSICALCONTAINERTYPE e in Enum.GetValues(typeof(_LIBCAT_PHYSICALCONTAINERTYPE)))
                if (((_LIBCAT_PHYSICALCONTAINERTYPE)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_PHYSICALCONTAINERTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_PHYSICALCONTAINERTYPE: (_LIBCAT_PHYSICALCONTAINERTYPE)({0}){1}", outVal, tempStr));


            csLib.GetCategoryField2(childIndex, (int)_LIB_CATEGORY2.LC_SEARCHMATCHTYPE, out outVal);
            tempStr = string.Empty;
            foreach (_LIBCAT_SEARCHMATCHTYPE e in Enum.GetValues(typeof(_LIBCAT_SEARCHMATCHTYPE)))
                if (((_LIBCAT_SEARCHMATCHTYPE)outVal & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIBCAT_SEARCHMATCHTYPE), e);
            Logger.Log(string.Format("LIB_CATEGORY.LC_SEARCHMATCHTYPE: (_LIBCAT_SEARCHMATCHTYPE)({0}){1}", outVal, tempStr));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="title"></param>
        /// <param name="itemsToDo">number of items to explore, set -1 to explore all (this would take a LOT of time and space),</param>
        private void ExploreListStructure(IVsObjectList2 list, string title, int itemsToDo)
        {
            Logger.Log("\r\n-------------------------------\r\nDetails for list '" + title + "'\r\n------------------------------- ");

            string tempStr = string.Empty;
            uint
                temp = 0,
                itemCount = 0;


            list.GetItemCount(out itemCount);
            Logger.Log(string.Format("* Child item count: {0}", itemCount));

            Logger.Log(string.Format("* GetCapabilities2", itemCount));
            list.GetCapabilities2(out temp);


            tempStr = string.Empty;
            foreach (_LIB_LISTCAPABILITIES2 e in Enum.GetValues(typeof(_LIB_LISTCAPABILITIES2)))
                if (((_LIB_LISTCAPABILITIES2)temp & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIB_LISTCAPABILITIES2), e);
            Logger.Log(string.Format("_LIB_LISTCAPABILITIES2: ({0}){1}", temp, tempStr));

            tempStr = string.Empty;
            foreach (_LIB_LISTCAPABILITIES e in Enum.GetValues(typeof(_LIB_LISTCAPABILITIES)))
                if (((_LIB_LISTCAPABILITIES)temp & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_LIB_LISTCAPABILITIES), e);
            Logger.Log(string.Format("_LIB_LISTCAPABILITIES: ({0}){1}", temp, tempStr));

            Logger.Log(string.Format("* GetFlags", itemCount));
            temp = 0;
            list.GetFlags(out temp);

            tempStr = string.Empty;
            foreach (_VSTREEFLAGS2 e in Enum.GetValues(typeof(_VSTREEFLAGS2)))
                if (((_VSTREEFLAGS2)temp & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_VSTREEFLAGS2), e);
            Logger.Log(string.Format("_VSTREEFLAGS2: ({0}){1}", temp, tempStr));

            tempStr = string.Empty;
            foreach (_VSTREEFLAGS e in Enum.GetValues(typeof(_VSTREEFLAGS)))
                if (((_VSTREEFLAGS)temp & e) == e)
                    tempStr += "|" + Enum.GetName(typeof(_VSTREEFLAGS), e);
            Logger.Log(string.Format("_VSTREEFLAGS: ({0}){1}", temp, tempStr));

            // children items
            if (itemCount > 0)
                
                if (itemsToDo > -1 && itemsToDo < itemCount)
                    itemCount = (uint)itemsToDo;

                for (uint i = 0; i < itemCount; i++)
                {
                    Logger.Log(string.Format("* Details of child item {0}", i));
                    object prop;
                    string listName = string.Empty;


                    list.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME, out prop);
                    Logger.Log(string.Format("VSOBJLISTELEMPROPID_FULLNAME: {0}", prop));
                    listName = prop.ToString();

                    list.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_HELPKEYWORD, out prop);
                    Logger.Log(string.Format("VSOBJLISTELEMPROPID_HELPKEYWORD: {0}", prop));

                    list.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_LEAFNAME, out prop);
                    Logger.Log(string.Format("VSOBJLISTELEMPROPID_LEAFNAME: {0}", prop));

                    list.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_SUPPORTSCALLSFROM, out prop);
                    Logger.Log(string.Format("VSOBJLISTELEMPROPID_SUPPORTSCALLSFROM: {0}", prop));

                    list.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_SUPPORTSCALLSTO, out prop);
                    Logger.Log(string.Format("VSOBJLISTELEMPROPID_SUPPORTSCALLSTO: {0}", prop));

                    list.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_COMPONENTPATH, out prop);
                    Logger.Log(string.Format("VSOBJLISTELEMPROPID_COMPONENTPATH: {0}", prop));

                    //SI: Crashes VS
                    //list.GetText(i, VSTREETEXTOPTIONS.TTO_BASETEXT, out tempStr);
                    //Logger.Log(string.Format("Base text: {0}", tempStr));

                    LogListSupportedCategories(i, list);

                    IVsObjectList2 childList = null;
                    uint childItemCount = 0;

                    //list.GetList2(
                    //    i,
                    //    (uint)_LIB_LISTTYPE.LLT_MEMBERS,
                    //    16,
                    //    new[]
                    //        {
                    //            new VSOBSEARCHCRITERIA2
                    //                {
                    //                    eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD,
                    //                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_LOOKINREFS, // 2                                
                    //                    szName = "*"
                    //                }
                    //        },
                    //        out chldList);
                    foreach (_LIB_LISTTYPE en in Enum.GetValues(typeof(_LIB_LISTTYPE)))
                    {
                        list.GetList2(i, (uint)en, 0, null, out childList);
                        if (childList == null)
                            continue;
                        
                        ExploreListStructure(
                            childList,
                            string.Format("Child {0} of list {1}", i, title),
                            -1);
                    }

                    
                }

            Logger.Log(string.Format("End of list {0}\r\n-------------------------------\r\n", title));

        }
        #endregion
    }
}