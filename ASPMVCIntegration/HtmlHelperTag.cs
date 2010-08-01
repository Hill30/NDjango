using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Compatibility;
using System.Web.Mvc;

namespace NDjango.ASPMVC
{
    public abstract class HtmlHelperTag : SimpleTag
    {
        public HtmlHelperTag(bool nested, int num_params)
            : base(nested, num_params)
        { }

        public abstract MvcHtmlString ProcessTag(HtmlHelper htmlHelper, NDjango.Interfaces.IContext context, string content, object[] parms);

        public override string ProcessTag(NDjango.Interfaces.IContext context, string content, object[] parms)
        {
            var htmlHelperOption = context.tryfind("Html");
            if (htmlHelperOption == null)
                return "";

            var htmlHelper = (HtmlHelper)htmlHelperOption.Value;

            var metadata_provider = new DataAnnotationsModelMetadataProvider();

            if (context.ModelType == null)
                htmlHelper.ViewData.ModelMetadata = null;
            else
            {
                var model = Activator.CreateInstance(context.ModelType.Value);
                htmlHelper.ViewData.ModelMetadata = metadata_provider.GetMetadataForType(() => model, context.ModelType.Value);
            }

            var result = ProcessTag(htmlHelper, context, content, parms);
            if (result == null)
                return "";
            else
                return result.ToHtmlString();
        }
    
    }
}
