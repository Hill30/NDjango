using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NDjango.Interfaces;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using NDjango.ASPMVC;

namespace ASPMVC2010SampleApplication
{
    [NDjango.ParserNodes.Description("Django wrapper around HtmlHelper.ActionLink")]
    [Name("action-link")]
    public class ActionLinklTag : HtmlHelperTag
    {
        public ActionLinklTag()
            : base(false, 3)
        { }

        public override MvcHtmlString ProcessTag(HtmlHelper htmlHelper, IContext context, string content, object[] parms)
        {
            return htmlHelper.ActionLink(parms[0].ToString(), parms[1].ToString(), parms[2].ToString());
        }

    }
}