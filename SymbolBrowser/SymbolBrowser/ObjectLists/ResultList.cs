using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Runtime.InteropServices;

namespace Microsoft.SymbolBrowser.ObjectLists
{
    /// <summary>
    /// Root of our object list model
    /// </summary>
    public class ResultList : IVsSimpleObjectList2, IVsNavInfoNode
    {
        /// <summary>
        /// Enumeration of the possible types of node. The type of a node can be the combination
        /// of one of more of these values.
        /// This is actually a copy of the _LIB_LISTTYPE enumeration with the difference that the
        /// Flags attribute is set so that it is possible to specify more than one value.
        /// </summary>
        [Flags]
        public enum LibraryNodeType
        {
            None = 0,
            Hierarchy = _LIB_LISTTYPE.LLT_HIERARCHY,
            Namespaces = _LIB_LISTTYPE.LLT_NAMESPACES,
            Classes = _LIB_LISTTYPE.LLT_CLASSES,
            Members = _LIB_LISTTYPE.LLT_MEMBERS,
            Package = _LIB_LISTTYPE.LLT_PACKAGE,
            PhysicalContainer = _LIB_LISTTYPE.LLT_PHYSICALCONTAINERS,
            Containment = _LIB_LISTTYPE.LLT_CONTAINMENT,
            ContainedBy = _LIB_LISTTYPE.LLT_CONTAINEDBY,
            UsesClasses = _LIB_LISTTYPE.LLT_USESCLASSES,
            UsedByClasses = _LIB_LISTTYPE.LLT_USEDBYCLASSES,
            NestedClasses = _LIB_LISTTYPE.LLT_NESTEDCLASSES,
            InheritedInterface = _LIB_LISTTYPE.LLT_INHERITEDINTERFACES,
            InterfaceUsedByClasses = _LIB_LISTTYPE.LLT_INTERFACEUSEDBYCLASSES,
            Definitions = _LIB_LISTTYPE.LLT_DEFINITIONS,
            References = _LIB_LISTTYPE.LLT_REFERENCES,
            DeferExpansion = _LIB_LISTTYPE.LLT_DEFEREXPANSION,
        }

        public const uint NullIndex = (uint)0xFFFFFFFF;

        protected readonly int
            lineNumber = 0,
            columnNumber = 0;

        private readonly string
            symbolText,
            symbolPrefix = string.Empty,
            fName;

        private uint
            updateCount = 0;
        private Dictionary<LibraryNodeType, ResultList> filteredView = new Dictionary<LibraryNodeType, ResultList>();
        private readonly List<ResultList> children = new List<ResultList>();
        private readonly LibraryNodeType nodeType;

        public ResultList(string text, string prefix, string fName, int lineNumber, int columnNumber, LibraryNodeType type)
        {
            this.symbolText = text;
            this.symbolPrefix = prefix;
            this.fName = fName;
            this.lineNumber = lineNumber;
            this.columnNumber = columnNumber;
            this.nodeType = type;
        }

        internal ResultList(ResultList node)
        {
            this.symbolText = node.symbolText;
            this.symbolPrefix = node.symbolPrefix;
            this.fName = node.fName;
            this.lineNumber = node.lineNumber;
            this.columnNumber = node.columnNumber;
            this.nodeType = node.nodeType;
            node.children.ForEach(c => AddChild(c));
        }

        protected virtual VSTREEDISPLAYDATA DisplayData
        {
            get
            {
                // SI: checked out icon indexes, there's 236 of them
                // In case of you need it check \\spb-dev\Upload\SIvanov\VSGlyphs.bmp 

                return new VSTREEDISPLAYDATA
                {
                    ForceSelectLength = 5,
                    ForceSelectStart = 0,
                    hImageList = IntPtr.Zero,
                    Image = (ushort)0,
                    SelectedImage = (ushort)0,
                    Mask = (uint)_VSTREEDISPLAYMASK.TDM_IMAGE, //?!
                    State = (uint)0, //display as normal text
                    StateMask = (uint)0
                };
            }
        }

        public LibraryNodeType NodeType
        {
            get { return nodeType; }
        }

        /// <summary>
        /// What native C# IVsObjectLibrary2 object should be referenced
        /// </summary>
        public IVsObjectList2 ListToReference { get; set; }

        public void AddChild(ResultList child)
        {
            children.Add(child);
            updateCount++;
        }
        public void RemoveChild(ResultList child)
        {
            children.Remove(child);
            updateCount++;
        }

