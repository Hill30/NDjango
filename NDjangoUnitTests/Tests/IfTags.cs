using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NDjango.UnitTests.Data;

namespace NDjango.UnitTests
{
    public partial class Tests
    {

        [Test, TestCaseSource("GetIfTagTests")]
        public void IfTag(TestDescriptor test)
        {
            test.Run(manager);
        }

        public IList<TestDescriptor> GetIfTagTests()
        {

            IList<TestDescriptor> lst = new List<TestDescriptor>();

            // ### IF TAG ################################################################
            //designer test
            lst.Add(new TestDescriptor("if-tag-designer", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), null
                , new List<DesignerData>() 
                {
                    //position, length, values, severity, errorMessage
                    new DesignerData(0, 0, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(0, 12, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(25, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(27, 11, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(29, 6, TestDescriptor.standartValues.ToArray(), -1, String.Empty),
                    new DesignerData(27, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(36, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(12, 3, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(15, 10, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(17, 5, TestDescriptor.standartValues.ToArray(), -1, String.Empty),
                    new DesignerData(15, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(23, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(2, 3, TestDescriptor.standartValues.ToArray(), -1, String.Empty),
                    new DesignerData(0, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(10, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(38, 0, new List<string>().ToArray(), -1, String.Empty)
                } 
                , null));
            
            lst.Add(new TestDescriptor("if-tag01", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag02", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag02-1", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p("foo", null), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag03", "{% if foo %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("no")));

            // AND
            lst.Add(new TestDescriptor("if-tag-and01", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-and02", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and03", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and04", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and05", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and06", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and07", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-and08", "{% if foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("bar", true), ContextObjects.p("no")));

            // OR
            lst.Add(new TestDescriptor("if-tag-or01", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-or02", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-or03", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-or04", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-or05", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-or06", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-or07", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-or08", "{% if foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("bar", true), ContextObjects.p("yes")));

            // TODO, multiple ORs

            // NOT
            lst.Add(new TestDescriptor("if-tag-not01", "{% if not foo %}no{% else %}yes{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not02", "{% if not %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not03", "{% if not %}yes{% else %}no{% endif %}", ContextObjects.p("not", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not04", "{% if not not %}no{% else %}yes{% endif %}", ContextObjects.p("not", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not05", "{% if not not %}no{% else %}yes{% endif %}", ContextObjects.p(), ContextObjects.p("no")));

            lst.Add(new TestDescriptor("if-tag-not06", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not07", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not08", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not09", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not10", "{% if foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));

            lst.Add(new TestDescriptor("if-tag-not11", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not12", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not13", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not14", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not15", "{% if not foo and bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("no")));

            lst.Add(new TestDescriptor("if-tag-not16", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not17", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not18", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not19", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not20", "{% if foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("yes")));

            lst.Add(new TestDescriptor("if-tag-not21", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not22", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not23", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not24", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not25", "{% if not foo or bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("yes")));

            lst.Add(new TestDescriptor("if-tag-not26", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not27", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not28", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not29", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not30", "{% if not foo and not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("yes")));

            lst.Add(new TestDescriptor("if-tag-not31", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p(), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not32", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("if-tag-not33", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not34", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("if-tag-not35", "{% if not foo or not bar %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p("yes")));

            // AND and OR raises a TemplateSyntaxError
            lst.Add(new TestDescriptor("if-tag-error01", "{% if foo or bar and baz %}yes{% else %}no{% endif %}", ContextObjects.p("foo", false, "bar", false), ContextObjects.p(typeof(Lexer.SyntaxErrorException))));
            lst.Add(new TestDescriptor("if-tag-error02", "{% if foo and %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p(typeof(Lexer.SyntaxErrorException))));
            lst.Add(new TestDescriptor("if-tag-error03", "{% if foo or %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p(typeof(Lexer.SyntaxErrorException))));
            lst.Add(new TestDescriptor("if-tag-error04", "{% if not foo and %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p(typeof(Lexer.SyntaxErrorException))));
            lst.Add(new TestDescriptor("if-tag-error05", "{% if not foo or %}yes{% else %}no{% endif %}", ContextObjects.p("foo", true), ContextObjects.p(typeof(Lexer.SyntaxErrorException))));
            lst.Add(new TestDescriptor("if-tag-error06", "{% if not foo %}yes{% else %}no", ContextObjects.p("foo", true), ContextObjects.p(typeof(Lexer.SyntaxErrorException))));
            
            // IFEqual TAG
            lst.Add(new TestDescriptor("ifequal-tag-designer", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", true, "bar", true), null
                , new List<DesignerData>() 
                {
                    //position, length, values, severity, errorMessage
                    new DesignerData(0, 0, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(0, 21, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(34, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(36, 16, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(38, 11, TestDescriptor.standartValues.ToArray(), -1, String.Empty),
                    new DesignerData(36, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(50, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(21, 3, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(24, 10, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(26, 5, TestDescriptor.standartValues.ToArray(), -1, String.Empty),
                    new DesignerData(24, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(32, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(2, 8, TestDescriptor.standartValues.ToArray(), -1, String.Empty),
                    new DesignerData(0, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(19, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(52, 0, new List<string>().ToArray(), -1, String.Empty)
                }
                , null));
            lst.Add(new TestDescriptor("ifequal-tag-01", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("ifequal-tag-02", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("ifequal-tag-03", "{% ifnotequal foo bar %}yes{% else %}no{% endifnotequal %}", ContextObjects.p("foo", true, "bar", true), ContextObjects.p("no")));
            lst.Add(new TestDescriptor("ifequal-tag-04", "{% ifnotequal foo bar %}yes{% else %}no{% endifnotequal %}", ContextObjects.p("foo", true, "bar", false), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("ifequal-tag-05", "{% ifequal foo \"true\" %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", "true"), ContextObjects.p("yes")));
            lst.Add(new TestDescriptor("ifequal-tag-06", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}", ContextObjects.p("foo", "true", "bar", false), ContextObjects.p("no")));

            return lst;
        }
    }
}
