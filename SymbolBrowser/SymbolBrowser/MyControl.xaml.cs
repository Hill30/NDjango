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
                AddLibraryContent(lib);
            }

        }

        private void AddLibraryContent(IVsLibrary2 lib)
        {
            if (lib == null)
                return;

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

            var libRoot = new TreeViewItem { Header = "giud=(" + libGuid + ") + flags=" + flags };
            treeView1.Items.Add(libRoot);

            IVsLiteTreeList globalLibs;
            ErrorHandler.Succeeded(lib.GetLibList(LIB_PERSISTTYPE.LPT_GLOBAL, out globalLibs));
            AddLibList(libRoot, "Global", globalLibs);

            IVsLiteTreeList projectLibs;
            ErrorHandler.Succeeded(lib.GetLibList(LIB_PERSISTTYPE.LPT_PROJECT, out projectLibs));
            AddLibList(libRoot, "Project", globalLibs);

            IVsObjectList2 objects = null;
            try
            {
                ErrorHandler.Succeeded(lib.GetList2(
                    (uint) (_LIB_LISTTYPE.LLT_NAMESPACES),
                    (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                    new[]
                        {
                            new VSOBSEARCHCRITERIA2
                                {
                                    eSrchType = VSOBSEARCHTYPE.SO_PRESTRING,
                                    grfOptions = (uint) _VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
                                    szName = "*"
                                }
                            ,

                        },
                    out objects
                                            ));
            }
            catch (Exception e)
            {
                var s = e.Message;
            }
            AddContent(libRoot, objects as IVsSimpleObjectList2);

        }

        private void AddContent(TreeViewItem parent, IVsSimpleObjectList2 objects)
        {
            if (objects == null)
                return;

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
            flags = flags.Substring(1);

            uint count;
            ErrorHandler.Succeeded(objects.GetItemCount(out count));
            var root = new TreeViewItem() {Header = "Items count = " + count + " flags = " + flags};
            for (var i = 0; i < count; i++)
            {
                string text;
                objects.GetTextWithOwnership((uint)i, VSTREETEXTOPTIONS.TTO_DEFAULT, out text);
                root.Items.Add(text);
            }
            parent.Items.Add(root);
        }

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