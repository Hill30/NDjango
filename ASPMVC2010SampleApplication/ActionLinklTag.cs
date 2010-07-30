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
            : base(false, "action-link", 3)
        { }

        public override string ProcessTag(NDjango.Interfaces.IContext context, string content, object[] parms)
        {
            var htmlHelper =  context.tryfind("Html");

            return ((HtmlHelper)htmlHelper.Value).ActionLink(parms[0].ToString(), parms[1].ToString(), parms[2].ToString()).ToHtmlString();
        }
    }
}