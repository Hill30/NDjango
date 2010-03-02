using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    internal class TemplateNameNode : DesignerNode
    {
        public TemplateNameNode(NodeProvider provider, DesignerNode parent, ITextSnapshot snapshot, INode node)
            : base(provider, parent, snapshot, node)
        { }

        protected override bool IsIntellisenseProvider
        {
            get
            {
                return true;
            }
        }

    }
}
