using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace NDjango.Designer.Parsing
{
    public interface ITemplateManager
    {
    }

    [Export(typeof(ITemplateManager))]
    public class TemplateManager : ITemplateManager
    {
        public TemplateManager() { }
    }
}
