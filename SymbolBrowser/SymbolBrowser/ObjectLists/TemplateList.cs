using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    /// <summary>
    /// Node of PhisicalContainer type
    /// </summary>
    public class TemplateList:ResultList
    {
        // file list
        public TemplateList(string text, string fName)
            : base(text, fName, 0, LibraryNodeType.PhysicalContainer)
        {
        }

        protected override bool IsExpandable
        {
            get { return true; }
        }

        protected override void GotoSource(VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            base.GotoSource(gotoType);
        }

        public override bool CanGoToSource
        {
            get
            {
                return true; // templates can go to source
            }
        }
    }
}
