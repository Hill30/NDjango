using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NDjango.UnitTests.Data;
using NDjango.FiltersCS;
using System.Diagnostics;
using NUnit.Framework;


namespace NDjango.UnitTests
{
    public class TestDescriptor
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public object[] ContextValues { get; set; }
        public object[] Result { get; set; }
        public string[] Vars { get; set; }
        ResultGetter resultGetter;

        public override string ToString()
        {
            return Name;
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

    }

}
