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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                templateSource.Text = File.ReadAllText(openFileDialog1.FileName);
                parseToolStripMenuItem_Click(sender, e);
            }
        }

        private void parseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            templateTree.Nodes.Clear();
            Diagonstics.Clear();
            try
            {
                foreach (INode node in provider.GetTemplate("does not matter").Nodes)
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
            string text = node.NodeType.ToString();
            text += ": " + templateSource.Text.Substring(node.Position, node.Length);
            
            TreeNode tnode = new TreeNode(text);
            tnode.Tag = node;
            tnode.NodeFont = new Font(templateTree.Font, FontStyle.Underline);

            if (node.ErrorMessage.Severity > -1)
                tnode.Nodes.Add("(Error)" + node.ErrorMessage.Message);

            string vlist = "";
            foreach (string s in node.Values)
                vlist += s + ' ';
            if (!string.IsNullOrEmpty(vlist))
                tnode.Nodes.Add("(Values) = " + vlist);

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
