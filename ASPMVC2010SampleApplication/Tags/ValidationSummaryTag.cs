using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NDjango.Interfaces;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ASPMVC2010SampleApplication.Tags
{
    [Name("validation-summary")]
    public class ValidationSummaryTag : NDjango.Compatibility.SimpleTag
    {
        public ValidationSummaryTag()
            : base(false, 1)
        { }

        public override string ProcessTag(NDjango.Interfaces.IContext context, string content, object[] parms)
        {
            var htmlHelperOption = context.tryfind("Html");
            if (htmlHelperOption == null)
                return "";

            var htmlHelper = (HtmlHelper)htmlHelperOption.Value;

            var result = htmlHelper.ValidationSummary(false, parms[0].ToString());
            if (result == null)
                return "";
            else
                return result.ToHtmlString();
        }
    }
}