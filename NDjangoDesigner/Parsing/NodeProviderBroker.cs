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
    }

    /// <summary>
    /// Allocates node porviders to text buffers
    /// </summary>
    [Export(typeof(INodeProviderBroker))]
    internal class NodeProviderBroker : INodeProviderBroker
    {

        private static IParser InitializeParser()
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

        private static void CreateEntry<T>(List<T> list, Type t) where T:class
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

        IParser parser = InitializeParser();

        [Import]
        internal IVsEditorAdaptersFactoryService adaptersFactory { get; set; }

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
            lock (this)
                if (!initialized)
                {
                    djangoDiagnostics = GetOutputPane(buffer);
                    initialized = true;
                }

            NodeProvider provider;
            if (!buffer.Properties.TryGetProperty(typeof(NodeProvider), out provider))
            {
                provider = new NodeProvider(djangoDiagnostics, parser, buffer);
                buffer.Properties.AddProperty(typeof(NodeProvider), provider);
            }
            return provider;
        }
        bool initialized = false;
        IVsOutputWindowPane djangoDiagnostics = null;

        public IVsOutputWindowPane GetOutputPane(ITextBuffer textBuffer)
        {
            Guid page = this.GetType().GUID;
            string caption = "Django Templates";

            IVsOutputWindow service = GetService<IVsOutputWindow>(textBuffer, typeof(SVsOutputWindow));

            IVsOutputWindowPane ppPane = null;
            if ((ErrorHandler.Failed(service.GetPane(ref page, out ppPane)) && (caption != null)) 
                && ErrorHandler.Succeeded(service.CreatePane(ref page, caption, 1, 1)))
            {
                service.GetPane(ref page, out ppPane);
            }
            if (ppPane != null)
            {
                ErrorHandler.ThrowOnFailure(ppPane.Activate());
            }
            return ppPane;
        }

        private T GetService<T>(ITextBuffer textBuffer, Type serviceType)
        {
            var vsBuffer = adaptersFactory.GetBufferAdapter(textBuffer);
            if (vsBuffer == null)
                return default(T);

            Guid guidServiceProvider = VSConstants.IID_IUnknown;
            IObjectWithSite objectWithSite = vsBuffer as IObjectWithSite;
            IntPtr ptrServiceProvider = IntPtr.Zero;
            objectWithSite.GetSite(ref guidServiceProvider, out ptrServiceProvider);
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider =
                (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)Marshal.GetObjectForIUnknown(ptrServiceProvider);

            Guid guidService = serviceType.GUID;
            Guid guidInterface = typeof(T).GUID;
            IntPtr ptrObject = IntPtr.Zero;

            int hr = serviceProvider.QueryService(ref guidService, ref guidInterface, out ptrObject);
            if (ErrorHandler.Failed(hr) || ptrObject == IntPtr.Zero)
                return default(T);

            T result = (T)Marshal.GetObjectForIUnknown(ptrObject);
            Marshal.Release(ptrObject);

            return result;
        }
        
    }
}
