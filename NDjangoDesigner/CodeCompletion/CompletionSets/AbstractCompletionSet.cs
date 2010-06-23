using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace NDjango.Designer.CodeCompletion.CompletionSets
{
    /// <summary>
    /// Collection of the values to choose from in the code completion dialog
    /// </summary>
    abstract class AbstractCompletionSet : Microsoft.VisualStudio.Language.Intellisense.CompletionSet
    {

        internal static CompletionSet Create<T>
            (Source source, NodeProvider nodeProvider, SnapshotPoint point, Predicate<DesignerNode> filter)
            where T : AbstractCompletionSet, new()
        {
            // Get a list of all nodes of template name type
            var node = nodeProvider.GetNodes(point, filter).FindLast(n => true);
            if (node == null)
                return null;
            var result = new T();
            result.Initialize(source, node, point);
            return result;
       }

        internal static CompletionSet Create<T>
            (Source source, SnapshotPoint point, DesignerNode node)
            where T : AbstractCompletionSet, new()
        {
            if (node == null)
                return null;
            var result = new T();
            result.Initialize(source, node, point);
            return result;
        }

        /// <summary>
        /// Span tracking filter to be applied to the value list as the user types.
        /// Starts at the beginning of the word and ends at the left most position of the user input
        /// </summary>
        private ITrackingSpan filterSpan;
        private DesignerNode node;
        private CompletionList completions;
        private List<Completion> nodeCompletions;
        private List<Completion> completionBuilders;

        /// <summary>
        /// Completion set constructor - only called from the Create method
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        internal AbstractCompletionSet(Source source, DesignerNode node, SnapshotPoint point)
            : base("Django Completions", "Django Completions", null, null, null)
        {
            Initialize(source, node, point);
        }

        protected AbstractCompletionSet()
            : base("Django Completions", "Django Completions", null, null, null) { }

        protected Source Source { get; private set; }

        protected virtual void Initialize(Source source, DesignerNode node, SnapshotPoint point)
        {
            Source = source;

            // calculate the span to be replaced with user selection
            Span span = new Span(point.Position, 0);
            if (node.SnapshotSpan.IntersectsWith(span))
                span = node.SnapshotSpan.Span;
            ApplicableTo = point.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);

            // claculate the filter span (for explanation see comment on the filterspan member definition)
            filterSpan = point.Snapshot.CreateTrackingSpan(span.Start, point.Position - span.Start, SpanTrackingMode.EdgeInclusive); 

            this.node = node;
        }

        /// <summary>
        /// The node this completion set is associated with
        /// </summary>
        protected DesignerNode Node { get { return node; } }

        /// <summary>
        /// Starting position of the part of the input string to be used to filter the completions
        /// </summary>
        protected virtual int FilterOffset { get { return 0; } }

        /// <summary>
        /// Builds a list of completions out if the list of values
        /// </summary>
        /// <param name="values">The list of values</param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        /// <remarks>All values in the list are prepended with the prefix and appended with the suffix</remarks>
        protected IEnumerable<Completion> BuildCompletions(IEnumerable<string> values, string prefix, string suffix)
        {
            foreach (string value in values)
                yield return new Completion(value, prefix + value + suffix, null, null, null);
        }

        /// <summary>
        /// Supplies the list of completions for the node. Called only once the first time
        /// the list is accessed
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<Completion> BuildNodeCompletions();

        /// <summary>
        /// Supplies the list of completion builders for the node. Called only once the first time
        /// the list is accessed
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<Completion> BuildNodeCompletionBuilders()
        {
            return new List<Completion>();
        }

        /// <summary>
        /// Calculates the prefix used to filter and position the completion list
        /// </summary>
        /// <returns></returns>
        protected virtual string getPrefix()
        {
            var prefix = filterSpan.GetText(filterSpan.TextBuffer.CurrentSnapshot);
            if (prefix.Length > FilterOffset)
                return prefix.Substring(FilterOffset);
            return prefix;
        }

        /// <summary>
        /// Selects the best match to what the user typed
        /// </summary>
        /// <remarks>
        /// The steps to determine the best match:
        /// 1. Check if there is a precise match in the builders list
        /// 2. If not - check if there is a precise match in the completions list
        /// 3. If not - look up the closest match in the completions list
        /// </remarks>
        public override void SelectBestMatch()
        {
            string prefix = getPrefix();

            // precise match to a completion builder
            Completion completion = CompletionBuilders.FirstOrDefault(c => c.DisplayText.CompareTo(prefix) == 0);

            // if none - precise match to a completion 
            if (completion == null)
                completion = Completions.FirstOrDefault(c => c.DisplayText.CompareTo(prefix) == 0);

            // if none - position the completion list
            if (completion == null)
                completion = Completions.FirstOrDefault(c => c.DisplayText.CompareTo(prefix) >= 0);

            if (completion != null)
                SelectionStatus = new CompletionSelectionStatus(completion,
                    completion.DisplayText == prefix,
                    true
                    );
        }

        class CompletionList : List<Completion>, INotifyCollectionChanged
        {
            bool isModified = false;
            public void RaiseCollectionChanged()
            {
                if (!isModified)
                    return;

                isModified = false;
                if (CollectionChanged != null)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            new public void Insert(int index, Completion item)
            {
                base.Insert(index, item);
                isModified = true;
            }

            new public void RemoveAt(int index)
            {
                base.RemoveAt(index);
                isModified = true;
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
        }

        public override sealed IList<Completion> Completions
        {
            get
            {
                if (completions == null)
                {
                    nodeCompletions = new List<Completion>(BuildNodeCompletions());
                    completions = new CompletionList();
                }

                string prefix = getPrefix();
                int cPos = 0;
                foreach (var c in nodeCompletions)
                {
                    if (to_be_included(prefix, c))
                        if (cPos < completions.Count && completions[cPos] == c)
                            cPos++;
                        else
                            completions.Insert(cPos++, c);
                    else
                        if (cPos < completions.Count && completions[cPos] == c)
                            completions.RemoveAt(cPos);
                }
                completions.RaiseCollectionChanged();
                return completions;
            }
        }

        private bool to_be_included(string prefix, Completion c)
        {
            if (prefix.Length < 2)
                return true;
            return c.DisplayText.StartsWith(prefix.Substring(0, prefix.Length - 1));
        }

        public override sealed IList<Completion> CompletionBuilders
        {
            get
            {
                if (completionBuilders == null)
                    completionBuilders = new List<Completion>(BuildNodeCompletionBuilders());
                return completionBuilders;
            }
        }

        internal class Win32
        {
            public static string CharsOfKey(Key key)
            {
                uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);
                byte[] keyState = new byte[256];
                Win32.GetKeyboardState(keyState);
                uint scancode = Win32.MapVirtualKey(vk, (uint)Win32.MapType.MAPVK_VK_TO_VSC);
                char[] buffer = new char[10];
                int count = Win32.ToUnicode(vk, scancode, keyState, buffer, buffer.Length, 0);
                if (count < 0)
                    count = 0;
                char[] result = new char[count];
                Array.Copy(buffer, result, count);
                return new string(result);
            }

            /// <summary>The set of valid MapTypes used in MapVirtualKey
            /// </summary>
            /// <remarks></remarks>
            public enum MapType : uint
            {
                /// <summary>uCode is a virtual-key code and is translated into a scan code.
                /// If it is a virtual-key code that does not distinguish between left- and
                /// right-hand keys, the left-hand scan code is returned.
                /// If there is no translation, the function returns 0.
                /// </summary>
                /// <remarks></remarks>
                MAPVK_VK_TO_VSC = 0x0,

                /// <summary>uCode is a scan code and is translated into a virtual-key code that
                /// does not distinguish between left- and right-hand keys. If there is no
                /// translation, the function returns 0.
                /// </summary>
                /// <remarks></remarks>
                MAPVK_VSC_TO_VK = 0x1,

                /// <summary>uCode is a virtual-key code and is translated into an unshifted
                /// character value in the low-order word of the return value. Dead keys (diacritics)
                /// are indicated by setting the top bit of the return value. If there is no
                /// translation, the function returns 0.
                /// </summary>
                /// <remarks></remarks>
                MAPVK_VK_TO_CHAR = 0x2,

                /// <summary>Windows NT/2000/XP: uCode is a scan code and is translated into a
                /// virtual-key code that distinguishes between left- and right-hand keys. If
                /// there is no translation, the function returns 0.
                /// </summary>
                /// <remarks></remarks>
                MAPVK_VSC_TO_VK_EX = 0x3,

                /// <summary>Not currently documented
                /// </summary>
                /// <remarks></remarks>
                MAPVK_VK_TO_VSC_EX = 0x4,
            }

            [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern uint MapVirtualKey(uint uCode, uint uMapType);

            [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern bool GetKeyboardState(byte[] keyState);

            [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int
                ToUnicode(
                    uint virtKey,
                    uint scanCode,
                    byte[] keyState,
                    char[] resultBuffer,
                    int bufSize,
                    int flags
                );
        }

        public static CompletionContext GetCompletionContext(Key key, ITextBuffer buffer, int position)
        {
            string triggerChars = Win32.CharsOfKey(key);

            // return if the key pressed is not a character key
            if (triggerChars == "")
                return CompletionContext.None;

            switch (triggerChars[0])
            {
                case '%':
                    if (position > 0 && buffer.CurrentSnapshot[position - 1] == '{')
                        // it is start of a new tag
                        return CompletionContext.Tag;
                    else
                        // if it is not we can ignore it
                        return CompletionContext.None;

                case '{':
                    if (position > 0 && buffer.CurrentSnapshot[position - 1] == '{')
                        // it is start of a new variable 
                        return CompletionContext.Variable;
                    else
                        // if it is not we can ignore it
                        return CompletionContext.None;

                case '|':
                    return CompletionContext.FilterName;

                case '\'':
                    return CompletionContext.AposString;

                case '"':
                    return CompletionContext.QuotedString;

                case '.':
                    return CompletionContext.Reference;

                default:
                    if (Char.IsLetterOrDigit(triggerChars[0]))
                        return CompletionContext.Word;
                    return CompletionContext.None;
            }

        }

    }

    /// <summary>
    /// A list of various contexts a list of completions can be requested from
    /// </summary>
    public enum CompletionContext
    {
        /// <summary>
        /// A new tag context - triggered if a '%' is entered right after '{'
        /// </summary>
        Tag,

        /// <summary>
        /// A filter name context - triggered by '|'
        /// </summary>
        FilterName,

        /// <summary>
        /// A new variable context - triggered if a '{' is entered right after '{'
        /// </summary>
        Variable,

        /// <summary>
        /// Word is a context covering typing inside a word - a tag name, a filter name a keyword, etc
        /// </summary>
        Word,

        Reference,

        QuotedString,

        AposString,

        /// <summary>
        /// This is not a recognized code completion context
        /// </summary>
        None
    }
}
