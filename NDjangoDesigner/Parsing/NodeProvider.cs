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
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Shell;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Manages a list of syntax nodes for a given buffer.
    /// </summary>
    class NodeProvider
    {
        // it can take some time for the parser to build the token list.
        // for now let us initialize it to an empty list
        private List<DesignerNode> nodes = new List<DesignerNode>();
        
        // this lock is used to synchronize access to the nodes list
        private object node_lock = new object();
        private ITextBuffer buffer;
        public INodeProviderBroker Broker { get; private set; }

        /// <summary>
        /// indicates the delay (in milliseconds) of parser invoking. 
        /// </summary>
        private const int PARSING_DELAY = 500;
        /// <summary>
        /// The timer for optimization the parsing process. If there would be some changes with time 
        /// between sequential changes less then PARSING_DELAY, then rebuild process would be invoked only once.
        /// </summary>
        private Timer parserTimer;
        
        /// <summary>
        /// Creates a new node provider
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="buffer">buffer to watch</param>
        public NodeProvider(INodeProviderBroker broker, ITextBuffer buffer)
        {
            Broker = broker;
            this.buffer = buffer;
            FilePath = ((ITextDocument)buffer.Properties[typeof(ITextDocument)]).FilePath;

            buffer.Changed += new EventHandler<TextContentChangedEventArgs>(buffer_Changed);
            // we need to run rebuildNodes on a separate thread. Using timer
            // for this seems to be an overkill, but we need the timer anyway so - why not
            parserTimer =
                new Timer(rebuildNodes, buffer.CurrentSnapshot, 0, Timeout.Infinite);
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
                new Timer(rebuildNodes,  e.After,  PARSING_DELAY, Timeout.Infinite);
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
        /// TextReader wrapper around text in the buffer
        /// </summary>
        class SnapshotReader : TextReader
        {
            ITextSnapshot snapshot;
            int pos = 0;
            public SnapshotReader(ITextSnapshot snapshot)
            {
                this.snapshot = snapshot;
            }

            public override int Read(char[] buffer, int index, int count)
            {
                int actual = snapshot.Length - pos;
                if (actual > count)
                    actual = count;
                if (actual > 0)
                    snapshot.ToCharArray(pos, actual).CopyTo(buffer, index);
                pos += actual;
                return actual;
            }
        }

        /// <summary>
        /// Builds a list of syntax nodes for a snapshot. This method is called on a separate thread
        /// </summary>
        private void rebuildNodes(object snapshotObject)
        {
            ITextSnapshot snapshot = (ITextSnapshot)snapshotObject;
            List<DesignerNode> nodes = Broker.ParseTemplate(new SnapshotReader(snapshot))
                .Aggregate(
                    new List<DesignerNode>(),
                    (list, node) => { list.Add(CreateDesignerNode(null, snapshot, (INode)node)); return list; }
                        );
            List<DesignerNode> oldNodes;
            lock (node_lock)
            {
                oldNodes = this.nodes;
                this.nodes = nodes;
            }
            oldNodes.ForEach(node => node.Dispose());
            nodes.ForEach(node => node.ShowDiagnostics());
            RaiseNodesChanged(snapshot);
        }

        internal DesignerNode CreateDesignerNode(DesignerNode parent, ITextSnapshot snapshot, INode node)
        {
            switch (node.NodeType)
            {
                case NodeType.TemplateName:
                    return new TemplateNameNode(this, parent, snapshot, node);
                default:
                    return new DesignerNode(this, parent, snapshot, node);
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
        /// Shows diagnostic message associated with the node
        /// </summary>
        /// <param name="task"></param>
        internal void ShowDiagnostics(ErrorTask task)
        {
            Broker.ShowDiagnostics(task);
        }

        /// <summary>
        /// Removes diagnostic message associated with the node
        /// </summary>
        /// <param name="task"></param>
        internal void RemoveDiagnostics(ErrorTask task)
        {
            Broker.RemoveDiagnostics(task);
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

        public string FilePath { get; private set; }

        internal void Dispose()
        {
            nodes.ForEach(node => node.Dispose()); 
        }
    }
}
