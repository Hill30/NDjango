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
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;
using NDjango.Interfaces;

namespace NDjango.Designer.Classifiers
{
    /// <summary>
    /// Supplies a list of <see cref="ClassificationSpan"/> according to the specs of <see cref="IClassifier"/> interface
    /// </summary>
    /// <remarks>
    /// When an instance of the <see cref="Classifier"/> is created for a buffer it requests from the 
    /// <see cref="Parser"/> an instance of the <see cref="NodeProvider"/> for the specified buffer and subscribes
    /// to the TagsChanged event of the tokenizer. From this moment on work of the <see cref="Classifier"/> is 
    /// controlled by the recieved instance of the <see cref="NodeProvider"/>.
    /// </remarks>
    internal class Classifier : IClassifier
    {
        private IClassificationTypeRegistryService classificationTypeRegistry;
        private NodeProvider nodeProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="Classifier"/>
        /// </summary>
        /// <param name="nodeProviderBroker"></param>
        /// <param name="classificationTypeRegistry"></param>
        /// <param name="buffer"></param>
        public Classifier(INodeProviderBroker nodeProviderBroker, IClassificationTypeRegistryService classificationTypeRegistry, ITextBuffer buffer)
        {
            this.classificationTypeRegistry = classificationTypeRegistry;
            nodeProvider = nodeProviderBroker.GetNodeProvider(buffer);
            nodeProvider.NodesChanged += new NodeProvider.SnapshotEvent(nodeProvider_TagsChanged);
        }

        /// <summary>
        /// Bubbles up the notification about buffer changes
        /// </summary>
        /// <param name="snapshotSpan"></param>
        private void nodeProvider_TagsChanged(SnapshotSpan snapshotSpan)
        {
            if (ClassificationChanged != null)
                ClassificationChanged(this, new ClassificationChangedEventArgs(snapshotSpan));
        }

        /// <summary>
        /// Provides a list of <see cref="ClassificationSpan"/> objects for the specified span
        /// </summary>
        /// <param name="span">span for which the list is requested</param>
        /// <returns></returns>
        /// <remarks>The list is generated based on the list of <see cref="TokenSnapshots"/> recieved
        /// from the tokenizer</remarks> 
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            List<ClassificationSpan> classifications = new List<ClassificationSpan>();

            // create classifiers for currently selected tag (if any)
            DesignerNode selection;
            if (span.Snapshot.TextBuffer.Properties
                .TryGetProperty<DesignerNode>(typeof(Highlighter), out selection) && selection != null)
            {
                
                // colorize the selected tag name
                classifications.Add(
                    new ClassificationSpan(
                        selection.SnapshotSpan,
                        classificationTypeRegistry.GetClassificationType(Constants.DJANGO_SELECTED_TAGNAME)
                        )
                    );

                // colorize the selected tag itself
                DesignerNode tag = selection.Parent;
                classifications.Add(
                    new ClassificationSpan(
                        tag.SnapshotSpan,
                        classificationTypeRegistry.GetClassificationType(Constants.DJANGO_SELECTED_TAG)
                        )
                    );

                // for every context defined in the tag
                foreach (DesignerNode child in tag.Children)
                {
                    // colorize the context
                    if (child.NodeType == NodeType.ParsingContext)
                        classifications.Add(
                            new ClassificationSpan(
                                child.ExtensionSpan,
                                classificationTypeRegistry.GetClassificationType(Constants.DJANGO_SELECTED_TAG)
                                )
                            );
                    // locate the closing tag for context
                    if (child.NodeType == NodeType.CloseTag)
                    {
                        foreach (DesignerNode t in child.Children)
                            if (t.NodeType == NodeType.TagName)
                            {
                                // colorize the closing tag name
                                classifications.Add(
                                    new ClassificationSpan(
                                        t.SnapshotSpan,
                                        classificationTypeRegistry.GetClassificationType(Constants.DJANGO_SELECTED_TAGNAME)
                                        )
                                    );
                                break;
                            }
                    }
                }
            }

            // create standard classifiers for tags
            nodeProvider.GetNodes(span, node => node.NodeType != NodeType.ParsingContext)
                .ForEach(
                node =>
                {
                    switch (node.NodeType) {
                        case NodeType.Marker:
                            classifications.Add(
                                new ClassificationSpan(
                                    node.SnapshotSpan,
                                    classificationTypeRegistry.GetClassificationType(Constants.MARKER_CLASSIFIER)
                                    ));
                            break;
                        case NodeType.CommentContext:
                            classifications.Add(
                                new ClassificationSpan(
                                    node.SnapshotSpan,
                                    classificationTypeRegistry.GetClassificationType(Constants.COMMENT_CLASSIFIER)
                                    ));
                            break;
                        default:
                            break;
                    };
                }
                        );

            return classifications;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

    }
}
