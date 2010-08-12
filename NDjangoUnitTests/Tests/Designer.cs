using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NDjango.UnitTests.Data;
using NDjango.Interfaces;
using System.Collections;
using NDjango.FiltersCS;

namespace NDjango.UnitTests
{
    public partial class Tests
    {
        List<TestDescriptor> tests = new List<TestDescriptor>();

        public void SetupStandartdValues()
        {
            provider = new TemplateManagerProvider()
                .WithLoader(new Loader())
                .WithTag("non-nested", new TestDescriptor.SimpleNonNestedTag())
                .WithTag("nested", new TestDescriptor.SimpleNestedTag())
                .WithTag("url", new TestUrlTag()).WithSetting(Constants.EXCEPTION_IF_ERROR, false);

            this.standardTags = ((IDictionary<string, ITag>)((ITemplateManagerProvider)provider).Tags).Keys;
            this.standardFilters = ((IDictionary<string, ISimpleFilter>)((ITemplateManagerProvider)provider).Filters).Keys;
        }
      

        [Test, TestCaseSource("GetDesignerTests")]
        public void DesignerTests(TestDescriptor test)
        {
            DesignerTest(test);
        }

        private void DesignerTest(TestDescriptor test)
        {
            test.Run(managerForDesigner);
        }

        private DesignerData[] Nodes(params DesignerData[] nodes) { return nodes; }

        private string[] EmptyList
        {
            get
            {
                return new string[] { };
            }
        }


        //[Test, TestCaseSource("GetBlockNamesTests")]
        //public void BlockNames(TestDescriptor test)
        //{
        //    test.AnalyzeBlockNameNode(managerForDesigner);
        //}
        
        //public IList<TestDescriptor> GetBlockNamesTests()
        //{
        //    IList<TestDescriptor> lst = new List<TestDescriptor>();
        //    //1. simple inheritance
        //    lst.Add(new TestDescriptor("BlockNamesTest 01", "{% extends \"t1\" %} skip1--{% block b1 %}the replacement{% endblock %}--skip2",null,new string[]{"b1"},""));
        //    //2. two-level hierarchy
        //    lst.Add(new TestDescriptor("BlockNamesTest 02", "{% extends \"t21\" %} skip1--{% block b1 %}the replacement1++{{ block.super }}++{% endblock %}--skip2", null, new string[]{"b1","b2"}));
        //    //3. base template with html tags
        //    lst.Add(new TestDescriptor("BlockNameTest_Std",
        //        "{% extends \"base\" %}{% block ValidateVars %}{% endblock %}",null,
        //        new string[]{"SubSub","Sub1","MainContent","Title"})); 


        //    return lst;
        //}

