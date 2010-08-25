using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango;
using NDjango.Interfaces;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;
namespace DesignerTests
{
    public class TestTypeResolver : NDjango.TypeResolver.ITypeResolver
    {
        #region ITypeResolver Members

        public Type Resolve(string type_name)
        {
            return Type.GetType(type_name);
        }
        #endregion
    }
    public static class TestHelper
    {
        public static ITemplateManager InitializeParser()
        {

            List<Tag> tags = new List<Tag>();
            List<Filter> filters = new List<Filter>();
            TemplateManagerProvider provider = new TemplateManagerProvider();
            return provider
                    .WithTags(tags)
                    .WithFilters(filters)
                    .WithSetting(NDjango.Constants.EXCEPTION_IF_ERROR, false)
                    .GetNewManager();

        }
    }
    public class TextLine : ITextSnapshotLine
    {
        string text;
        ITextSnapshot snapshot;
        SnapshotSpan extent;
        public TextLine(string text,ITextSnapshot snapshot)
        {
            this.text = text;
            this.snapshot = snapshot;
            extent = new SnapshotSpan(snapshot, 0, snapshot.Length);
        }
        public SnapshotPoint End
        {
            get { throw new NotImplementedException(); }
        }

        public SnapshotPoint EndIncludingLineBreak
        {
            get { throw new NotImplementedException(); }
        }

        public SnapshotSpan Extent
        {
            get { throw new NotImplementedException(); }
        }

        public SnapshotSpan ExtentIncludingLineBreak
        {
            get { throw new NotImplementedException(); }
        }

        public string GetLineBreakText()
        {
            throw new NotImplementedException();
        }

        public string GetText()
        {
            return text;
        }

        public string GetTextIncludingLineBreak()
        {
            return text + "\r\n";
        }

        public int Length
        {
            get { return text.Length; }
        }

        public int LengthIncludingLineBreak
        {
            get { throw new NotImplementedException(); }
        }

        public int LineBreakLength
        {
            get { throw new NotImplementedException(); }
        }

        public int LineNumber
        {
            get { throw new NotImplementedException(); }
        }

        public ITextSnapshot Snapshot
        {
            get { return snapshot; }
        }

        public SnapshotPoint Start
        {
            get { throw new NotImplementedException(); }
        }
    }
}
