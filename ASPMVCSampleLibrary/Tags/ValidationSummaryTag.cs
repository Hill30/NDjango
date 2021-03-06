﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NDjango.Interfaces;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using NDjango.ASPMVC;

namespace ASPMVCSampleLibrary.Tags
{
    [NDjango.ParserNodes.Description("Django wrapper around HtmlHelper.ValidationSummary")]
    [Name("validation-summary")]
    public class ValidationSummaryTag : HtmlHelperTag
    {
        public ValidationSummaryTag()
            : base(false, 1)
        { }

        public override MvcHtmlString ProcessTag(HtmlHelper htmlHelper, IContext context, string content, object[] parms)
        {
            return htmlHelper.ValidationSummary(false, parms[0].ToString());
        }

    }
}