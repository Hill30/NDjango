using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
namespace NDjango.Designer.Commands
{
    public partial class AddViewDlg : Form
    {
        ViewWizard wizard = new ViewWizard();
        StringBuilder templateMem = new StringBuilder();
        const string TEXT_NONE = "None";


        public AddViewDlg()
        {
            InitializeComponent();
            this.MinimumSize = this.Size; // minimum size is set to what is defined in designer
            WriteItemDirectly = true;
        }

        public void FillDialogControls()
        {
            wizard.Update();
            ViewName = wizard.GenerateName();
            FillModelList();
            FillAllTemplates();
            comboBaseTemplate.SelectedIndex = 0;//none value
            comboModel.SelectedIndex = 0;//none value

        }

        // PUBLIC PROPERTIES

        /// <summary>
        /// Always contains the pre-generated template text
        /// </summary>
        public string PreGeneratedTemplateText { get; set; }

        /// <summary>
        /// If set tu true then this form will directly generate the file with the specified name.
        /// Otherwise this form will only set the pre-generated text to the "PreGeneratedTemplateText" field and return.
        /// To ontain only pre-generated text without writing it to the file - set this value to false before showing the dialog
        /// </summary>
        public bool WriteItemDirectly { get; set; }

        /// <summary>
        /// Get or set the shown view name
        /// </summary>
        public string ViewName 
        {
            get { return tbViewName.Text; }
            set { tbViewName.Text = value; }
        }

        public bool ViewNameEnabled
        {
            get { return tbViewName.Enabled; }
            set { tbViewName.Enabled = value; }
        }

        public string SelectedModel
        {
            get { return (comboModel.SelectedItem == null || comboModel.SelectedItem.ToString() == "None") ? 
                string.Empty : 
                comboBaseTemplate.SelectedItem.ToString(); }
        }

        public string ModelToExtend
        {
            get { return (comboModel.SelectedItem == null || comboModel.SelectedItem.ToString() == "None") ? 
                string.Empty : 
                comboModel.SelectedItem.ToString(); }
        }

        // PRIVATE PROPERTIES

        private bool IsInheritance 
        {
            get { return (comboBaseTemplate.SelectedItem != null && comboBaseTemplate.SelectedItem.ToString() != TEXT_NONE); } 
        }

        private bool IsViewModel 
        {
            get { return comboModel.SelectedItem != null && comboModel.SelectedItem.ToString() != TEXT_NONE; } 
        }

        // METHODS

        private void FillModelList()
        {
            comboModel.Items.Clear();
            comboModel.Items.Add(TEXT_NONE);
            try
            {
                List<Assembly> assmlist = wizard.GetReferences();
                foreach (Assembly assm in assmlist)
                {
                    var types= assm.GetExportedTypes();
                    foreach (Type t in types)
                        comboModel.Items.Add(t.FullName);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.WriteLine(ex);
            }

        }
        
        private void PopRecentTemplates()
        {
            int i = 0;
            foreach (string item in wizard.Recent5Templates)
            {
                comboBaseTemplate.Items.Insert(i++, item);
            }
        }
        
        private void FillAllTemplates()
        {
            comboBaseTemplate.Items.Clear();
            comboBaseTemplate.Items.Add(TEXT_NONE);
            IEnumerable<string> allTemplates = wizard.GetTemplates("");
            foreach (string item in allTemplates)
                //if (!comboBaseTemplate.Items.Contains(item))
                    comboBaseTemplate.Items.Add(item);
        }
        
        private void ModifyTemplate()
        {
            templateMem.Clear();
            templateMem.AppendLine("temp://{% extends \"" + comboBaseTemplate.SelectedItem + "\" %}");
            templateMem.AppendLine("{% block A %}");
            templateMem.AppendLine("{% endblock %}");
        }
        
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ViewName == string.Empty)
            {
                Misc.otherStuff.ShowErrorDialog("Error", "View Name can not be empty.");
                return;
            }

            if (comboBaseTemplate.SelectedItem != null)
                wizard.RegisterInserted(comboBaseTemplate.SelectedItem.ToString());

            string itemName = ViewName + ".django";            
            string templateFile = Path.GetTempFileName();
            string PreGeneratedTemplateText = string.Empty;

            // We need to generate these tags only if we're writing directly to the file. otherwise this will be generated on the fly
            if (IsViewModel && WriteItemDirectly)
                PreGeneratedTemplateText += "{% model Model:" + comboModel.SelectedItem + " %}\r\n";
            
            // We need to generate these tags only if we're writing directly to the file. otherwise this will be generated on the fly
            if (IsInheritance && WriteItemDirectly)
            {
                PreGeneratedTemplateText += "{% extends \"" + comboBaseTemplate.SelectedItem + "\" %}\r\n";
                if (checkedListBlocks.CheckedItems.Count > 0)
                {
                    foreach (string name in checkedListBlocks.CheckedItems)
                    {
                        PreGeneratedTemplateText += "{% block " + name + " %}\r\n";
                        PreGeneratedTemplateText += "{% endblock " + name + " %}\r\n";
                    }
                }
            }

            if (WriteItemDirectly)
            {
                using (StreamWriter sw = new StreamWriter(templateFile))
                {
                    sw.WriteLine(PreGeneratedTemplateText);
                    sw.Close();
                }

                try
                {
                    wizard.AddFromFile(templateFile, itemName);
                    this.Close();
                }
                catch (COMException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    File.Delete(templateFile);
                }
            }
            else
                this.Close();
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void comboBaseTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsInheritance)
            {
                ModifyTemplate();
                List<string> blockNames = wizard.GetTemplateBlocks(templateMem.ToString());
                checkedListBlocks.Items.Clear();
                foreach (string item in blockNames)
                    checkedListBlocks.Items.Add(item);

            }

            lblBlocks.Enabled = checkedListBlocks.Enabled = IsInheritance && checkedListBlocks.Items.Count > 0;
        }

    }
}
