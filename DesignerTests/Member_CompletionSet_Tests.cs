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
        [TestFixtureSetUp]
        public void setup()
        {
            parser = TestHelper.InitializeParser();
        }

        ITemplateManager parser;

        //[Test]
        //public void Can_Create_Source()
        //{
        //    var broker = new Mock<INodeProviderBroker>().Object;
        //    var textBuffer = new Mock<ITextBuffer>().Object;
        //    var source = new Source(broker, textBuffer);
        //    Assert.IsInstanceOf<Source>(source);
        //}

        //[Test]
        //public void Create_CompletionSet_Empty()
        //{
        //    var broker = new Mock<INodeProviderBroker>().Object;
        //    var textBuffer = new Mock<ITextBuffer>().Object;
        //    var source = new Source(broker, textBuffer);
        //    Assert.IsInstanceOf<Source>(source);
            
        //    var session = new Mock<ICompletionSession>();
        //    var session_properties = new Mock<PropertyCollection>().Object;
        //    session.Setup<PropertyCollection>(s => s.Properties).Returns(session_properties);
            
        //    var completionSets = new List<CompletionSet>();

        //    source.AugmentCompletionSession(session.Object, completionSets);

        //    Assert.AreEqual(0, completionSets.Count);
        //}

        [Test]
        public void Create_CompletionSet_Variable()
        {

            // setting up the template content
            var content = "{% for i in enum %} { {% endfor %}";
            var template = "temp://" + content;
            
            // mock text document
            //var text_document = new Mock<ITextDocument>().Object;
            
            // mock buffer properties
            //var buffer_properties = new Mock<PropertyCollection>().Object;
            //buffer_properties.AddProperty(typeof(ITextDocument), text_document);

            var textBuffer = new Mock<ITextBuffer>();
            //textBuffer.Setup<PropertyCollection>(t => t.Properties).Returns(buffer_properties);

            var tracking_span = new Mock<ITrackingSpan>();
            tracking_span.Setup<ITextBuffer>(ts => ts.TextBuffer).Returns(textBuffer.Object);
            tracking_span.Setup<string>(ts => ts.GetText(It.IsAny<ITextSnapshot>())).Returns((ITextSnapshot SnapshotPoint) => "");

            // mock current snapshot - always return the content
            var current_snapshot = new Mock<ITextSnapshot>();
            current_snapshot.Setup<int>(s => s.Length).Returns(content.Length);
            current_snapshot.Setup<char[]>(s => s.ToCharArray(It.IsAny<int>(), It.IsAny<int>())).Returns((int pos, int len) => content.ToCharArray(pos, len));
            current_snapshot.Setup(s => s.GetText()).Returns(content);
            current_snapshot.Setup(s => s.CreateTrackingSpan(It.IsAny<int>(), It.IsAny<int>(), SpanTrackingMode.EdgeInclusive))
                .Returns((int start, int length, SpanTrackingMode tracking_mode) => tracking_span.Object);
            current_snapshot.Setup<ITextBuffer>(s => s.TextBuffer).Returns(textBuffer.Object);
            textBuffer.Setup<ITextSnapshot>(t => t.CurrentSnapshot).Returns(current_snapshot.Object);
            
            // mock snapshot lines - test templates are all single line
            var line = new Mock<ITextSnapshotLine>();
            var extent = new SnapshotSpan(current_snapshot.Object, 0, current_snapshot.Object.Length);
            line.Setup(l => l.LineNumber).Returns(0);
            line.Setup(l => l.Snapshot).Returns(current_snapshot.Object);
            line.Setup(l => l.Start).Returns(new SnapshotPoint(current_snapshot.Object, 0));
            line.Setup(l => l.End).Returns(new SnapshotPoint(current_snapshot.Object, current_snapshot.Object.Length));
            line.Setup(l => l.Extent).Returns(extent);

            // build a list of lines into snapshot
            var lines = new List<ITextSnapshotLine>();
            lines.Add(line.Object);
            current_snapshot.Setup(s => s.Lines).Returns(lines);
            current_snapshot.Setup(s => s.LineCount).Returns(lines.Count);
            current_snapshot.Setup(s => s.GetLineFromPosition(It.IsAny<int>())).Returns(line.Object);

            Assert.IsInstanceOf<ITemplateManager>(parser);

            // mock handler
            var handler = new Mock<IProjectHandler>();
            handler.Setup<ITextSnapshot>(h => h.GetSnapshot(It.IsAny<string>())).Returns((string t) => current_snapshot.Object);
            handler.Setup<Microsoft.FSharp.Collections.FSharpList<INodeImpl>>(h => h.ParseTemplate(It.IsAny<string>(),(NDjango.TypeResolver.ITypeResolver) It.IsAny<TestTypeResolver>()))
                .Returns((string t, NDjango.TypeResolver.ITypeResolver resolver) => parser.GetTemplate(template,resolver,new NDjango.TypeResolver.ModelDescriptor(new List<NDjango.TypeResolver.IDjangoType>())).Nodes);

            var provider = new NodeProvider(handler.Object, template, new TestTypeResolver());

            var parsing_completed = false;

            provider.NodesChanged += delegate(SnapshotSpan span) 
            {
                parsing_completed = true;
            };

            while (!parsing_completed)
                System.Threading.Thread.Sleep(1);
            
            var source = new Source(provider, textBuffer.Object);

            // mock session properties 
            var session_properties = new Mock<PropertyCollection>().Object;
            session_properties.AddProperty(typeof(CompletionContext), CompletionContext.Variable);

            // mock tracking point 
            var trigger_position = 21;
            var tracking_point = new Mock<ITrackingPoint>();
            tracking_point.Setup<SnapshotPoint>(t => t.GetPoint(It.IsAny<ITextSnapshot>())).Returns((ITextSnapshot snapshot) => new SnapshotPoint(snapshot, trigger_position));

            var session = new Mock<ICompletionSession>();
            session.Setup<PropertyCollection>(s => s.Properties).Returns(session_properties);
            session.Setup<ITrackingPoint>(s => s.GetTriggerPoint(It.IsAny<ITextBuffer>())).Returns(tracking_point.Object);

            var completionSets = new List<CompletionSet>();
            source.AugmentCompletionSession(session.Object, completionSets);

            Assert.AreEqual(1, completionSets.Count);
            var c = completionSets[0];
            Assert.AreEqual(2, c.Completions.Count);
            Assert.AreEqual("forloop", c.Completions[0].DisplayText);
            Assert.AreEqual("i", c.Completions[1].DisplayText); 
            
        }
    }
}
