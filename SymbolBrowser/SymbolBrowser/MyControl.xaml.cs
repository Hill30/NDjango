﻿using System;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var objectManager = SymbolBrowserPackage.GetGlobalService(typeof(SVsObjectManager)) as IVsObjectManager2;
            IVsEnumLibraries2 libs;
            ErrorHandler.Succeeded(objectManager.EnumLibraries(out libs));
            var libArray = new IVsLibrary2[20];
            uint fetched;
            libs.Next((uint)libArray.Length, libArray, out fetched);
            treeView1.Items.Clear();
            foreach (var lib in libArray)
            {
                AddLibrary(lib);
            }

        }

        private void AddLibrary(IVsLibrary2 lib)
        {
            if (lib == null)
                return;

            var simpleLib = lib as IVsSimpleLibrary2;

            Guid libGuid;
            ErrorHandler.Succeeded(simpleLib.GetGuid(out libGuid));
            var libRoot = new TreeViewItem { Header = "guid=(" + libGuid + ")" };
            var expander = new TreeViewItem();
            libRoot.Items.Add(expander);
            libRoot.Expanded += (sender, args) => AddLibraryContent(libRoot, expander, lib);
            treeView1.Items.Add(libRoot);
        }

        private void AddLibraryContent(TreeViewItem libRoot, TreeViewItem expander, IVsLibrary2 lib)
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


            libRoot.Items.Add("Flags=" + flags);

            //IVsLiteTreeList globalLibs;
            //ErrorHandler.Succeeded(lib.GetLibList(LIB_PERSISTTYPE.LPT_GLOBAL, out globalLibs));
            //AddLibList(libRoot, "Global", globalLibs);

            //IVsLiteTreeList projectLibs;
            //ErrorHandler.Succeeded(lib.GetLibList(LIB_PERSISTTYPE.LPT_PROJECT, out projectLibs));
            //AddLibList(libRoot, "Project", globalLibs);

            AddNested(lib, libRoot, _LIB_LISTTYPE.LLT_NAMESPACES);

            AddNested(lib, libRoot, _LIB_LISTTYPE.LLT_CLASSES);

            AddNested(lib, libRoot, _LIB_LISTTYPE.LLT_MEMBERS);

            AddNested(lib, libRoot, _LIB_LISTTYPE.LLT_REFERENCES);

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

            var root = new TreeViewItem {Header = listType};
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

            for (var i = (uint)0; i < count; i++)
            {
                object propValue;
                ErrorHandler.Succeeded(objects.GetProperty(i, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_LEAFNAME, out propValue));
                var item = new TreeViewItem {Header = (string)propValue};
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

        private void AddLibList(TreeViewItem parent, string header, IVsLiteTreeList theList)
        {
            if (theList == null)
                return;
            uint count;
            ErrorHandler.Succeeded(theList.GetItemCount(out count));
            var root = new TreeViewItem {Header = header + " count = " + count};
            parent.Items.Add(root);
            //for (var i = 0; i< count; i++)
            //{
            //    string item;
            //    var rc = theList.GetText((uint) i, VSTREETEXTOPTIONS.TTO_DEFAULT, out item);
            //    root.Items.Add(item);
            //}
        }
    }
}