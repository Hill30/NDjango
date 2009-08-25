using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NDjango.UnitTests.Data;
using NDjango.FiltersCS;
using System.Diagnostics;
using NUnit.Framework;
using NDjango.Interfaces;

namespace NDjango.UnitTests
{
    public struct DesignerData
    {
        public DesignerData(int position, int length, string[] values, int severity, string errorMessage)
        {
            this.Position = position;
            this.Length = length;
            this.Values = values;
            this.Severity = severity;
            this.ErrorMessage = errorMessage;
        }

        public int Position;
        public int Length;
        public string[] Values;
        public int Severity;
        public string ErrorMessage;
    }

    public class TestDescriptor
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public object[] ContextValues { get; set; }
        public object[] Result { get; set; }
        public List<DesignerData> ResultForDesigner { get; set; }
        public string[] Vars { get; set; }
        ResultGetter resultGetter;

        public override string ToString()
        {
            return Name;
        }

        public TestDescriptor(string name, string template, object[] values, object[] result, List<DesignerData> designResult, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            Result = result;
            Vars = vars;
            ResultForDesigner = designResult;
        }

        public TestDescriptor(string name, string template, object[] values, object[] result, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            Result = result;
            Vars = vars;
        }

        public delegate object[] ResultGetter();

        public TestDescriptor(string name, string template, object[] values, ResultGetter resultGetter, params string[] vars)
        {
            Name = name;
            Template = template;
            ContextValues = values;
            this.resultGetter = resultGetter;
            Vars = vars;
        }

        public static string runTemplate(NDjango.Interfaces.ITemplateManager manager, string templateName, IDictionary<string,object> context)
        {
            var template = manager.RenderTemplate(templateName, context);
            string retStr = template.ReadToEnd();
            return retStr;
        }

		public class SimpleNonNestedTag : NDjango.Compatibility.SimpleTag
        {
            public SimpleNonNestedTag() : base(false, "non-nested", 2) { }

            public override string ProcessTag(NDjango.Interfaces.IContext context, string contents, object[] parms)
            {
                StringBuilder res = new StringBuilder();
                foreach (object o in parms)
                    res.Append(o);

                return res
                    .Append(contents)
                    .ToString();
            }
        }

        public class SimpleNestedTag : NDjango.Compatibility.SimpleTag
        {
            public SimpleNestedTag() : base(true, "nested", 2) { }

            public override string ProcessTag(NDjango.Interfaces.IContext context, string contents, object[] parms)
            {
                StringBuilder res = new StringBuilder();
                foreach (object o in parms)
                    res.Append(o);

                return res
                    .Append("start")
                    .Append(contents)
                    .Append("end")
                    .ToString();
            }
        }

        public void Run(NDjango.Interfaces.ITemplateManager manager)
        {
            if (ResultForDesigner != null)
            {
                ValidateDesigner(manager);
                return;
            }

            //ITemplate template = manager.GetTemplate(Template);

            //List<INode> stuff = getNodes(manager.GetTemplate(Template).Nodes.ToList<INodeImpl>().ConvertAll(node => (INode)node));

            var context = new Dictionary<string, object>();

            if (ContextValues != null)
                for (int i = 0; i <= ContextValues.Length - 2; i += 2)
                    context.Add(ContextValues[i].ToString(), ContextValues[i + 1]);

            try
            {
                if (resultGetter != null)
                    Result = resultGetter();
                
                Assert.AreEqual(Result[0], runTemplate(manager, Template, context), "** Invalid rendering result");
                //if (Vars.Length != 0)
                //    Assert.AreEqual(Vars, manager.GetTemplateVariables(Template), "** Invalid variable list");
            }
            catch (Exception ex)
            {
                // Result[0] is either some object, in which case this shouldn't have happened
                // or it's the type of the exception the calling code expects.
                if (resultGetter != null)
                    Result = resultGetter();
                Assert.AreEqual(Result[0], ex.GetType(), "Exception: " + ex.Message);
            }
        }

        private void ValidateDesigner(NDjango.Interfaces.ITemplateManager manager)
        {
            ITemplate template = manager.GetTemplate(Template);
            
            //the same logic responsible for retriving nodes as in NodeProvider class (DjangoDesigner).
            List<INode> nodes = getNodes(template.Nodes.ToList<INodeImpl>().ConvertAll(node => (INode)node));
            List<DesignerData> actualResult = nodes.ConvertAll(
                node => new DesignerData(node.Position, node.Length, new List<string>(node.Values).ToArray(), node.ErrorMessage.Severity, node.ErrorMessage.Message));

            for (int i = 0; i < nodes.Count; i++)
            {
                Assert.AreEqual(actualResult[i].Length, ResultForDesigner[i].Length, "Invalid Length");
                Assert.AreEqual(actualResult[i].Position, ResultForDesigner[i].Position, "Invalid Position");
                Assert.AreEqual(actualResult[i].Severity, ResultForDesigner[i].Severity, "Invalid Severity");
                Assert.AreEqual(actualResult[i].ErrorMessage, ResultForDesigner[i].ErrorMessage, "Invalid ErrorMessage");
                Assert.AreEqual(actualResult[i].Values, ResultForDesigner[i].Values, "Invalid Values");
            }            
        }

        //the same logic responsible for retriving nodes as in NodeProvider class (DjangoDesigner).
        public static List<INode> getNodes(IEnumerable<INode> nodes)
        {
            List<INode> result = new List<INode>();

            foreach (INode ancestor in nodes)
	        {
                result.Add(ancestor);
                foreach (IEnumerable<INode> list in ancestor.Nodes.Values)
                {
                    result.AddRange(getNodes(list));
                }
	        }
            return result;
        }

        //the same list as in Defaults.standardTags
        public static string[] standartValues = new string[]
        { 
            "autoescape",
            "block",
            "comment",
            "cycle",
            "debug",
            "extends",
            "filter",
            "firstof",
            "for",
            "if",
            "ifchanged",
            "ifequal",
            "ifnotequal",
            "include",
            "nested",
            "non-nested",
            "now",
            "regroup",
            "spaceless",
            "ssi",
            "templatetag",
            "url",
            "widthratio",
            "with"
        };
    }

}
