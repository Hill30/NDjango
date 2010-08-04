using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NDjango.Interfaces;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using NDjango.ASPMVC;

namespace ASPMVCSampleLibrary
{
    [NDjango.ParserNodes.Description("Django wrapper around HtmlHelper.PasswordFor")]
    [Name("password-for")]
    public class PasswordForTag : HtmlHelperTag
    {
        public PasswordForTag()
            : base(false, 1)
        { }

        public override MvcHtmlString ProcessTag(HtmlHelper htmlHelper, IContext context, string content, object[] parms)
        {
            return htmlHelper.Password(parms[0].ToString());
        }

    }
}