        public IList<TestDescriptor> GetDesignerTests()
        {
            SetupStandartdValues();


            /* IF BLOCK */
            NewTest("if-tag-designer", "{% if foo %}yes{% else %}no{% endif %}"
                , Nodes
                (
                    StandardNode(0, 38),
                    Node(25, 13, "endif"),
                    StandardNode(30, 5),
                    Node(12, 13, "else", "endif"),
                    StandardNode(18, 4),
                    StandardNode(3, 2)
                ));
            NewTest("if-tag-designer-error1", "{% if %}yes{% else %}no{% endif %}"
                , Nodes
                (
                    StandardNode(0, 34),
                    ErrorNode(0, 8, EmptyList, 2, "invalid conditional expression in 'if' tag"),
                    Node(8, 13, "else", "endif"),
                    StandardNode(14, 4),
                    Node(21, 13, "endif"),
                    StandardNode(26, 5),
                    StandardNode(3, 2)
                ));
            NewTest("if-tag-designer", "{% if foo or b or c %}yes{% else %}no{% endif %}"
                , Nodes
                (
                    StandardNode(0, 48),
                    Node(35, 13, "endif"),
                    StandardNode(40, 5),
                    Node(22, 13, "else", "endif"),
                    StandardNode(28, 4),
                    StandardNode(3, 2)
                ));
            NewTest("if-tag-designer-error2", "{% if foo or b and c %}yes{% else %}no{% endif %}"
                , Nodes
                (
                    StandardNode(0, 49),
                    ErrorNode(0, 27, EmptyList, 2, "'if' tags can't mix 'and' and 'or'"),
                    Node(23, 13, "else", "endif"),
                    StandardNode(29, 4),
                    Node(36, 13, "endif"),
                    StandardNode(41, 5),
                    StandardNode(3, 2)
                ));
            NewTest("if-tag-designer-error3", "{% if foo or b and c %}yes{% else %}no"
               , Nodes
               (
                   StandardNode(0, 23),
                   ErrorNode(0, 23, EmptyList, 2, "'if' tags can't mix 'and' and 'or'"),
                   Node(23, 13, "else", "endif"),
                   StandardNode(29, 4),
                   Node(36, 2, "endif"),
                   ErrorNode(0, 23, EmptyList, 2, "Missing closing tag. Available tags: endif"),
                   StandardNode(3, 2),
                   StandardNode(3, 2)
               ));
            NewTest("if-tag-designer-error4", "{% if foo or b or c %}yes{% else %}no"
               , Nodes
               (
                   StandardNode(0, 22),
                   Node(35, 2, "endif"),
                   ErrorNode(0, 23, EmptyList, 2, "Missing closing tag. Available tags: endif"),
                   StandardNode(3, 2),
                   Node(22, 13, "else", "endif"),
                   StandardNode(28, 4),
                   StandardNode(3, 2)
               ));
            /* END OF IF BLOCK /*

            /* FOR BLOCK */
            NewTest("for-tag-designer", "{% for a, b in c %}whatever{% empty %} {% endfor %}"
                , Nodes
                (
                    StandardNode(0, 51),
                    Node(19, 19, "empty", "endfor"),
                    StandardNode(30, 5),
                    Node(38, 13, "endfor"),
                    StandardNode(42, 6),
                    StandardNode(3, 3)
                ));
            NewTest("for-tag-designer2", "{% for a, b in c %}whatever{% endfor %}"
                , Nodes
                (
                    StandardNode(0, 39),
                    Node(19, 20, "empty", "endfor"),
                    StandardNode(30, 6),
                    StandardNode(3, 3)
                ));
            NewTest("for-tag-designer-error1", "{% for a, b in c %}whatever{% empty %}"
                , Nodes
                (
                    StandardNode(0, 19),
                    Node(19, 19, "empty", "endfor"),
                    StandardNode(30, 5),
                    Node(38, 0, "endfor"),
                    StandardNode(0, 19),
                    StandardNode(3, 3),
                    StandardNode(3, 3)
                ));
            NewTest("for-tag-designer-error2", "{% for a, b c %}whatever{% empty %}"
                , Nodes
                (
                    StandardNode(0, 16),
                    ErrorNode(0, 16, EmptyList, 2, "malformed 'for' tag"),
                    Node(16, 19, "empty", "endfor"),
                    StandardNode(27, 5),
                    Node(35, 0, "endfor"),
                    ErrorNode(0, 16, EmptyList, 2, "Missing closing tag. Available tags: endfor"),
                    StandardNode(3, 3),
                    StandardNode(3, 3)
                ));
            NewTest("for-tag-designer-error3", "{% for a, b c %}whatever {% endfor %}"
                , Nodes
                (
                    StandardNode(0, 37),
                    ErrorNode(0, 16, EmptyList, 2, "malformed 'for' tag"),
                    Node(16, 21, "empty", "endfor"),
                    StandardNode(28, 6),
                    StandardNode(3, 3)
                ));
            NewTest("for-tag-designer-error4", "{% for a, b in c d %}whatever{% endfor %}"
                , Nodes
                (
                    StandardNode(0, 41),
                    ErrorNode(0, 21, EmptyList, 2, "malformed 'for' tag"),
                    Node(21, 20, "empty", "endfor"),
                    StandardNode(32, 6),
                    StandardNode(3, 3)
                ));
            /* END OF FOR BLOCK */

            /* IFEQUAL */
            NewTest("ifequal-tag-designer", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}"
                , Nodes
                (
                    StandardNode(0, 52),
                    Node(34, 18, "endifequal"),
                    StandardNode(39, 10),
                    Node(21, 13, "else", "endifequal"),
                    StandardNode(27, 4),
                    StandardNode(3, 7)
                ));
            NewTest("ifequal-tag-designer-error1", "{% ifequal foo bar %}yes{% else %}no"
                , Nodes
                (
                    StandardNode(0, 21),
                    Node(34, 2, "endifequal"),
                    ErrorNode(0, 21, EmptyList, 2, "Missing closing tag. Available tags: endifequal"),
                    StandardNode(3, 7),
                    Node(21, 13, "else", "endifequal"),
                    StandardNode(27, 4),
                    StandardNode(3, 7)
                ));
            NewTest("ifequal-tag-designer-error2", "{% ifequal foo %}% endifequal %}"
                , Nodes
                (
                    StandardNode(0, 17),
                    ErrorNode(0, 17, EmptyList, 2, "'ifequal' takes two arguments"),
                    Node(17, 15, "else", "endifequal"),
                    ErrorNode(0, 17, EmptyList, 2, "Missing closing tag. Available tags: endifequal"),
                    StandardNode(3, 7),
                    StandardNode(3, 7)
                ));
            NewTest("ifequal-tag-designer-error3", "{% ifequal %}"
                , Nodes
                (
                    StandardNode(0, 13),
                    ErrorNode(0, 13, EmptyList, 2, "'ifequal' takes two arguments"),
                    Node(13, 0, "else", "endifequal"),
                    ErrorNode(0, 13, EmptyList, 2, "Missing closing tag. Available tags: endifequal"),
                    StandardNode(3, 7),
                    StandardNode(3, 7)
                ));
            NewTest("ifequal-tag-designer-error4", "{% ifequal foo bar %}yes{% mess %}no{% endifequal %}"
                , Nodes
                (
                    StandardNode(0, 52),
                    Node(21, 31, "else", "endifequal"),
                    ErrorNode(24, 10, EmptyList, 2, "Unknown tag: mess"),
                    StandardNode(27, 4),
                    StandardNode(39, 10),
                    StandardNode(3, 7)
                ));
            /* END OF IFEQUAL BLOCK */

