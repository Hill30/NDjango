using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using NDjango.Interfaces;
using System.Web.Mvc.Html;

namespace NDjango.ASPMVC
{
    [NDjango.ParserNodes.Description("Returns an URL matching given view with its parameters")]
    [Name("url")]
    public class AspMvcUrlTag : NDjango.Tags.Abstract.UrlTag
    {
        public override string GenerateUrl(string pathTemplate, string[] parameters, NDjango.Interfaces.IContext context)
        {
            var htmlHelperOption = context.tryfind("Html");
            if (htmlHelperOption == null)
                return "";

            var htmlHelper = (HtmlHelper)htmlHelperOption.Value;
            var anchor = LinkExtensions.ActionLink(htmlHelper, pathTemplate, pathTemplate, parameters[0]).ToHtmlString();

            //retriving relativePath from anchor element.
            var startindex = anchor.IndexOf("href") + 6; var endIndex = anchor.IndexOf("\"", startindex);
            return anchor.Substring(startindex, endIndex - startindex);
        }
    }
}
