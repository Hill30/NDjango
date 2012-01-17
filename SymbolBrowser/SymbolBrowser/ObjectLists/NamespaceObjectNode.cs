using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    /// <summary>
    /// Node for Object Browser
    /// </summary>
    class NamespaceObjectNode : SymbolNode
    {
        public NamespaceObjectNode(string text, string preffix, string fName)
            : base(text, fName, preffix, 0, 0, LibraryNodeType.Classes)
        {
            #region Log part for namespace node
            /*
            LIB_CATEGORY.LC_ACTIVEPROJECT: (_LIBCAT_ACTIVEPROJECT)(0)
            LIB_CATEGORY.LC_LISTTYPE: (_LIB_LISTTYPE)(4)|LLT_CLASSES
            LIB_CATEGORY.LC_CLASSTYPE: (_LIBCAT_CLASSTYPE2)(1)
            LIB_CATEGORY.LC_CLASSTYPE: (_LIBCAT_CLASSTYPE)(1)|LCCT_NSPC
            LIB_CATEGORY.LC_MEMBERACCESS: (_LIBCAT_MEMBERACCESS)(1)|LCMA_PUBLIC
            LIB_CATEGORY.LC_MEMBERTYPE: (_LIBCAT_MEMBERTYPE)(1)|LCMT_METHOD
            LIB_CATEGORY.LC_MODIFIER: (_LIBCAT_MODIFIERTYPE)(1)|LCMDT_VIRTUAL
            LIB_CATEGORY.LC_VISIBILITY: (_LIBCAT_VISIBILITY)(1)|LCV_VISIBLE
            LIB_CATEGORY._LIBCAT_HIERARCHYTYPE2: (_LIBCAT_HIERARCHYTYPE2)(1)
            LIB_CATEGORY._LIBCAT_HIERARCHYTYPE: (_LIBCAT_HIERARCHYTYPE)(1)|LCHT_UNKNOWN
            LIB_CATEGORY LC_Last2 - 1(1)
            LIB_CATEGORY.LC_MEMBERINHERITANCE: (_LIBCAT_MEMBERINHERITANCE)(1)|LCMI_IMMEDIATE
            LIB_CATEGORY LC_NIL - 1(1)
            LIB_CATEGORY.LC_PHYSICALCONTAINERTYPE: (_LIBCAT_PHYSICALCONTAINERTYPE)(0)
            LIB_CATEGORY.LC_SEARCHMATCHTYPE: (_LIBCAT_SEARCHMATCHTYPE)(0)
            Error reading list type 'LLT_USESCLASSES'
             */
            
            #endregion
            classType = _LIBCAT_CLASSTYPE.LCCT_NSPC; // 1
            memberAccess = _LIBCAT_MEMBERACCESS.LCMA_PUBLIC; // 1
            memberType = _LIBCAT_MEMBERTYPE.LCMT_METHOD; // 1
            modifierType = _LIBCAT_MODIFIERTYPE.LCMDT_VIRTUAL; // 1
            visibility = _LIBCAT_VISIBILITY.LCV_VISIBLE; // 1
            hierarchyType = _LIBCAT_HIERARCHYTYPE.LCHT_UNKNOWN; // 1
            memberInheritance = _LIBCAT_MEMBERINHERITANCE.LCMI_IMMEDIATE; // 1
            phisContainerType = 0;
            srchMatchType = 0;
        }

        protected override bool IsExpandable
        {
            get { return true; }
        }
        public override bool CanGoToSource
        {
            get
            {
                return false; // Namespaces can not go to source I think )
            }
        }
        protected override bool CanDelete { get { return false; } }

        protected override VSTREEDISPLAYDATA DisplayData
        {
            get
            {
                return new VSTREEDISPLAYDATA
                {
                    ForceSelectLength = 5,
                    ForceSelectStart = 0,
                    hImageList = IntPtr.Zero,
                    Image = 90,
                    SelectedImage = 90,
                    Mask = (uint)_VSTREEDISPLAYMASK.TDM_IMAGE, //?!
                    State = (uint)0,
                    StateMask = (uint)0
                };
            }
        }

        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            // do nothing - not supported for namespaces
        }

        /// <summary>
        /// Gets the supported category
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="pfCatField"></param>
        public override void GetCategory(int Category, out uint pfCatField)
        {
            pfCatField = 0;
        }
    }
}
