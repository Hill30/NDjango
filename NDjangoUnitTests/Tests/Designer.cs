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
        [Test, TestCaseSource("GetDesignerTests")]
        public void DesignerTests(TestDescriptor test)
        {
            InternalFilterProcess(test);
        }

        public IList<TestDescriptor> GetDesignerTests()
        {
            IList<TestDescriptor> lst = new List<TestDescriptor>();
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
            lst.Add(new TestDescriptor("add-filter-designer", "{{ value| add:\"2\" }}", null, null
                , new List<DesignerData>() 
                {
                    //position, length, values, severity, errorMessage
                    new DesignerData(0, 0, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(0, 20, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(0, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(18, 2, new List<string>().ToArray(), -1, String.Empty),
                    new DesignerData(20, 0, new List<string>().ToArray(), -1, String.Empty)
                }
                , null));


            return lst;
        }
    }
}
