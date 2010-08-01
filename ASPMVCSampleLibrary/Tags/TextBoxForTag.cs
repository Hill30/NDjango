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
    [NDjango.ParserNodes.Description("Django wrapper around HtmlHelper.TextBoxFor")]
    [Name("textbox-for")]
    public class TextBoxForlTag : HtmlHelperTag
    {
        public TextBoxForlTag()
            : base(false, 1)
        { }

        public override MvcHtmlString ProcessTag(HtmlHelper htmlHelper, IContext context, string content, object[] parms)
        {
            return htmlHelper.TextBox(parms[0].ToString());
        }

    }
}