            NewTest("cycle-tag-designer", "{% cycle a,b,c as abc %}{% cycle abc %}{% cycle abc %}"
                , Nodes
                (
                    StandardNode(0, 54),
                    StandardNode(3, 5),
                    StandardNode(27, 5),
                    StandardNode(42, 5)
                ));

            NewTest("with-tag-designer", "{% with var.otherclass.method as newvar %}{{ newvar }}{% endwith %}"
                , Nodes
                (
                    StandardNode(0, 67),
                    Node(42, 25, "endwith"),
                    StandardNode(57, 7),
                    StandardNode(3, 4)
                ));
            
            NewTest("ifequal-tag-designer", "{% ifequal foo bar %}yes{% else %}no{% endifequal %}"
                , Nodes 
                (
                    StandardNode(0, 52),
                    Node(34, 18, "endifequal"),
                    StandardNode(39, 10),
                    Node(21, 13, "else", "endifequal"),
                    StandardNode(27, 4),
                    StandardNode(3, 7)
                ));
            NewTest("autoescape-tag-designer-error", "{% autoescape %}whatever{% endautoescape %}"
                , Nodes
                (
                    StandardNode(0, 43),
                    ErrorNode(0, 16, EmptyList, 2, "invalid arguments for 'Autoescape' tag"),
                    Node(16, 27, "endautoescape"),
                    StandardNode(27, 13),
                    StandardNode(3, 10),
                    KeywordNode(14, 0, "on", "off")
                ));

