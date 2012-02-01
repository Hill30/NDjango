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
            MinimumSize = Size; // minimum size is set to what is defined in designer
            WriteItemDirectly = true;
        }

        public void FillDialogControls(string viewsFolderName)
        {
            wizard.ViewsFolderName = viewsFolderName;
            FillDialogControls();
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
            get
            {
                string model = string.Empty;
                if(IsViewModel)
                    model = comboModel.SelectedItem.ToString();
                return model;
            }
        }

        public string TemplateToExtend
        {
            get
            {
                string template = string.Empty;
                if(IsInheritance)
                           template = comboBaseTemplate.SelectedItem.ToString();

                return template;
            }
        }

        public string ViewsFolderName
        {
            set { wizard.ViewsFolderName = value; }
        }

        // PRIVATE PROPERTIES

        public bool IsInheritance 
        {
            get
            {
                return (comboBaseTemplate.SelectedItem != null &&
                    String.CompareOrdinal(comboBaseTemplate.SelectedItem.ToString(), TEXT_NONE) != 0);
            } 
        }

        public bool IsViewModel 
        {
            get 
            { 
                return (comboModel.SelectedItem != null && 
                    String.CompareOrdinal(comboModel.SelectedItem.ToString(), TEXT_NONE) != 0); 
            } 
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

            DialogResult = DialogResult.OK;
            Close();
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
