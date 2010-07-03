using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text;
using System.IO;

namespace NDjango.Designer.Parsing
{
    class TemplateLoader: ITemplateLoader
    {
        Dictionary<string, BufferRecord> templates = new Dictionary<string, BufferRecord>();

        class BufferRecord : Tuple<ITextSnapshot>
        {
            public BufferRecord(ITextSnapshot snapshot, bool is_dirty)
                : base(snapshot)
            {
                snapshot.TextBuffer.Changed += new EventHandler<TextContentChangedEventArgs>(buffer_Changed);
                IsUpdated = is_dirty;
            }

            void buffer_Changed(object sender, TextContentChangedEventArgs e)
            {
                IsUpdated = true;
            }

            public bool IsUpdated { get; private set; }

        }

        internal void Register(string path, ITextBuffer buffer, NodeProvider provider)
        {
            templates[path] = new BufferRecord(buffer.CurrentSnapshot, true);
        }

        internal void Unregister(string path)
        {
            templates.Remove(path);
        }


        /// <summary>
        /// TextReader wrapper around text in the buffer
        /// </summary>
        class BufferReader : TextReader
        {
            ITextSnapshot snapshot;
            int pos = 0;
            public BufferReader(ITextSnapshot snapshot)
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

        class DummyReader : TextReader
        {
            public override int Read()
            {
                return -1;
            }
        }

        public ITextSnapshot GetSnapshot(string path)
        {
            BufferRecord record;
            if (templates.TryGetValue(path, out record) && record.Item1 != null)
                return record.Item1;
            else
                return null;
        }

        #region ITemplateLoader Members

        public TextReader GetTemplate(string path)
        {
            BufferRecord record;
            if (templates.TryGetValue(path, out record) && record.Item1 != null)
            {
                var new_record = new BufferRecord(record.Item1.TextBuffer.CurrentSnapshot, false);
                templates[path] = new_record;
                return new BufferReader(new_record.Item1);
            }
            if (File.Exists(path))
                return new StreamReader(path);
            return new DummyReader();
        }

        public bool IsUpdated(string path, DateTime timestamp)
        {
            BufferRecord record;
            if (templates.TryGetValue(path, out record) && record.Item1 != null)
                return record.IsUpdated;
            return File.GetLastWriteTime(path) > timestamp;
        }

        #endregion
    }
}
