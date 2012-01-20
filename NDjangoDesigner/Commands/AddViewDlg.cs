﻿using System;
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
        public AddViewDlg()
        {
            InitializeComponent();
            this.MinimumSize = this.Size; // minimum size is set to what is defined in designer
        }

        public void FillDialogControls()
        {
            wizard.Update();
            tbViewName.Text = wizard.GenerateName();
            FillModelList();
            FillAllTemplates();
            comboBaseTemplate.SelectedIndex = 0;//none value
            comboModel.SelectedIndex = 0;//none value

        }
        
        private void FillModelList()
        {
            comboModel.Items.Clear();
            comboModel.Items.Add("None");
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
            comboBaseTemplate.Items.Add("None");
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
            string itemName = tbViewName.Text + ".django";
            wizard.RegisterInserted(comboBaseTemplate.SelectedItem.ToString());
            string templateFile = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(templateFile);
            if (IsViewModel)
                sw.WriteLine("{% model Model:" + comboModel.SelectedItem + " %}");
            if (IsInheritance)
            {
                sw.WriteLine("{% extends \"" + comboBaseTemplate.SelectedItem + "\" %}");
                if (checkedListBlocks.CheckedItems.Count > 0)
                {
                    foreach (string name in checkedListBlocks.CheckedItems)
                    {
                        sw.WriteLine("{% block " + name + " %}");
                        sw.WriteLine("{% endblock " + name + " %}");
                    }
                }
            }
            sw.Close();
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool IsInheritance { get { return (comboBaseTemplate.SelectedItem != null && comboBaseTemplate.SelectedItem.ToString() != "None"); } }
        private bool IsViewModel { get { return comboModel.SelectedItem != null && comboModel.SelectedItem.ToString() != "None"; } }
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

        private void comboModel_KeyDown(object sender, KeyEventArgs e)
        {
            // Suppress overwriting of selected text, allow only changing selected item
            if (e.KeyCode != Keys.Up && e.KeyCode != Keys.Down)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void comboBaseTemplate_KeyDown(object sender, KeyEventArgs e)
        {
            // Suppress overwriting of selected text, allow only changing selected item
            if (e.KeyCode != Keys.Up && e.KeyCode != Keys.Down)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }


    }
}