            NewTest("model-tag-designer", "{% model Model:NDjango.UnitTests.TestModel %}{{ }}",
                Nodes
                (
                    StandardNode(0, 50),
                    StandardNode(3, 5),
                    ErrorNode(47, 1, EmptyList, 2, "Could not parse some characters | "),
                    VariableNode(47, 1, 2, "Variables and attributes may not be empty, begin with underscores or minus (-) signs: ' '",
                        "Standard", "ToString", "GetHashCode", "GetType", "Model", "MethodString", "MethodInt", "ToString", 
                        "GetHashCode", "GetType", "Field1", "Field2")
                ));
            NewTest("variables-standard-designer", "{{ }}",
                Nodes
                (
                    StandardNode(0, 5),
                    StandardNode(2, 1),
                    VariableNode(2, 1, 2, "Variables and attributes may not be empty, begin with underscores or minus (-) signs: ' '",
                        "Standard", "ToString", "GetHashCode", "GetType")
                ));
            //NewTest("add-filter-designer", "{{ value| add:\"2\" }}"
            //    , Nodes 
            //    (
            //        StandardFilter(9, 3)
            //    ));
            //NewTest("fortag-designer", "{% for i in test %}{% ifchanged %}nothing changed{%else%}same {% endifchanged %}{{ forloop.counter }},{% endfor %}"
            //    , Nodes 
            //    (
            //        Node(0, 0),
            //        Node(0, 19),
            //        Node(19, 0),
            //        Node(19, 15),
            //        Node(57, 5),
            //        Node(62, 18),
            //        Node(64, 13, AddToStandardList("endif")),
            //        Node(62, 2),
            //        Node(78, 2),
            //        Node(34, 15),
            //        Node(49, 8),
            //        Node(51, 4, AddToStandardList("else", "endif")),
            //        Node(49, 2),
            //        Node(55, 2),
            //        Node(21, 10, standardTags.ToArray()),
            //        Node(19, 2),
            //        Node(32, 2),
            //        Node(80, 0),
            //        Node(80, 21),
            //        Node(80, 2),
            //        Node(99, 2),
            //        Node(101, 1),
            //        Node(102, 12),
            //        Node(104, 7, standardTags.ToArray()),
            //        Node(102, 2),
            //        Node(112, 2),
            //        Node(2, 4, standardTags.ToArray()),
            //        Node(0, 2),
            //        Node(17, 2),
            //        Node(114, 0)
            //    ));

            return tests;
        }

        private DesignerData KeywordNode(int position, int length, params string[] keyWords)
        {
            return new DesignerData(position, length, keyWords, -1, String.Empty);
        }

        //The following 'standard' methods are for nodes, which have standard Values list without any additions.
        private DesignerData StandardFilter(int position, int length, int errorSeverity, string errorMessage)
        {
            return new DesignerData(position, length, standardFilters.ToArray(), errorSeverity, errorMessage);
        }

        private DesignerData StandardNode(int position, int length, int errorSeverity, string errorMessage)
        {
            return new DesignerData(position, length, standardTags.ToArray(), errorSeverity, errorMessage);
        }

        private DesignerData StandardFilter(int position, int length)
        {
            return new DesignerData(position, length, standardFilters.ToArray(), -1, String.Empty);
        }

        private DesignerData StandardNode(int position, int length)
        {
            return new DesignerData(position, length, standardTags.ToArray(), -1, String.Empty);
        }

        //method for some nodes with error message
        private DesignerData ErrorNode(int position, int length, string[] values, int errorSeverity, string errorMessage)
        {
            if (values.Length == 0)
                return new DesignerData(position, length, EmptyList, errorSeverity, errorMessage);
            else
                return new DesignerData(position, length, AddToStandardList(values), errorSeverity, errorMessage);
        }

        private DesignerData VariableNode(int position, int length, int severity, string errorMessage, params string[] values)
        {
            return new DesignerData(position, length, values, severity, errorMessage);
        }

        private DesignerData Node(int position, int length, params string[] values)
        {
            return ErrorNode(position, length, values, -1, String.Empty);
        }

        private void NewTest(string name, string template, DesignerData[] nodeList)
        {
            tests.Add(new TestDescriptor(name, template, nodeList.ToList<DesignerData>()));
        }

        private string[] AddToStandardList(params string[] tags)
        {
            List<string> result = new List<string>(standardTags);
            result.InsertRange(0, tags);
            return result.ToArray();
        }
    }

    public class TestTyperesolver : ITypeResolver
    {
        #region ITypeResolver Members

        public Type Resolve(string type_name)
        {
            return Type.GetType(type_name);
        }

        #endregion
    }

    public class EmptyClass { }

    //this class is required for model tests
    public class TestModel
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string MethodString() { return null; }
        public int MethodInt() { return 0; }
    }
}
