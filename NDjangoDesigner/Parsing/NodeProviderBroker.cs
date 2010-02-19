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
        bool ShowDiagnostics{ get; }
        IVsOutputWindowPane DjangoDiagnostics { get; }
    }

    /// <summary>
    /// Allocates node porviders to text buffers
    /// </summary>
    [Export(typeof(INodeProviderBroker))]
    internal class NodeProviderBroker : INodeProviderBroker
    {
        IVsSolution ivsSolution = null;
        IVsOutputWindowPane djangoDiagnostics = null;

        public bool ShowDiagnostics
        {
            get
            {
                lock (this)
                {
                    if (ivsSolution == null)
                    {
                        ivsSolution = GetService<IVsSolution>(typeof(SVsSolution));
                    }
                }
                object slnName;
                int errCode =
                    ivsSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionFileName, out slnName);

                return (int)VSConstants.E_UNEXPECTED != errCode;
            }
        }

        public IVsOutputWindowPane DjangoDiagnostics
        {
            get
            {
                lock (this)
                {
                    if(djangoDiagnostics == null)
                    {
                        djangoDiagnostics = GetOutputPane();
                    }
                }
                return djangoDiagnostics;
            }
        }

        [Import]
        internal SVsServiceProvider ServiceProvider = null;

        [Import]
        internal IVsEditorAdaptersFactoryService adaptersFactory { get; set; }

        [Import]
        internal Microsoft.VisualStudio.Utilities.IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        IParser parser = InitializeParser();

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

        /// <summary>
        /// Determines whether the buffer conatins ndjango code
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns><b>true</b> if this is a ndjango buffer</returns>
        public bool IsNDjango(ITextBuffer buffer)
        {

            var types = new List<Microsoft.VisualStudio.Utilities.IContentType>(ContentTypeRegistryService.ContentTypes);

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
                provider = new NodeProvider(parser, buffer, this);
                buffer.Properties.AddProperty(typeof(NodeProvider), provider);
            }
            return provider;
        }        

        public IVsOutputWindowPane GetOutputPane()
        {
            Guid page = this.GetType().GUID;
            string caption = "Django Templates";

            IVsOutputWindow outputWindow = GetService<IVsOutputWindow>(typeof(SVsOutputWindow));

            IVsOutputWindowPane ppPane = null;
            if (ErrorHandler.Failed(outputWindow.GetPane(ref page, out ppPane)))
            {
                ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref page, caption, 1, 1));
                ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref page, out ppPane));
            }
            ErrorHandler.ThrowOnFailure(ppPane.Activate());
            return ppPane;
        }

        private T GetService<T>(Type serviceType)
        {
            return (T)ServiceProvider.GetService(serviceType);
        }
    }
}
