using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace DesignerTests
{
    using NDjango.Designer.Parsing;
    using NDjango.Designer.CodeCompletion;
    using Microsoft.VisualStudio.Utilities;
    using NDjango.Designer.CodeCompletion.CompletionSets;
    using NDjango.Interfaces;

    [TestFixture]
    public class Member_CompletionSet_Tests
    {
        [Test]
        public void Can_Create_Source()
        {
            var broker = new Mock<INodeProviderBroker>().Object;
            var textBuffer = new Mock<ITextBuffer>().Object;
            var source = new Source(broker, textBuffer);
            Assert.IsInstanceOf<Source>(source);
        }

        [Test]
        public void Create_CompletionSet_Empty()
        {
            var broker = new Mock<INodeProviderBroker>().Object;
            var textBuffer = new Mock<ITextBuffer>().Object;
            var source = new Source(broker, textBuffer);
            Assert.IsInstanceOf<Source>(source);
            
            var session = new Mock<ICompletionSession>();
            var session_properties = new Mock<PropertyCollection>().Object;
            session.Setup<PropertyCollection>(s => s.Properties).Returns(session_properties);
            
            var completionSets = new List<CompletionSet>();

            source.AugmentCompletionSession(session.Object, completionSets);

            Assert.AreEqual(0, completionSets.Count);
        }

        [Test]
        public void Create_CompletionSet_Variable()
        {

            var content = "{% for i in enum %} { {% endfor %}";
            var template = "temp://" + content;
            var textBuffer = new Mock<ITextBuffer>();
            var buffer_properties = new Mock<PropertyCollection>().Object;
            var text_document = new Mock<ITextDocument>().Object;
            var broker = new Mock<INodeProviderBroker>().Object;
            buffer_properties.AddProperty(typeof(ITextDocument), text_document);
            textBuffer.Setup<PropertyCollection>(t => t.Properties).Returns(buffer_properties);
            var current_snapshot = new Mock<ITextSnapshot>();
            current_snapshot.Setup<int>(s => s.Length).Returns(content.Length);
            current_snapshot.Setup<char[]>(s => s.ToCharArray(It.IsAny<int>(), It.IsAny<int>())).Returns((int pos, int len) => content.ToCharArray(pos, len));
            current_snapshot.Setup<ITextBuffer>(s => s.TextBuffer).Returns(textBuffer.Object);
            textBuffer.Setup<ITextSnapshot>(t => t.CurrentSnapshot).Returns(current_snapshot.Object);
            var line = new Mock<ITextSnapshotLine>();
            var extent = new SnapshotSpan(current_snapshot.Object,0,current_snapshot.Object.Length);
            line.Setup(l => l.LineNumber).Returns(0);
            line.Setup(l => l.Snapshot).Returns(current_snapshot.Object);
            line.Setup(l => l.Start).Returns(new SnapshotPoint(current_snapshot.Object, 0));
            line.Setup(l => l.End).Returns(new SnapshotPoint(current_snapshot.Object, current_snapshot.Object.Length));
            line.Setup(l => l.Extent).Returns(extent);
            var lines = new List<ITextSnapshotLine>();
            lines.Add(line.Object);
            current_snapshot.Setup(s => s.Lines).Returns(lines);
            current_snapshot.Setup(s => s.LineCount).Returns(lines.Count);
            current_snapshot.Setup(s => s.GetLineFromPosition(It.IsAny<int>())).Returns(line.Object);
            current_snapshot.Setup(s => s.GetText()).Returns(content);
            var handler = new Mock<IHandler>();
            var parser = TestHelper.InitializeParser();
            Assert.IsInstanceOf<ITemplateManager>(parser);
            handler.Setup<ITextSnapshot>(h => h.GetSnapshot(It.IsAny<string>())).Returns((string t) => current_snapshot.Object);
            handler.Setup<Microsoft.FSharp.Collections.FSharpList<INodeImpl>>(h => h.ParseTemplate(It.IsAny<string>(),(NDjango.TypeResolver.ITypeResolver) It.IsAny<TestTypeResolver>()))
                .Returns((string t, NDjango.TypeResolver.ITypeResolver resolver) => parser.GetTemplate(template,resolver,new NDjango.TypeResolver.ModelDescriptor(new List<NDjango.TypeResolver.IDjangoType>())).Nodes);

            var provider = new NodeProvider(handler.Object, template, new TestTypeResolver());
            //as rebuildNodes runs in a separate thread - we need to be sure it has finished parsing the template the first time
            System.Threading.Thread.Sleep(1000);
            var source = new Source(provider, textBuffer.Object);
            Assert.IsInstanceOf<Source>(source);

            var session = new Mock<ICompletionSession>();
            var session_properties = new Mock<PropertyCollection>().Object;
            session_properties.AddProperty(typeof(CompletionContext), CompletionContext.Variable);
            session.Setup<PropertyCollection>(s => s.Properties).Returns(session_properties);

            var tracking_point = new Mock<ITrackingPoint>();
            tracking_point.Setup<SnapshotPoint>(t => t.GetPoint(It.IsAny<ITextSnapshot>())).Returns((ITextSnapshot snapshot) => new SnapshotPoint(snapshot, 21));
            session.Setup<ITrackingPoint>(s => s.GetTriggerPoint(It.IsAny<ITextBuffer>())).Returns(tracking_point.Object);

            var node = provider.GetNodes(new SnapshotPoint(current_snapshot.Object, 21), n => true).FindLast(n => true);
            Assert.IsNotNull(node);

            var completionSets = new List<CompletionSet>();
            source.AugmentCompletionSession(session.Object, completionSets);

            Assert.AreEqual(1, completionSets.Count);
            var c = completionSets[0];
            Assert.AreEqual(3, c.Completions.Count);
            
        }
    }
}
