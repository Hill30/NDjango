using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NDjango.Interfaces;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ASPMVC2010SampleApplication
{
    [Name("action-link")]
    public class ActionLinklTag : NDjango.Compatibility.SimpleTag
    {
        public ActionLinklTag()
            : base(false, 3)
        { }

        public override string ProcessTag(NDjango.Interfaces.IContext context, string content, object[] parms)
        {
            var htmlHelperOption = context.tryfind("Html");
            if (htmlHelperOption == null)
                return "";

            var htmlHelper = (HtmlHelper)htmlHelperOption.Value;

            var result = htmlHelper.ActionLink(parms[0].ToString(), parms[1].ToString(), parms[2].ToString()).ToHtmlString();
            if (result == null)
                return "";
            else
                return result;
        }
    }
}