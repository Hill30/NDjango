/****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Designer.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using NDjango.Interfaces;
using IOLEServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.FSharp.Collections;

namespace NDjango.Designer.Parsing
{

    public interface INodeProviderBroker
    {
        NodeProvider GetNodeProvider(ITextBuffer buffer);
        bool IsNDjango(ITextBuffer buffer);
        void ShowDiagnostics(ErrorTask task);
        void RemoveDiagnostics(ErrorTask task);
    }

    /// <summary>
    /// Allocates node porviders to text buffers
    /// </summary>
    [Export(typeof(INodeProviderBroker))]
    public class NodeProviderBroker : INodeProviderBroker, IVsRunningDocTableEvents, IVsSolutionEvents
    {

        public NodeProviderBroker()
        {
            ErrorHandler.ThrowOnFailure(GlobalServices.RDT.AdviseRunningDocTableEvents(this, out RDTEventsCookie));
            ErrorHandler.ThrowOnFailure(GlobalServices.Solution.AdviseSolutionEvents(this, out SolutionEventsCookie));
        }

        private uint RDTEventsCookie;
        private uint SolutionEventsCookie;

        private Dictionary<string, ProjectHandler> projects = new Dictionary<string, ProjectHandler>();

        [Import]
        GlobalServices services;

        #region Diagnostic handling

        /// <summary>
        /// Adds a diganostic message defined by the parameter to the tasklist
        /// </summary>
        /// <param name="task">the object representing the error message</param>
        public void ShowDiagnostics(ErrorTask task)
        {
            task.Navigate += new EventHandler(NavigateTo);
            GlobalServices.TaskList.Tasks.Add(task);
        }

        /// <summary>
        /// Removes a diganostic message defined by the parameter from the tasklist
        /// </summary>
        /// <param name="task">the object representing the error message</param>
        public void RemoveDiagnostics(ErrorTask task)
        {
            GlobalServices.TaskList.Tasks.Remove(task);
        }

        /// <summary>
        /// Navigates to the location of the error in the source code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void NavigateTo(object sender, EventArgs arguments)
        {
            Microsoft.VisualStudio.Shell.Task task = sender as Microsoft.VisualStudio.Shell.Task;
            if (task == null)
                throw new ArgumentException("Sender is not a Microsoft.VisualStudio.Shell.Task", "sender");

            // Get the doc data for the task's document
            if (String.IsNullOrEmpty(task.Document))
                return;

            IVsUIShellOpenDocument openDoc = services.GetService<IVsUIShellOpenDocument>();
            if (openDoc == null)
                return;

            IVsWindowFrame frame;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp;
            IVsUIHierarchy hier;
            uint itemid;
            Guid logicalView = VSConstants.LOGVIEWID_Code;

            if (Microsoft.VisualStudio.ErrorHandler.Failed(openDoc.OpenDocumentViaProject(task.Document, ref logicalView, out sp, out hier, out itemid, out frame)) || frame == null)
                return;

            object docData;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData));

            // Get the VsTextBuffer
            VsTextBuffer buffer = docData as VsTextBuffer;
            if (buffer == null)
            {
                IVsTextBufferProvider bufferProvider = docData as IVsTextBufferProvider;
                if (bufferProvider != null)
                {
                    IVsTextLines lines;
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));
                    buffer = lines as VsTextBuffer;
                    System.Diagnostics.Debug.Assert(buffer != null, "IVsTextLines does not implement IVsTextBuffer");
                    if (buffer == null)
                        return;
                }
            }

            // Finally, perform the navigation.
            IVsTextManager mgr = services.GetService<IVsTextManager>(typeof(VsTextManagerClass));
            if (mgr == null)
                return;

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(mgr.NavigateToLineAndColumn(buffer, ref logicalView, task.Line, task.Column, task.Line, task.Column));
        }

        #endregion

        /// <summary>
        /// Determines whether the buffer conatins ndjango code
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns><b>true</b> if this is a ndjango buffer</returns>
        public bool IsNDjango(ITextBuffer buffer)
        {
            switch (buffer.ContentType.TypeName)
            {
                case "plaintext":
                case "HTML":
                case "XML":
                    // there is no file associated with the buffer
                    // it looks like a buffer created for a tool tip 
                    // we do not need to mess with those
                    if (!buffer.Properties.ContainsProperty(typeof(ITextDocument)))
                        return false;
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Retrieves or creates a node provider for a buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public NodeProvider GetNodeProvider(ITextBuffer buffer)
        {
            NodeProvider provider;
            if (!buffer.Properties.TryGetProperty(typeof(NodeProvider), out provider))
            {
                var adapter = editorFactoryService.GetBufferAdapter(buffer);
                string filename;
                uint format;
                ErrorHandler.ThrowOnFailure(((IPersistFileFormat)adapter).GetCurFile(out filename, out format));

                IVsHierarchy hier;
                uint itemId;
                IntPtr docData;
                uint cookie;
                GlobalServices.RDT.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, filename, out hier, out itemId, out docData, out cookie);
                if (IntPtr.Zero != docData)
                    Marshal.Release(docData);

                object objDirectory;
                ErrorHandler.ThrowOnFailure(hier.GetProperty((uint)VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out objDirectory));
                
                var project_directory = (string)objDirectory;


                ProjectHandler project;

                lock (projects)
                {
                    if (!projects.TryGetValue(project_directory, out project))
                    {
                        project = new ProjectHandler(this, hier, project_directory);
                        projects.Add(project_directory, project);
                    }
                }

                provider = project.GetNodeProvider(buffer, hier, filename);

            }
            return provider;
        }

        [Import]
        IVsEditorAdaptersFactoryService editorFactoryService = null; // null is not really necessary, but to keep the compiler happy...

        void ApplyToProvider(Func<IntPtr> docGetter, Action<NodeProvider> action)
        {
            var docData = docGetter();
            try
            {
                IVsTextLines textLines = Marshal.GetObjectForIUnknown(docData) as IVsTextLines;
                if (textLines == null)
                {
                    IVsTextBufferProvider vsTextBufferProvider = Marshal.GetObjectForIUnknown(docData) as IVsTextBufferProvider;
                    if (vsTextBufferProvider != null)
                        ErrorHandler.ThrowOnFailure(vsTextBufferProvider.GetTextBuffer(out textLines));
                }
                if (textLines != null)
                {
                    var textBuffer = editorFactoryService.GetDocumentBuffer((IVsTextBuffer)textLines);
                    NodeProvider provider;
                    if (textBuffer.Properties.TryGetProperty<NodeProvider>(typeof(NodeProvider), out provider))
                        action(provider);
                }
            }
            finally
            {
                Marshal.Release(docData);
            }
        }

        #region IVsRunningDocTableEvents Members

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {

            if (dwEditLocksRemaining == 0 && dwReadLocksRemaining == 0)
                ApplyToProvider(
                    () =>
                    {
                        uint pgrfRDTFlags;
                        uint pdwReadLocks;
                        uint pdwEditLocks;
                        string pbstrMkDocument;
                        IVsHierarchy ppHier;
                        uint pitemid;
                        IntPtr ppunkDocData;

                        ErrorHandler.ThrowOnFailure(
                            GlobalServices.RDT.GetDocumentInfo(
                                docCookie,
                                out pgrfRDTFlags,
                                out pdwReadLocks,
                                out pdwEditLocks,
                                out pbstrMkDocument,
                                out ppHier,
                                out pitemid,
                                out ppunkDocData));
                        return ppunkDocData;
                    },
                        provider => { provider.Dispose(); }
                            );
            return VSConstants.S_OK;
        }

        #endregion

        private void CloseProjectHandler(IVsHierarchy hier)
        {
            object objDirectory;
            ErrorHandler.ThrowOnFailure(hier.GetProperty((uint)VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out objDirectory));

            var project_directory = (string)objDirectory;


            ProjectHandler project;

            lock (projects)
            {
                if (projects.TryGetValue(project_directory, out project))
                {
                    project.Dispose();
                    projects.Remove(project_directory);
                }
            }

        }

        #region IVsSolutionEvents Members

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            CloseProjectHandler(pHierarchy);
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
