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

            var textBuffer = new Mock<ITextBuffer>();
            var buffer_properties = new Mock<PropertyCollection>().Object;
            var text_document = new Mock<ITextDocument>().Object;
            buffer_properties.AddProperty(typeof(ITextDocument), text_document);
            textBuffer.Setup<PropertyCollection>(t => t.Properties).Returns(buffer_properties);
            var current_snapshot = new Mock<ITextSnapshot>();
            current_snapshot.Setup<int>(s => s.Length).Returns(content.Length);
            current_snapshot.Setup<char[]>(s => s.ToCharArray(It.IsAny<int>(), It.IsAny<int>())).Returns((int pos, int len) => content.ToCharArray(pos, len));

            textBuffer.Setup<ITextSnapshot>(t => t.CurrentSnapshot).Returns(current_snapshot.Object);

            var broker = new NodeProviderBroker();

            var source = new Source(broker, textBuffer.Object);
            Assert.IsInstanceOf<Source>(source);

            var session = new Mock<ICompletionSession>();
            var session_properties = new Mock<PropertyCollection>().Object;
            session_properties.AddProperty(typeof(CompletionContext), CompletionContext.Variable);
            session.Setup<PropertyCollection>(s => s.Properties).Returns(session_properties);

            var tracking_point = new Mock<ITrackingPoint>();
            tracking_point.Setup<SnapshotPoint>(t => t.GetPoint(It.IsAny<ITextSnapshot>())).Returns((ITextSnapshot snapshot) => new SnapshotPoint(snapshot, 21));
            session.Setup<ITrackingPoint>(s => s.GetTriggerPoint(It.IsAny<ITextBuffer>())).Returns(tracking_point.Object);

            var completionSets = new List<CompletionSet>();

            //var node = broker.GetNodeProvider(textBuffer.Object).GetNodes(new SnapshotPoint(current_snapshot.Object, 21), n => true).FindLast(n => true);

            //Assert.IsNotNull(node);

            source.AugmentCompletionSession(session.Object, completionSets);

            Assert.AreEqual(1, completionSets.Count);
        }
    }
}
