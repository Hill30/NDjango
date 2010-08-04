using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NDjango.Interfaces;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.IO;

namespace ASPMVCSampleLibrary
{
    [NDjango.ParserNodes.Description("Django wrapper around HtmlHelper.BeginForm")]
    [Name("form")]
    public class FormTag : NDjango.Compatibility.SimpleTag
    {
        public FormTag()
            : base(true, 0)
        { }

        public override string ProcessTag(NDjango.Interfaces.IContext context, string content, object[] parms)
        {
            var htmlHelperOption = context.tryfind("Html");
            if (htmlHelperOption == null)
                return "";

            var htmlHelper = (HtmlHelper)htmlHelperOption.Value;

            var string_writer = new StringWriter();
            var writer = htmlHelper.ViewContext.Writer;
            try
            {
                htmlHelper.ViewContext.Writer = string_writer;
                var form = htmlHelper.BeginForm();
                string_writer.Flush();
                var start = string_writer.ToString();

                string_writer = new StringWriter();
                htmlHelper.ViewContext.Writer = string_writer;
                form.EndForm();
                string_writer.Flush();
                var end = string_writer.ToString();

                return start + content + end;
            }
            finally
            {
                htmlHelper.ViewContext.Writer = writer;
            }

        }
    }
}