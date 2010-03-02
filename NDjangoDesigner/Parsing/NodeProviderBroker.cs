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

namespace NDjango.Designer.Parsing
{

    internal interface INodeProviderBroker
    {
        NodeProvider GetNodeProvider(ITextBuffer buffer);
        bool IsNDjango(ITextBuffer buffer);
        Microsoft.FSharp.Collections.FSharpList<INodeImpl> ParseTemplate(TextReader template);
        void ShowDiagnostics(ErrorTask task);
        void RemoveDiagnostics(ErrorTask task);
    }

    /// <summary>
    /// Allocates node porviders to text buffers
    /// </summary>
    [Export(typeof(INodeProviderBroker))]
    internal class NodeProviderBroker : INodeProviderBroker, IVsRunningDocTableEvents
    {

        #region Broker Initialization routines

        private NodeProviderBroker()
        {
            parser = InitializeParser();
        }

        IParser parser;

        private IParser InitializeParser()
        {
            string path = typeof(TemplateManagerProvider).Assembly.CodeBase;
            List<Tag> tags = new List<Tag>();
            List<Filter> filters = new List<Filter>();
            if (path.StartsWith("file:///"))
                foreach (string file in
                    Directory.EnumerateFiles(
                        Path.GetDirectoryName(path.Substring(8)),
                        "*.NDjangoExtension.dll",
                        SearchOption.AllDirectories))
                {
                    AssemblyName name = new AssemblyName();
                    name.CodeBase = file;
                    foreach (Type t in Assembly.Load(name).GetExportedTypes())
                    {
                        if (typeof(ITag).IsAssignableFrom(t))
                            CreateEntry<Tag>(tags, t);
                        if (typeof(ISimpleFilter).IsAssignableFrom(t))
                            CreateEntry<Filter>(filters, t);
                    }
                }

            TemplateManagerProvider parser = new TemplateManagerProvider();
            return parser
                    .WithTags(tags)
                    .WithFilters(filters)
                    .WithSetting(NDjango.Constants.EXCEPTION_IF_ERROR, false);

        }

        private static void CreateEntry<T>(List<T> list, Type t) where T : class
        {
            if (t.IsAbstract)
                return;
            if (t.IsInterface)
                return;

            var attrs = t.GetCustomAttributes(typeof(NameAttribute), false) as NameAttribute[];
            if (attrs.Length == 0)
                return;

            if (t.GetConstructor(new Type[] { }) == null)
                return;

            list.Add((T)Activator.CreateInstance(typeof(T), attrs[0].Name, Activator.CreateInstance(t)));
        }

        #endregion

        private SVsServiceProvider serviceProvider;

        [Import]
        internal SVsServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            private set
            {
                serviceProvider = value;
                taskList = new TaskProvider(serviceProvider);
                rdt = GetService<IVsRunningDocumentTable>(typeof(SVsRunningDocumentTable));
                ErrorHandler.ThrowOnFailure(rdt.AdviseRunningDocTableEvents(this, out rdtEventsCookie));
            }
        }

        private T GetService<T>(Type serviceType)
        {
            return (T)ServiceProvider.GetService(serviceType);
        }

        #region Diagnostic handling

        private TaskProvider taskList;

        /// <summary>
        /// Adds a diganostic message defined by the parameter to the tasklist
        /// </summary>
        /// <param name="task">the object representing the error message</param>
        public void ShowDiagnostics(ErrorTask task)
        {
            task.Navigate += new EventHandler(NavigateTo);
            taskList.Tasks.Add(task);
        }

        /// <summary>
        /// Removes a diganostic message defined by the parameter from the tasklist
        /// </summary>
        /// <param name="task">the object representing the error message</param>
        public void RemoveDiagnostics(ErrorTask task)
        {
            taskList.Tasks.Remove(task);
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

            IVsUIShellOpenDocument openDoc = serviceProvider.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
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
            IVsTextManager mgr = serviceProvider.GetService(typeof(VsTextManagerClass)) as IVsTextManager;
            if (mgr == null)
                return;

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(mgr.NavigateToLineAndColumn(buffer, ref logicalView, task.Line, task.Column, task.Line, task.Column));
        }

        #endregion

        /// <summary>
        /// Parses the template
        /// </summary>
        /// <param name="template">a reader with the template</param>
        /// <returns>A list of the syntax nodes</returns>
        public Microsoft.FSharp.Collections.FSharpList<INodeImpl> ParseTemplate(TextReader template)
        {
            return parser.ParseTemplate(template);
        }

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
                provider = new NodeProvider(this, buffer);
                buffer.Properties.AddProperty(typeof(NodeProvider), provider);
            }
            return provider;
        }

        [Import]
        IVsEditorAdaptersFactoryService editorFactoryService = null; // null is not really necessary, but to keep the compiler happy...

        IVsRunningDocumentTable rdt;

        uint rdtEventsCookie;


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
        	uint pgrfRDTFlags;
	        uint pdwReadLocks;
	        uint pdwEditLocks;
	        string pbstrMkDocument;
	        IVsHierarchy ppHier;
	        uint pitemid;
	        IntPtr ppunkDocData;

            ErrorHandler.ThrowOnFailure(rdt.GetDocumentInfo(docCookie, out pgrfRDTFlags, out pdwReadLocks, out pdwEditLocks, out pbstrMkDocument, out ppHier, out pitemid, out ppunkDocData));
            try
            {
                if (pdwReadLocks == 0 && pdwEditLocks == 0)
                {
                    // The last lock removed - the buffer will be destroyed. 
                    // Let's try to remove any diagnostic messages associated with it
                    IVsTextLines textLines = Marshal.GetObjectForIUnknown(ppunkDocData) as IVsTextLines;
                    if (textLines == null)
                    {
                        IVsTextBufferProvider vsTextBufferProvider = Marshal.GetObjectForIUnknown(ppunkDocData) as IVsTextBufferProvider;
                        if (vsTextBufferProvider != null)
                        {
                            ErrorHandler.ThrowOnFailure(vsTextBufferProvider.GetTextBuffer(out textLines));
                        }
                    }
                    if (textLines != null)
                    {
                        var textBuffer = editorFactoryService.GetDocumentBuffer((IVsTextBuffer)textLines);
                        NodeProvider provider;
                        if (textBuffer.Properties.TryGetProperty<NodeProvider>(typeof(NodeProvider), out provider))
                        {
                            provider.Dispose();
                        }
                    }
                }
            }
            finally
            {
                Marshal.Release(ppunkDocData);
            }
               

            return VSConstants.S_OK;
        }

        #endregion
    }
}
