using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace NDjango.Designer.Parsing
{
    public class TemplateLoader: ITemplateLoader
    {
        Dictionary<string, BufferRecord> templates = new Dictionary<string, BufferRecord>();
        private string project_directory;

        public TemplateLoader(string project_directory)
        {
            this.project_directory = project_directory;
        }

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

        private string get_absolute_path(string path)
        {
            if (path.StartsWith((string)project_directory)) 
                return path;
            if (path.Length > 2 && (path.StartsWith("\"") && path.EndsWith("\"") || path.StartsWith("'") && path.EndsWith("'")))
                try
                {
                    return Path.Combine((string)project_directory, path.Substring(1, path.Length-2));
                }
                catch (Exception) // if it failed to combine - I do not want to deal with it
                {}
            return path;
        }

        #region ITemplateLoader Members

        public TextReader GetTemplate(string path)
        {
            if (path.StartsWith("temp://"))
            {
                path.Remove(0,7);
                byte[] byteArray = Encoding.ASCII.GetBytes(path); 
                MemoryStream stream = new MemoryStream( byteArray);
                return new StreamReader(stream);
            }
            path = get_absolute_path(path);
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
            if (path.StartsWith("temp://"))
                return false;
            path = get_absolute_path(path);
            BufferRecord record;
            if (templates.TryGetValue(path, out record) && record.Item1 != null)
                return record.IsUpdated;
            return File.GetLastWriteTime(path) > timestamp;
        }

        #endregion
    }
}
