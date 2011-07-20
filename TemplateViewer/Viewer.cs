using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NDjango;
using System.IO;
using NDjango.Interfaces;

namespace TemplateViewer
{
    public partial class Viewer : Form
    {
        ITemplateManager provider;
        public Viewer()
        {
            InitializeComponent();
            provider = new TemplateManagerProvider()
                .WithSetting(Constants.EXCEPTION_IF_ERROR, false)
                .WithLoader(new TemplateLoader(templateSource))
                .GetNewManager();
        }

        class TemplateLoader : ITemplateLoader
        {
            RichTextBox source;
            public TemplateLoader(RichTextBox source)
            {
                this.source = source;
            }

            #region ITemplateLoader Members

            public TextReader GetTemplate(string path)
            {
                return new StringReader(source.Text);
            }

            public bool IsUpdated(string path, DateTime timestamp)
            {
                return true;
            }

            #endregion
        }

        public class TypeResolver : NDjango.TypeResolver.ITypeResolver
        {
            #region ITypeResolver Members

            public Type Resolve(string type_name)
            {
                return Type.GetType(type_name);
            }

            #endregion
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                templateSource.Text = File.ReadAllText(openFileDialog1.FileName);
                parseToolStripMenuItem_Click(sender, e);
            }
        }

        public class EmptyClass
        {
            public String HereY { get; set; }
        }

        private void parseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            templateTree.Nodes.Clear();
            Diagonstics.Clear();
            try
            {
                foreach (INode node in provider
                        .GetTemplate(
                            "does not matter",
                            new TypeResolver(),
                            new NDjango.TypeResolver.ModelDescriptor(
                                new NDjango.TypeResolver.IDjangoType[] 
                                {
                                    new NDjango.TypeResolver.CLRTypeDjangoType("Standard", typeof(EmptyClass))
                                })
                        ).Nodes)
                    Process(templateTree.Nodes, node);
            }
            catch (Exception ex)
            {
                Diagonstics.AppendText(
                    "Exception: " + ex.Message + "\n"
                    + ex.StackTrace
                    );
            }
        }

        private void Process(TreeNodeCollection treeNodeCollection, INode node)
        {
            string text = node.GetType().Name;
            switch (node.NodeType)
            {
                case NodeType.Text:
                    text = node.NodeType.ToString();
                    break;
                default:
                    break;
            }
            if (typeof(NDjango.ParserNodes.ErrorNode).IsAssignableFrom(node.GetType()))
                text = "ErrorNode";
            text += ": " + templateSource.Text.Substring(node.Position, node.Length);
            
            TreeNode tnode = new TreeNode(text);
            tnode.Tag = node;
            tnode.NodeFont = new Font(templateTree.Font, FontStyle.Underline);

            if (node.ErrorMessage.Severity > -1)
                tnode.Nodes.Add("(Error)" + node.ErrorMessage.Message);

            string vlist = "";
            var completion_provider = node as ICompletionValuesProvider;
            if (completion_provider != null)
                foreach (string s in completion_provider.Values)
                    vlist += s + ' ';

            if (!string.IsNullOrEmpty(vlist))
                tnode.Nodes.Add("(Values) = " + vlist);

            var mlist = "";
            foreach (var member in node.Context.Model.Members)
                mlist += member.Name + " ";

            if (!string.IsNullOrEmpty(mlist))
                tnode.Nodes.Add("(Members) = " + mlist);

            foreach (KeyValuePair<string, IEnumerable<INode>> item in node.Nodes)
            {
                TreeNode list = new TreeNode(item.Key);
                tnode.Nodes.Add(list);
                foreach (INode child in item.Value)
                    Process(list.Nodes, child);
            }
            treeNodeCollection.Add(tnode);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(openFileDialog1.FileName))
                saveAsToolStripMenuItem_Click(sender, e);
            else
                File.WriteAllText(openFileDialog1.FileName, templateSource.Text);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                File.WriteAllText(saveFileDialog1.FileName, templateSource.Text);
        }

        int selpos = 0;
        int sellen = 0;
        private void templateTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (sellen > 0)
            {
                templateSource.Select(selpos, sellen);
                templateSource.SelectionBackColor = Color.White;
            }
            selpos = 0;
            sellen = 0;

            INode node = e.Node.Tag as INode;
            nodeInfo.Text = "";
            if (node != null)
            {
                selpos = node.Position;
                sellen = node.Length;
                templateSource.Select(selpos, sellen);
                templateSource.SelectionBackColor = Color.Wheat;
                nodeInfo.Text = String.Format("Node: Pos = {0}, Length = {1}", node.Position, node.Length);
            }
        }

    }
}
