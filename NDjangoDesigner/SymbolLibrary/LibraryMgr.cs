using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace NDjango.Designer.SymbolLibrary
{
    public interface ILibraryMgr
    {
        void Initialize();
        void Dispose();
    }

    [Export(typeof(ILibraryMgr))]
    public class LibraryMgr : ILibraryMgr, IVsLibraryMgr
    {

        [Import]
        private GlobalServices services;

        private NDjangoSymbolLibrary library;
        private uint libCookie;
        private uint mgrCookie;
        private IVsObjectManager2 objectManager;

        #region IVsLibraryMgr Members

        public int GetCheckAt(uint nLibIndex, LIB_CHECKSTATE[] pstate)
        {
            if (nLibIndex != 0)
                return VSConstants.E_UNEXPECTED;
            throw new NotImplementedException();
        }

        public int GetCount(out uint pnCount)
        {
            pnCount = 1;
            return VSConstants.S_OK;
        }

        public int GetLibraryAt(uint nLibIndex, out IVsLibrary ppLibrary)
        {
            ppLibrary = null;
            if (nLibIndex != 0)
                throw new Exception("How stupid");
                //return VSConstants.E_INVALIDARG;
            ppLibrary = library;
            return VSConstants.S_OK;
        }

        public int GetNameAt(uint nLibIndex, IntPtr pszName)
        {
            if (nLibIndex != 0)
                return VSConstants.E_UNEXPECTED;
            throw new NotImplementedException();
        }

        public int SetLibraryGroupEnabled(LIB_PERSISTTYPE lpt, int fEnable)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int ToggleCheckAt(uint nLibIndex)
        {
            if (nLibIndex != 0)
                return VSConstants.E_UNEXPECTED;
            throw new NotImplementedException();
        }

        #endregion

        #region ILibraryMgr Members

        void ILibraryMgr.Initialize()
        {
            library = new NDjangoSymbolLibrary();
            objectManager = services.GetService<IVsObjectManager2, SVsObjectManager>();
            var mgrGuid = GetType().GUID;
            ErrorHandler.ThrowOnFailure(objectManager.RegisterSimpleLibrary(library, out libCookie));
//            ErrorHandler.ThrowOnFailure(((IVsObjectManager)objectManager).RegisterLibMgr(mgrGuid, this, out mgrCookie));
        }

        void ILibraryMgr.Dispose()
        {
            ((IVsObjectManager)objectManager).UnregisterLibMgr(mgrCookie);
            objectManager.UnregisterLibrary(libCookie);
        }

        #endregion
    }
}
