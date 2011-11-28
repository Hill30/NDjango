using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser
{
    public class ResultList : IVsSimpleObjectList2
    {
        private string symbolText = string.Empty;
        private List<ResultList> children = new List<ResultList>();
        private uint updateCount = 0;

        public ResultList(string text)
        {
            symbolText = text;
        }

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
            pgrfCapabilities = (uint)_LIB_LISTCAPABILITIES.LLC_HASSOURCECONTEXT;
            return VSConstants.S_OK;
            //_LIB_LISTCAPABILITIES.LLC_HASDESCPANE |
            //_LIB_LISTCAPABILITIES.LLC_HASCOMMANDS | 
            //_LIB_LISTCAPABILITIES.LLC_HASBROWSEOBJ | 
            //_LIB_LISTCAPABILITIES.LLC_ALLOWSCCOPS | 
            //_LIB_LISTCAPABILITIES.LLC_ALLOWRENAME | 
            //_LIB_LISTCAPABILITIES.LLC_ALLOWDRAGDROP | 
            //_LIB_LISTCAPABILITIES.LLC_ALLOWDELETE
            
            //throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the current change counter for the tree list, and is used to indicate that the list contents have changed
        /// </summary>
        /// <param name="pCurUpdate"></param>
        /// <returns></returns>
        public int UpdateCounter(out uint pCurUpdate)
        {
            pCurUpdate = updateCount;
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
            throw new NotImplementedException();
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
            pbstrText = symbolText;
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
            pbstrText = symbolText;
            return VSConstants.S_OK;
        }
        /// <summary>
        /// Returns the value for the specified category for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Category"></param>
        /// <param name="pfCatField"></param>
        /// <returns></returns>
        public int GetCategoryField2(uint index, int Category, out uint pfCatField)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns a pointer to the property browse IDispatch for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppdispBrowseObj"></param>
        /// <returns></returns>
        public int GetBrowseObject(uint index, out object ppdispBrowseObj)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the user context object for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppunkUserCtx"></param>
        /// <returns></returns>
        public int GetUserContext(uint index, out object ppunkUserCtx)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
        public int CanGoToSource(uint index, VSOBJGOTOSRCTYPE SrcType, out int pfOK)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 	Navigates to the source for the given list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="SrcType"></param>
        /// <returns></returns>
        public int GoToSource(uint index, VSOBJGOTOSRCTYPE SrcType)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
        public int CanDelete(uint index, out int pfOK)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the specified property for the specified list item.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="propid"></param>
        /// <param name="pvar"></param>
        /// <returns></returns>
        public int GetProperty(uint index, int propid, out object pvar)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppNavInfo"></param>
        /// <returns></returns>
        public int GetNavInfo(uint index, out IVsNavInfo ppNavInfo)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 	Reserved for future use.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ppNavInfoNode"></param>
        /// <returns></returns>
        public int GetNavInfoNode(uint index, out IVsNavInfoNode ppNavInfoNode)
        {
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        /// <summary>
        /// Notifies the current tree list that it is being closed.
        /// </summary>
        /// <param name="ptca"></param>
        /// <returns></returns>
        public int OnClose(VSTREECLOSEACTIONS[] ptca)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
