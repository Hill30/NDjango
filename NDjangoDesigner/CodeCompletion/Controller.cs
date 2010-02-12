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
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NDjango.Interfaces;
using System.Runtime.InteropServices;
using NDjango.Designer.Parsing;
using NDjango.Designer.CodeCompletion.CompletionSets;

namespace NDjango.Designer.CodeCompletion
{
    /// <summary>
    /// Controls the flow of the codecompletion sessions for the given TextView
    /// </summary>
    class Controller : IIntellisenseController, IOleCommandTarget
    {
        private ITextBuffer buffer;
        private ITextView textView;
        private IWpfTextView WpfTextView;
        private ICompletionSession activeSession;
        private ControllerProvider provider;
        private int triggerPosition;
        private ITrackingSpan completionSpan;

        /// <summary>
        /// Given a text view creates a new instance of the code completion controller and subscribes 
        /// to the text view keyboard events
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="subjectBuffers"></param>
        /// <param name="subjectTextView"></param>
        /// <param name="context"></param>
        public Controller(ControllerProvider provider, ITextBuffer buffer, ITextView subjectTextView)
        {
            this.provider = provider;
            this.buffer = buffer;
            this.textView = subjectTextView;

            WpfTextView = subjectTextView as IWpfTextView;
            if (WpfTextView != null)
            {
                WpfTextView.VisualElement.KeyDown += new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                WpfTextView.VisualElement.KeyUp += new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
            }
        }
        
        /// <summary>
        /// Handles the key up event.
        /// The intellisense window is dismissed when one presses ESC key
        /// Pressing Enter key commits the session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualElement_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Make sure that this event happened on the same text view to which we're attached.
            ITextView textView = sender as ITextView;
            if (this.textView != textView)
                return;

            if (activeSession == null)
                return;

            switch (e.Key)
            {
                case Key.Space:
                case Key.Escape:
                    activeSession.Dismiss();
                    e.Handled = true;
                    return;

                case Key.Left:
                    if (textView.Caret.Position.BufferPosition.Position <= triggerPosition)
                        // we went too far to the left
                        activeSession.Dismiss();
                    return;

                case Key.Right:
                    if (textView.Caret.Position.BufferPosition.Position > 
                            triggerPosition + completionSpan.GetSpan(completionSpan.TextBuffer.CurrentSnapshot).Length)
                        // we went too far to the right
                        activeSession.Dismiss();
                    return;

                case Key.Enter:
                    if (this.activeSession.SelectedCompletionSet.SelectionStatus != null)
                        activeSession.Commit();
                    else
                        activeSession.Dismiss();
                    e.Handled = true;
                    return;

                default:
                    break;
            }
        }

        /// <summary>
        /// Triggers Statement completion when appropriate keys are pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualElement_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Make sure that this event happened on the same text view to which we're attached.
            ITextView textView = sender as ITextView;
            if (this.textView != textView)
                return;

            // if there is a session already leave it be
            if (activeSession != null)
                return;

            // determine which subject buffer is affected by looking at the caret position
            SnapshotPoint? caret = textView.Caret.Position.Point.GetPoint
                (textBuffer => buffer == textBuffer, PositionAffinity.Predecessor);

            // return if no suitable buffer found
            if (!caret.HasValue)
                return;

            SnapshotPoint caretPoint = caret.Value;

            var subjectBuffer = caretPoint.Snapshot.TextBuffer;

            CompletionContext completionContext = 
                AbstractCompletionSet.GetCompletionContext(e.Key, subjectBuffer, caretPoint.Position);

            // the invocation occurred in a subject buffer of interest to us
            triggerPosition = caretPoint.Position;
            ITrackingPoint triggerPoint = caretPoint.Snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);
            completionSpan = caretPoint.Snapshot.CreateTrackingSpan(caretPoint.Position, 0, SpanTrackingMode.EdgeInclusive);

            // attach filter to intercept the Enter key
            attachKeyboardFilter();

            // Create a completion session
            activeSession = provider.CompletionBroker.CreateCompletionSession(textView, triggerPoint, true);

            // Put the completion context and original (empty) completion span
            // on the session so that it can be used by the completion source
            activeSession.Properties.AddProperty(typeof(CompletionContext), completionContext);
            activeSession.Properties.AddProperty(typeof(Controller), completionSpan);

            // Attach to the session events
            activeSession.Dismissed += new System.EventHandler(OnActiveSessionDismissed);
            activeSession.Committed += new System.EventHandler(OnActiveSessionCommitted);

            // Start the completion session. The intellisense will be triggered.
            activeSession.Start();
        }

        void OnActiveSessionDismissed(object sender, System.EventArgs e)
        {
            detachKeyboardFilter();
            activeSession = null;
        }

        void OnActiveSessionCommitted(object sender, System.EventArgs e)
        {
            detachKeyboardFilter();
            var pos = activeSession.TextView.Caret.Position.BufferPosition;
            if (pos.Position > 1)
                if ((pos-1).GetChar() == '}' && ((pos-2).GetChar() == '}' || (pos-2).GetChar() == '%'))
                {
                    var textView = activeSession.TextView;
                    textView.Caret.MoveToPreviousCaretPosition();
                    textView.Caret.MoveToPreviousCaretPosition();
                    textView.Caret.MoveToPreviousCaretPosition();
                }
            activeSession = null;
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }

        public void Detach(Microsoft.VisualStudio.Text.Editor.ITextView textView)
        {
            detachKeyboardFilter();
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            WpfTextView = textView as IWpfTextView;
            if (WpfTextView != null)
            {
                WpfTextView.VisualElement.KeyDown -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                WpfTextView.VisualElement.KeyUp -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
                detachKeyboardFilter();
            }
        }

        private void attachKeyboardFilter()
        {
            ErrorHandler.ThrowOnFailure(provider.adaptersFactory.GetViewAdapter(textView).AddCommandFilter(this, out oldFilter));
        }

        private void detachKeyboardFilter()
        {
            ErrorHandler.ThrowOnFailure(provider.adaptersFactory.GetViewAdapter(textView).RemoveCommandFilter(this));
        }

        // The ugly COM code below is the heavy heritage from the "old style" editor integration
        // the reason we need it is that when the user presses Enter, this keypress in addition to being 
        // sent to our KeyUp event is also sent to the editor window itself. As a result, the window gets 
        // updated which causes the update events to fire which in turn causes the current selection
        // in the CompletionSet to be reset according to the matching rules. In other words, if the user 
        // selects something from the completion dropdown using arrows and presses enter, before the 
        // selection is applied to the code it is reset to whatever is considered to be a match to the 
        // letters keyed in before that.
        // The code below intercepts the ECMD_RETURN command before it is sent to the editor window. 
        private IOleCommandTarget oldFilter;

        private static readonly Guid CMDSETID_StandardCommandSet2k = new Guid("1496a755-94de-11d0-8c3f-00c04fc2aae2");
        private static readonly uint ECMD_RETURN = 3;

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == CMDSETID_StandardCommandSet2k && nCmdID == ECMD_RETURN)
                return VSConstants.S_OK;
            return oldFilter.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return oldFilter.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
