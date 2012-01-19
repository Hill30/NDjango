﻿/****************************************************************************
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
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.SymbolLibrary;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Shell;
using System.Runtime.Remoting.Messaging;
using System.ComponentModel.Design;

namespace NDjango.Designer.Parsing
{

      
    /// <summary>
    /// Manages a list of syntax nodes for a given buffer.
    /// </summary>
    public class NodeProvider
    {
        // it can take some time for the parser to build the token list.
        // for now let us initialize it to an empty list
        private List<DesignerNode> nodes = new List<DesignerNode>();
        
        // this lock is used to synchronize access to the nodes list
        private object node_lock = new object();
        public IProjectHandler Project { get;  set; }

        /// <summary>
        /// The delay (in milliseconds) of parser invoking. 
        /// </summary>
        
        private const int PARSING_DELAY = 500;
        /// <summary>
        /// The timer for optimization the parsing process. If there would be some changes with time 
        /// between sequential changes less then PARSING_DELAY, then rebuild process would be invoked only once.
        /// </summary>
        private Timer parserTimer;

        private NDjango.TypeResolver.ITypeResolver type_resolver;

        public string Filename { get; private set; }

        private NDjangoSymbolLibrary djangoSymbolLibrary;
        private uint libraryCookie;

        class ModelMeta
        {
            public string ModelClass { get; set; }
            public List<string> Members { get; set; }

            public IEnumerable<string> GetSymbols()
            {
                return Members.Select(member => member.Replace("Model", ModelClass)).ToList();
            }
        }

        /// <summary>
        /// Creates a new node provider
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="buffer">buffer to watch</param>
        public NodeProvider(IProjectHandler project, string filename, NDjango.TypeResolver.ITypeResolver type_resolver)
        {
            
            Project = project;
            this.type_resolver = type_resolver;
            Filename = filename;

            // we need to run rebuildNodes on a separate thread. Using timer
            // for this seems to be an overkill, but we need the timer anyway so - why not
            parserTimer = new Timer(rebuildNodes, null, 0, Timeout.Infinite);

            djangoSymbolLibrary = new NDjangoSymbolLibrary();
            //GlobalServices.ObjectManager.RegisterSimpleLibrary(djangoSymbolLibrary, out libraryCookie);

        }

        /// <summary>
        /// Initiates the delayed parsing in response to the buffer changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buffer_Changed(object sender, TextContentChangedEventArgs e)
        {

            // shut down the old one 
            parserTimer.Dispose();
            
            // put the call to the rebuildNodes on timer
            parserTimer = 
                new Timer(rebuildNodes,  null,  PARSING_DELAY, Timeout.Infinite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snapshotSpan"></param>
        public delegate void SnapshotEvent (SnapshotSpan snapshotSpan);

        /// <summary>
        /// This event is fired when an updated node list is ready to use
        /// </summary>
        public event SnapshotEvent NodesChanged;

        /// <summary>
        /// Builds a list of syntax nodes for a snapshot. This method is called on a separate thread
        /// </summary>
        private void rebuildNodes(object snapshotObject)
        {
            var nodes = Project.ParseTemplate(Filename, type_resolver);
            // get the snapshot used to parse the template. In theory it is possible to get a different one
            // if the parsing was requested again and there were changes since, by I do not think this is 
            // something to really happen
            var snapshot = Project.GetSnapshot(Filename);
            if (snapshot != null) // this is an overkill, I know
            {
                snapshot.TextBuffer.Changed -= new EventHandler<TextContentChangedEventArgs>(buffer_Changed); // just to prevent double-firing
                snapshot.TextBuffer.Changed += new EventHandler<TextContentChangedEventArgs>(buffer_Changed);
                List<DesignerNode> designer_nodes = nodes
                    .Aggregate(
                        new List<DesignerNode>(),
                        (list, node) => { list.Add( new DesignerNode(this, null, snapshot, (INode)node)); return list; }
                            );
                List<DesignerNode> oldNodes;
                lock (node_lock)
                {
                    oldNodes = this.nodes;
                    this.nodes = designer_nodes;
                }

                ModelMeta model = new ModelMeta();
                IndexNodes(this.nodes, model);

                oldNodes.ForEach(node => node.Dispose());
                designer_nodes.ForEach(node => node.ShowDiagnostics());
                RaiseNodesChanged(snapshot);
            }
        }



        private static void IndexNodes(IEnumerable<DesignerNode> nodes, ModelMeta model)
        {
            foreach (var designerNode in nodes)
            {

                switch (designerNode.NodeType)
                {
                    case NodeType.TypeName:
                        // got the model
                        string modelText = designerNode.SnapshotSpan.GetText();
                        model.ModelClass = modelText;
                        break;
                    case NodeType.Reference:
                        // got the member
                        string memberText = designerNode.SnapshotSpan.GetText();
                        if(model.Members == null) model.Members = new List<string>();
                        model.Members.Add(memberText);
                        break;
                }
                

                if (designerNode.Children.Count > 0)
                {
                    IndexNodes(designerNode.Children, model);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="NodesChanged"/> event
        /// </summary>
        /// <param name="snapshot"></param>
        internal void RaiseNodesChanged(ITextSnapshot snapshot)
        {
            if (NodesChanged != null)
                NodesChanged(new SnapshotSpan(snapshot, 0, snapshot.Length));
        }

        /// <summary>
        /// Returns a list of nodes in the specified snapshot span
        /// </summary>
        /// <param name="snapshotSpan"></param>
        /// <returns></returns>
        internal List<DesignerNode> GetNodes(ITextSnapshot snapshot)
        {
            List<DesignerNode> nodes;
            lock (node_lock)
            {
                nodes = this.nodes;

                // just in case if while the tokens list was being rebuilt
                // another modification was made
                if (nodes.Count > 0 && this.nodes[0].SnapshotSpan.Snapshot != snapshot)
                    this.nodes.ForEach(node => node.TranslateTo(snapshot));
            }

            return nodes;
        }

        /// <summary>
        /// Returns a list of django syntax nodes in the specified snapshot span
        /// </summary>
        /// <param name="snapshotSpan"></param>
        /// <param name="predicate">the predicate controlling what nodes to include in the list</param>
        /// <returns></returns>
        internal List<DesignerNode> GetNodes(SnapshotSpan snapshotSpan, Predicate<DesignerNode> predicate)
        {
            return GetNodes(snapshotSpan, GetNodes(snapshotSpan.Snapshot))
                .FindAll(predicate);
        }

        /// <summary>
        /// Walks the syntax node tree building a flat list of nodes intersecting with the span
        /// </summary>
        /// <param name="snapshotSpan"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private List<DesignerNode> GetNodes(SnapshotSpan snapshotSpan, IEnumerable<DesignerNode> nodes)
        {
            List<DesignerNode> result = new List<DesignerNode>();
            foreach (DesignerNode node in nodes)
            {
                if (node.SnapshotSpan.IntersectsWith(snapshotSpan) || node.ExtensionSpan.IntersectsWith(snapshotSpan))
                    result.Add(node);
                result.AddRange(GetNodes(snapshotSpan, node.Children));
            }
            return result;
        }

        /// <summary>
        /// Returns a list of django syntax nodes based on the point in the text buffer
        /// </summary>
        /// <param name="point">point identifiying the desired node</param>
        /// <param name="predicate">the predicate controlling what nodes to include in the list</param>
        /// <returns></returns>
        internal List<DesignerNode> GetNodes(SnapshotPoint point, Predicate<DesignerNode> predicate)
        {
            return GetNodes(new SnapshotSpan(point.Snapshot, point.Position, 0), predicate);
        }

        internal void Dispose()
        {
            nodes.ForEach(node => node.Dispose());
            Project.Unregister(Filename);
        }

        internal void RemoveDiagnostics(ErrorTask errorTask)
        {
            Project.RemoveDiagnostics(errorTask);
        }

        internal void ShowDiagnostics(ErrorTask errorTask)
        {
            Project.ShowDiagnostics(errorTask);
        }
    }
}