        /// <summary>
        /// preffix + name. Used in IVsNavInfo.get_Name
        /// </summary>
        public virtual string UniqueName
        {
            get { return symbolPrefix + symbolText; }
        }

        /// <summary>
        /// True if node has file name defined
        /// </summary>
        public virtual bool CanGoToSource
        {
            get { return (fName != string.Empty); } // Root can not go to source
        }
        public string FileName { get { return fName; } }

        protected virtual bool IsExpandable
        {
            get { return true; }
        }
        protected virtual bool CanDelete { get { return false; } }
        protected virtual void GotoSource(VSOBJGOTOSRCTYPE gotoType)
        {
            // Do nothing.
        }

        protected virtual void FillDescription(_VSOBJDESCOPTIONS flagsArg, IVsObjectBrowserDescription3 description)
        {
            description.ClearDescriptionText();
            description.AddDescriptionText3(symbolText, VSOBDESCRIPTIONSECTION.OBDS_NAME, null);
        }

        internal IVsSimpleObjectList2 FilterView(LibraryNodeType filterType, VSOBSEARCHCRITERIA2[] pobSrch)
        {
            ResultList
                filtered = null,
                temp = null;

            if (!filteredView.TryGetValue(filterType, out temp))
            {   // Filling filtered view
                filtered = this.Clone();
                for (int i = 0; i < filtered.children.Count; )
                    if (0 == (filtered.children[i].nodeType & filterType))
                        filtered.children.RemoveAt(i);
                    else
                        i += 1;
                filteredView.Add(filterType, filtered.Clone());
            }
            else
                filtered = temp.Clone(); // making sure we filter a new node

            // Checking if we need to perform search
            if (pobSrch != null)
            {
                Regex reg;
                switch (pobSrch[0].eSrchType)
                {
                    case VSOBSEARCHTYPE.SO_ENTIREWORD:
                        reg = new Regex(string.Format("^{0}$", pobSrch[0].szName.ToLower()));
                        break;
                    case VSOBSEARCHTYPE.SO_PRESTRING:
                        reg = new Regex(string.Format("^{0}.*", pobSrch[0].szName.ToLower()));
                        break;
                    case VSOBSEARCHTYPE.SO_SUBSTRING:
                        reg = new Regex(string.Format(".*{0}.*", pobSrch[0].szName.ToLower()));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                for (int i = 0; i < filtered.children.Count; )
                    if (!reg.Match(filtered.children[i].UniqueName.ToLower()).Success)
                        filtered.children.RemoveAt(i);
                    else
                        i += 1;
            }

            return filtered as IVsSimpleObjectList2;
        }

        protected virtual ResultList Clone()
        {
            return new ResultList(this);
        }

        protected virtual void OpenSourceFile()
        {
            System.Windows.Forms.MessageBox.Show("This implementation should not have been used - list to reference is not defined for this symbol and no valid path is probably defined.\r\n It is planned to be used for opening Django templates.");
            //throw new NotImplementedException("This implementation should not be used");
            #region commented out code

            //var fName = @"c:\Users\sivanov\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\Class1.cs";
            //var solution = SymbolBrowserPackage.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

            //IEnumHierarchies hiers;
            //ErrorHandler.Succeeded(solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, Guid.Empty, out hiers));
            //var projects = new IVsHierarchy[20];
            //uint actualCount;
            //ErrorHandler.Succeeded(hiers.Next((uint)projects.Length, projects, out actualCount));

            //uint pitemId = 0;
            //IVsProject3 containingProject = null;
            //foreach (IVsProject3 project in projects)
            //{
            //    int pfFound = 0;
            //    var docPriority = new VSDOCUMENTPRIORITY[0];
            //    ErrorHandler.Succeeded(project.IsDocumentInProject(fName, out pfFound, docPriority, out pitemId));

            //    if (pfFound > 0)
            //    {
            //        containingProject = project;
            //        break;
            //    }
            //}

            //if (null == containingProject) { return; }

            //IVsWindowFrame frame = null;
            //IntPtr documentData = IntPtr.Zero;
            //try
            //{
            //    Guid viewGuid = VSConstants.LOGVIEWID_Code;
            //    //SI: кажется не надо искать открыт ли этот документ или нет - Visual Studio сама это разруливает...
            //    ErrorHandler.ThrowOnFailure(containingProject.OpenItem(pitemId, ref viewGuid, documentData, out frame));
            //}
            //finally
            //{
            //    if (IntPtr.Zero != documentData)
            //    {
            //        Marshal.Release(documentData);
            //        documentData = IntPtr.Zero;
            //    }
            //}
            //// Make sure that the document window is visible.
            //ErrorHandler.ThrowOnFailure(frame.Show());

            //// Get the code window from the window frame.
            //object docView;
            //ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView));
            //IVsCodeWindow codeWindow = docView as IVsCodeWindow;
            //if (null == codeWindow)
            //{
            //    object docData;
            //    ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData));
            //    codeWindow = docData as IVsCodeWindow;
            //    if (null == codeWindow)
            //    {
            //        return;
            //    }
            //}

            //// Get the primary view from the code window.
            //IVsTextView textView;
            //ErrorHandler.ThrowOnFailure(codeWindow.GetPrimaryView(out textView));

            //// Set the cursor at the beginning of the declaration.
            //ErrorHandler.ThrowOnFailure(textView.SetCaretPos((int)lineNumber, (int)columnNumber));

            //// Make sure that the text is visible.
            //TextSpan visibleSpan = new TextSpan();
            //visibleSpan.iStartLine = lineNumber;
            //visibleSpan.iStartIndex = columnNumber;
            //visibleSpan.iEndLine = lineNumber;
            //visibleSpan.iEndIndex = columnNumber + 1;
            //ErrorHandler.ThrowOnFailure(textView.EnsureSpanVisible(visibleSpan));
            #endregion
        }

        public List<ResultList> Children { get { return children; } }

        #region Iron Python had some search for document pointer using RDT.
        // To me it seems that VS does the same thing itself

        //private IntPtr FindDocDataFromRDT(string fName)
        //{
        //    // Get a reference to the RDT.
        //    IVsRunningDocumentTable rdt = PythonPackage.GetGlobalService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
        //    if (null == rdt)
        //    {
        //        return IntPtr.Zero;
        //    }

        //    // Get the enumeration of the running documents.
        //    IEnumRunningDocuments documents;
        //    ErrorHandler.ThrowOnFailure(rdt.GetRunningDocumentsEnum(out documents));

        //    IntPtr documentData = IntPtr.Zero;
        //    uint[] docCookie = new uint[1];
        //    uint fetched;
        //    while ((VSConstants.S_OK == documents.Next(1, docCookie, out fetched)) && (1 == fetched))
        //    {
        //        uint flags;
        //        uint editLocks;
        //        uint readLocks;
        //        string moniker;
        //        IVsHierarchy docHierarchy;
        //        uint docId;
        //        IntPtr docData = IntPtr.Zero;
        //        try
        //        {
        //            ErrorHandler.ThrowOnFailure(
        //                rdt.GetDocumentInfo(docCookie[0], out flags, out readLocks, out editLocks, out moniker, out docHierarchy, out docId, out docData));
        //            // Check if this document is the one we are looking for.
        //            if ((docId == fileId) && (ownerHierarchy.Equals(docHierarchy)))
        //            {
        //                documentData = docData;
        //                docData = IntPtr.Zero;
        //                break;
        //            }
        //        }
        //        finally
        //        {
        //            if (IntPtr.Zero != docData)
        //            {
        //                Marshal.Release(docData);
        //            }
        //        }
        //    }

        //    return documentData;
        //}
        #endregion

        #region Implementation of IVsSimpleObjectList2
        /// <summary>
        /// Returns the attributes of the current tree list.
        /// </summary>
        /// <param name="pFlags"></param>
        /// <returns></returns>
        public int GetFlags(out uint pFlags)
        {
            pFlags = (uint)_VSTREEFLAGS.TF_NORELOCATE;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns an object list's capabilities.
        /// </summary>
        /// <param name="pgrfCapabilities"></param>
        /// <returns></returns>
        public int GetCapabilities2(out uint pgrfCapabilities)
        {
            pgrfCapabilities = /*(uint)_LIB_LISTCAPABILITIES.LLC_HASSOURCECONTEXT |*/ 
                (uint)_LIB_LISTCAPABILITIES2.LLC_ALLOWELEMENTSEARCH
                | (uint)_LIB_LISTCAPABILITIES.LLC_HASDESCPANE;
            
            //_LIB_LISTCAPABILITIES.LLC_HASDESCPANE |
            //_LIB_LISTCAPABILITIES.LLC_HASCOMMANDS | 
            //_LIB_LISTCAPABILITIES.LLC_HASBROWSEOBJ | 
            //_LIB_LISTCAPABILITIES.LLC_ALLOWSCCOPS | 
            //_LIB_LISTCAPABILITIES.LLC_ALLOWRENAME | 
            //_LIB_LISTCAPABILITIES.LLC_ALLOWDRAGDROP | 
            //_LIB_LISTCAPABILITIES.LLC_ALLOWDELETE

            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns the current change counter for the tree list, and is used to indicate that the list contents have changed
        /// </summary>
        /// <param name="pCurUpdate"></param>
        /// <returns></returns>
        public int UpdateCounter(out uint pCurUpdate)
        {
            pCurUpdate = 0;
            Logger.Log("ResultList.UpdateCounter count:" + pCurUpdate);
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns the number of items in the current tree list.
        /// </summary>
        /// <param name="pCount"></param>
        /// <returns></returns>
        public int GetItemCount(out uint pCount)
        {
            pCount = (uint)children.Count;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Retrieves data to draw the requested tree list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public int GetDisplayData(uint index, VSTREEDISPLAYDATA[] pData)
        {
            //ToDo: Find out where the displayData takes from in IronPython and supply it here
            Logger.Log("ResultList.GetDisplayData index:" + index);
            if (index >= (uint)children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pData[0] = children[(int)index].DisplayData;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns the text representations for the requested tree list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="tto"></param>
        /// <param name="pbstrText"></param>
        /// <returns></returns>
        public int GetTextWithOwnership(uint index, VSTREETEXTOPTIONS tto, out string pbstrText)
        {
            pbstrText = children[(int)index].symbolText;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns the tool tip text for the requested tree list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="eTipType"></param>
        /// <param name="pbstrText"></param>
        /// <returns></returns>
        public int GetTipTextWithOwnership(uint index, VSTREETOOLTIPTYPE eTipType, out string pbstrText)
        {
            pbstrText = children[(int)index].symbolText;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns the value for the specified category for the given list item. (LIB_CATEGORY enumeration)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Category"></param>
        /// <param name="pfCatField"></param>
        /// <returns></returns>
        public virtual int GetCategoryField2(uint index, int Category, out uint pfCatField)
        {
            Logger.Log("ResultList.GetCategoryField2, setting to E_NOTIMPL");
            pfCatField = (int)LIB_CATEGORY.LC_ACTIVEPROJECT;
            return VSConstants.E_NOTIMPL;
        }
        /// <summary>
        /// Returns a pointer to the property browse IDispatch for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppdispBrowseObj"></param>
        /// <returns></returns>
        public int GetBrowseObject(uint index, out object ppdispBrowseObj)
        {
            Logger.Log("ResultList.GetBrowseObject");
            ppdispBrowseObj = null;
            return VSConstants.E_NOTIMPL;
            //throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the user context object for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppunkUserCtx"></param>
        /// <returns></returns>
        public int GetUserContext(uint index, out object ppunkUserCtx)
        {
            // is used for IntelliSence?... (uint)_LIB_LISTCAPABILITIES.LLC_HASSOURCECONTEXT
            // Got called on saving and closing the application using these symbols, WTF?!
            ppunkUserCtx = null;
            return VSConstants.E_NOTIMPL;
        }
        /// <summary>
        /// Allows the list to display help for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int ShowHelp(uint index)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns a source filename and line number for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pbstrFilename"></param>
        /// <param name="pulLineNum"></param>
        /// <returns></returns>
        public int GetSourceContextWithOwnership(uint index, out string pbstrFilename, out uint pulLineNum)
        {
            pbstrFilename = fName;
            pulLineNum = (uint)lineNumber;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns the hierarchy and the number of ItemIDs corresponding to source files for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppHier"></param>
        /// <param name="pItemid"></param>
        /// <param name="pcItems"></param>
        /// <returns></returns>
        public int CountSourceItems(uint index, out IVsHierarchy ppHier, out uint pItemid, out uint pcItems)
        {
            Logger.Log("ResultList.CountSourceItems");
            ppHier = null;
            pItemid = 0;
            pcItems = 0;
            return 1;
            //throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the ItemID corresponding to source files for the given list item if more than one.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="grfGSI"></param>
        /// <param name="cItems"></param>
        /// <param name="rgItemSel"></param>
        /// <returns></returns>
        public int GetMultipleSourceItems(uint index, uint grfGSI, uint cItems, VSITEMSELECTION[] rgItemSel)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns a flag indicating if navigation to the given list item's source is supported.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="SrcType"></param>
        /// <param name="pfOK"></param>
        /// <returns></returns>
        int IVsSimpleObjectList2.CanGoToSource(uint index, VSOBJGOTOSRCTYPE SrcType, out int pfOK)
        {
            //if (ListToReference != null && index == ) {

            //    ListToReference.CanGoToSource(index, SrcType, out pfOK);
            //    return VSConstants.S_OK;
            //}

            Logger.Log("ResultList.CanGoToSource");
            if (index >= (uint)children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pfOK = children[(int)index].CanGoToSource ? 1 : 0;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// 	Navigates to the source for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="SrcType"></param>
        /// <returns></returns>
        int IVsSimpleObjectList2.GoToSource(uint index, VSOBJGOTOSRCTYPE SrcType)
        {
            if (index >= (uint)children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (children[(int)index].ListToReference != null)
                children[(int)index].ListToReference.GoToSource(0, SrcType);
            else
                children[(int)index].GotoSource(SrcType);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Allows the list to provide a different context menu and IOleCommandTarget for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pclsidActive"></param>
        /// <param name="pnMenuId"></param>
        /// <param name="ppCmdTrgtActive"></param>
        /// <returns></returns>
        public int GetContextMenu(uint index, out Guid pclsidActive, out int pnMenuId, out IOleCommandTarget ppCmdTrgtActive)
        {
            Logger.Log("ResultList.GetContextMenu, returning E_NOTIMPL");
            ppCmdTrgtActive = null;
            pnMenuId = 0;
            pclsidActive = new Guid();
            return VSConstants.E_NOTIMPL;
        }
        /// <summary>
        /// Returns a flag indicating whether the given list item supports a drag-and-drop operation.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pDataObject"></param>
        /// <param name="grfKeyState"></param>
        /// <param name="pdwEffect"></param>
        /// <returns></returns>
        public int QueryDragDrop(uint index, IDataObject pDataObject, uint grfKeyState, ref uint pdwEffect)
        {
            throw new NotImplementedException();
        }

        public int DoDragDrop(uint index, IDataObject pDataObject, uint grfKeyState, ref uint pdwEffect)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns a flag indicating if the given list item can be renamed.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pszNewName"></param>
        /// <param name="pfOK"></param>
        /// <returns></returns>
        public int CanRename(uint index, string pszNewName, out int pfOK)
        {
            throw new NotImplementedException();
        }

        public int DoRename(uint index, string pszNewName, uint grfFlags)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns a flag indicating if the given list item can be deleted.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pfOK"></param>
        /// <returns></returns>
        int IVsSimpleObjectList2.CanDelete(uint index, out int pfOK)
        {
            Logger.Log("ResultList.CanDelete");
            if (index >= (uint)children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            pfOK = children[(int)index].CanDelete ? 1 : 0;
            return VSConstants.S_OK;
        }

        public int DoDelete(uint index, uint grfFlags)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Asks the list item to provide description text to be used in the object browser.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="grfOptions"></param>
        /// <param name="pobDesc"></param>
        /// <returns></returns>
        public int FillDescription2(uint index, uint grfOptions, IVsObjectBrowserDescription3 pobDesc)
        {
            children[(int)index].FillDescription((_VSOBJDESCOPTIONS)grfOptions, pobDesc);
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Asks the given list item to enumerate its supported clipboard formats.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="grfFlags"></param>
        /// <param name="celt"></param>
        /// <param name="rgcfFormats"></param>
        /// <param name="pcActual"></param>
        /// <returns></returns>
        public int EnumClipboardFormats(uint index, uint grfFlags, uint celt, VSOBJCLIPFORMAT[] rgcfFormats, uint[] pcActual)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Asks the given list item to renders a specific clipboard format that it supports.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="grfFlags"></param>
        /// <param name="pFormatetc"></param>
        /// <param name="pMedium"></param>
        /// <returns></returns>
        public int GetClipboardFormat(uint index, uint grfFlags, FORMATETC[] pFormatetc, STGMEDIUM[] pMedium)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Asks the given list item to renders a specific clipboard format as a variant.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="grfFlags"></param>
        /// <param name="pcfFormat"></param>
        /// <param name="pvarFormat"></param>
        /// <returns></returns>
        public int GetExtendedClipboardVariant(uint index, uint grfFlags, VSOBJCLIPFORMAT[] pcfFormat, out object pvarFormat)
        {
            Logger.Log("ResultList.GetExtendedClipboardVariant");
            throw new NotImplementedException();
        }

        //protected object GetPropertyVal(_VSOBJLISTELEMPROPID propid)
        //{
        //    switch (propid)
        //    {
        //        case _VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME:
        //            return this.symbolText;                    
        //        case _VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FIRST:
        //        case _VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_LAST:
        //            return string.Empty;
        //        default:
        //            throw new NotImplementedException();
        //    }
        //}
        /// <summary>
        /// Returns the specified property for the specified list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="propid"></param>
        /// <param name="pvar"></param>
        /// <returns></returns>
        public int GetProperty(uint index, int propid, out object pvar)
        {
            Logger.Log(string.Format("ResultList.GetProperty index:{0} propid:{1}, out:null, returning VSConstants.E_NOTIMPL",
                index,
                Enum.GetName(typeof(_VSOBJLISTELEMPROPID), propid)));

            //pvar = children[(int)index].GetPropertyVal((_VSOBJLISTELEMPROPID)propid);
            //return VSConstants.S_OK;

            pvar = null;
            return VSConstants.E_NOTIMPL;
        }
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppNavInfo"></param>
        /// <returns></returns>
        public int GetNavInfo(uint index, out IVsNavInfo ppNavInfo)
        {
            ppNavInfo = null;
            return VSConstants.E_NOTIMPL;
        }
        /// <summary>
        /// 	Reserved for future use.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppNavInfoNode"></param>
        /// <returns></returns>
        public int GetNavInfoNode(uint index, out IVsNavInfoNode ppNavInfoNode)
        {
            Logger.Log("ResultList.GetNavInfoNode");
            ppNavInfoNode = children[(int)index];
            return VSConstants.S_OK;
            throw new NotImplementedException();
        }
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        /// <param name="pNavInfoNode"></param>
        /// <param name="pulIndex"></param>
        /// <returns></returns>
        public int LocateNavInfoNode(IVsNavInfoNode pNavInfoNode, out uint pulIndex)
        {
            Logger.Log("ResultList.LocateNavInfoNode   ");
            
            string needleName = string.Empty;
            pNavInfoNode.get_Name(out needleName);
            var index = 0;
            foreach(var c in children)
            {
                string nodeName = string.Empty;
                ((IVsNavInfoNode)c).get_Name(out nodeName);
                if (string.Compare(needleName, nodeName) == 0)
                {
                    pulIndex = (uint)index;
                    return VSConstants.S_OK;
                }
                index++;
            }
            pulIndex = 0;
            return VSConstants.E_FAIL;
        }
        /// <summary>
        /// Returns a flag indicating whether the given list item is expandable.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ListTypeExcluded"></param>
        /// <param name="pfExpandable"></param>
        /// <returns></returns>
        public int GetExpandable3(uint index, uint ListTypeExcluded, out int pfExpandable)
        {
            Logger.Log("ResultList.GetExpandable3");
            pfExpandable = children[(int)index].IsExpandable ? 1 : 0;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns a child IVsSimpleObjectList2 for the specified category.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ListType"></param>
        /// <param name="flags"></param>
        /// <param name="pobSrch"></param>
        /// <param name="ppIVsSimpleObjectList2"></param>
        /// <returns></returns>
        public int GetList2(uint index, uint ListType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2)
        {
            Logger.Log(string.Format(
                "ResultList.GetList2 index:{0} ListType: {1}",
                index,
                Enum.GetName(typeof(_LIB_LISTTYPE), ListType)));

            ppIVsSimpleObjectList2 = children[(int)index].FilterView((LibraryNodeType)ListType, pobSrch);

            return VSConstants.S_OK;
        }
        /// <summary>
        /// Notifies the current tree list that it is being closed.
        /// </summary>
        /// <param name="ptca"></param>
        /// <returns></returns>
        public int OnClose(VSTREECLOSEACTIONS[] ptca)
        {
            Logger.Log("ResultList.OnClose");
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Implementation of IVsNavInfoNode

        int IVsNavInfoNode.get_Name(out string pbstrName)
        {
            //Logger.Log("ResultList.IVsNavInfoNode.get_Name");
            pbstrName = UniqueName;
            return VSConstants.S_OK;
        }

        int IVsNavInfoNode.get_Type(out uint pllt)
        {
            //Logger.Log("ResultList.IVsNavInfoNode.get_Type");
            pllt = (uint)nodeType;
            return VSConstants.S_OK;
        }

        #endregion


    }
}
