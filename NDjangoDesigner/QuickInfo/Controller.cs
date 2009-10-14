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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Windows.Input;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.ApplicationModel.Environments;

namespace NDjango.Designer.QuickInfo
{
    /// <summary>
    /// Controls NDjango QuickInfo session to display the tag hints and diagnostic messages
    /// </summary>
    class Controller : IIntellisenseController
    {
        private ControllerProvider provider;
        private IList<ITextBuffer> subjectBuffers;
        private ITextView textView;
        private IQuickInfoSession activeSession;
        private IEnvironment context;

        /// <summary>
        /// Creates a new controller
        /// </summary>
        /// <param name="nodeProviderBroker"></param>
        /// <param name="subjectBuffers"></param>
        /// <param name="textView"></param>
        /// <param name="brokerMapService"></param>
        public Controller(ControllerProvider provider, IList<ITextBuffer> subjectBuffers, ITextView textView, IEnvironment context)
        {
            this.provider = provider;
            this.subjectBuffers = subjectBuffers;
            this.textView = textView;
            this.context = context;

            textView.MouseHover += new EventHandler<MouseHoverEventArgs>(textView_MouseHover);

        }

        /// <summary>
        /// Activates a new QuickInfo session in response to the MouseHover event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void textView_MouseHover(object sender, MouseHoverEventArgs e)
        {
            if (activeSession != null)
                activeSession.Dismiss();

            SnapshotPoint? point = e.TextPosition.GetPoint(
                textBuffer => 
                    (
                        subjectBuffers.Contains(textBuffer)
                        && provider.nodeProviderBroker.IsNDjango(textBuffer, context)
                        && provider.brokerMapService.GetBrokerForTextView(textView, textBuffer) != null
                                // only text buffers require expilicit session activation
                                // XML and HTML already have quickInfo session activation code
                                // adding our own would cause 'double vision' - our source would be hit
                                // by our session as well as by the standard one
                        && textBuffer.ContentType.TypeName == "text"
                    )
                ,PositionAffinity.Predecessor);
            
            if (point.HasValue)
            {
                IQuickInfoBroker broker = provider.brokerMapService.GetBrokerForTextView(textView, point.Value.Snapshot.TextBuffer);
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);

                activeSession = broker.CreateQuickInfoSession(triggerPoint, true);
                activeSession.Start();
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }

        public void Detach(ITextView textView)
        { }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }
    }
}
