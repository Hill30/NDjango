using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NDjango.Interfaces;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.IO;

namespace ASPMVC2010SampleApplication
{
    [Name("render-partial")]
    public class RenderPartialTag : NDjango.Compatibility.SimpleTag
    {
        public RenderPartialTag()
            : base(false, "render-partial", 1)
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
                htmlHelper.RenderPartial(parms[0].ToString());
                string_writer.Flush();
            }
            finally
            {
                htmlHelper.ViewContext.Writer = writer;
            }

            return string_writer.ToString();
        }
    }
}