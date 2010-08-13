namespace NDjango.Designer.Commands
{
    partial class AddViewDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblViewName = new System.Windows.Forms.Label();
            this.tbViewName = new System.Windows.Forms.TextBox();
            this.lblViewModel = new System.Windows.Forms.Label();
            this.comboModel = new System.Windows.Forms.ComboBox();
            this.lblBaseTemplate = new System.Windows.Forms.Label();
            this.btnAdd = new System.Windows.Forms.Button();
            this.checkedListBlocks = new System.Windows.Forms.CheckedListBox();
            this.lblBlocks = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.comboBaseTemplate = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblViewName
            // 
            this.lblViewName.AutoSize = true;
            this.lblViewName.Location = new System.Drawing.Point(12, 9);
            this.lblViewName.Name = "lblViewName";
            this.lblViewName.Size = new System.Drawing.Size(64, 13);
            this.lblViewName.TabIndex = 0;
            this.lblViewName.Text = "View Name:";
            // 
            // tbViewName
            // 
            this.tbViewName.Location = new System.Drawing.Point(12, 25);
            this.tbViewName.Name = "tbViewName";
            this.tbViewName.Size = new System.Drawing.Size(267, 20);
            this.tbViewName.TabIndex = 1;
            // 
            // lblViewModel
            // 
            this.lblViewModel.AutoSize = true;
            this.lblViewModel.Location = new System.Drawing.Point(12, 10);
            this.lblViewModel.Name = "lblViewModel";
            this.lblViewModel.Size = new System.Drawing.Size(126, 13);
            this.lblViewModel.TabIndex = 2;
            this.lblViewModel.Text = "Select model for the view";
            // 
            // comboModel
            // 
            this.comboModel.FormattingEnabled = true;
            this.comboModel.Items.AddRange(new object[] {
            "none"});
            this.comboModel.Location = new System.Drawing.Point(12, 26);
            this.comboModel.Name = "comboModel";
            this.comboModel.Size = new System.Drawing.Size(267, 21);
            this.comboModel.TabIndex = 3;
            // 
            // lblBaseTemplate
            // 
            this.lblBaseTemplate.AutoSize = true;
            this.lblBaseTemplate.Location = new System.Drawing.Point(12, 3);
            this.lblBaseTemplate.Name = "lblBaseTemplate";
            this.lblBaseTemplate.Size = new System.Drawing.Size(133, 13);
            this.lblBaseTemplate.TabIndex = 4;
            this.lblBaseTemplate.Text = "Select  template to extend:";
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(34, 15);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(89, 28);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // checkedListBlocks
            // 
            this.checkedListBlocks.Dock = System.Windows.Forms.DockStyle.Left;
            this.checkedListBlocks.FormattingEnabled = true;
            this.checkedListBlocks.Location = new System.Drawing.Point(12, 0);
            this.checkedListBlocks.Name = "checkedListBlocks";
            this.checkedListBlocks.Size = new System.Drawing.Size(267, 156);
            this.checkedListBlocks.TabIndex = 8;
            this.checkedListBlocks.Enabled = false;
            // 
            // lblBlocks
            // 
            this.lblBlocks.AutoSize = true;
            this.lblBlocks.Location = new System.Drawing.Point(12, 3);
            this.lblBlocks.Name = "lblBlocks";
            this.lblBlocks.Size = new System.Drawing.Size(128, 13);
            this.lblBlocks.TabIndex = 9;
            this.lblBlocks.Text = "Select Blocks to override:";
            this.lblBlocks.Enabled = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(166, 15);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(95, 28);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // comboBaseTemplate
            // 
            this.comboBaseTemplate.FormattingEnabled = true;
            this.comboBaseTemplate.Items.AddRange(new object[] {
            "none"});
            this.comboBaseTemplate.Location = new System.Drawing.Point(12, 25);
            this.comboBaseTemplate.Name = "comboBaseTemplate";
            this.comboBaseTemplate.Size = new System.Drawing.Size(267, 21);
            this.comboBaseTemplate.TabIndex = 11;
            this.comboBaseTemplate.SelectedIndexChanged += new System.EventHandler(this.comboBaseTemplate_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblViewName);
            this.panel1.Controls.Add(this.tbViewName);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(294, 50);
            this.panel1.TabIndex = 12;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnAdd);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 338);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(294, 62);
            this.panel2.TabIndex = 13;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.comboModel);
            this.panel3.Controls.Add(this.lblViewModel);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 50);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(294, 57);
            this.panel3.TabIndex = 14;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.comboBaseTemplate);
            this.panel4.Controls.Add(this.lblBaseTemplate);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 107);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(294, 51);
            this.panel4.TabIndex = 15;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.panel7);
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 158);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(294, 180);
            this.panel5.TabIndex = 16;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.lblBlocks);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(294, 24);
            this.panel6.TabIndex = 9;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.checkedListBlocks);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel7.Location = new System.Drawing.Point(0, 24);
            this.panel7.Name = "panel7";
            this.panel7.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.panel7.Size = new System.Drawing.Size(294, 156);
            this.panel7.TabIndex = 10;
            // 
            // AddViewDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 400);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Location = new System.Drawing.Point(150, 150);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddViewDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Django View";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblViewName;
        private System.Windows.Forms.TextBox tbViewName;
        private System.Windows.Forms.Label lblViewModel;
        private System.Windows.Forms.ComboBox comboModel;
        private System.Windows.Forms.Label lblBaseTemplate;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.CheckedListBox checkedListBlocks;
        private System.Windows.Forms.Label lblBlocks;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox comboBaseTemplate;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel7;
    }
}