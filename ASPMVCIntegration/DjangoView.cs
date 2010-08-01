using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NDjango.Interfaces;

namespace NDjango.ASPMVC
{
    class DjangoView : IView, IViewDataContainer
    {
        private ITemplateManager iTemplateManager;
        private string viewPath;

        public DjangoView(NDjango.Interfaces.ITemplateManager iTemplateManager, string viewPath)
        {
            this.iTemplateManager = iTemplateManager;
            this.viewPath = viewPath;
        }

        ViewDataDictionary data_dictionary;

        #region IView Members

        public void Render(ViewContext viewContext, System.IO.TextWriter writer)
        {
            data_dictionary = viewContext.ViewData;

            var requestContext = new Dictionary<string, object>();
            requestContext.Add("Html", new HtmlHelper(viewContext, this));
            requestContext.Add("Model", viewContext.ViewData);
            requestContext.Add("Session", viewContext.HttpContext.Session);

            var reader = iTemplateManager.RenderTemplate(viewPath, requestContext);
            var buffer = new char[4096];
            int count = 0;
            while ((count = reader.Read(buffer, 0, buffer.Length)) > 0)
                writer.Write(buffer, 0, count);
        }

        #endregion

        #region IViewDataContainer Members

        public ViewDataDictionary ViewData
        {
            get
            {
                return data_dictionary;
            }
            set
            {
                data_dictionary = value;
            }
        }

        #endregion
    }
}
