namespace NewViewGenerator
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
            this.SuspendLayout();
            // 
            // lblViewName
            // 
            this.lblViewName.AutoSize = true;
            this.lblViewName.Location = new System.Drawing.Point(18, 9);
            this.lblViewName.Name = "lblViewName";
            this.lblViewName.Size = new System.Drawing.Size(64, 13);
            this.lblViewName.TabIndex = 0;
            this.lblViewName.Text = "View Name:";
            // 
            // tbViewName
            // 
            this.tbViewName.Location = new System.Drawing.Point(18, 26);
            this.tbViewName.Name = "tbViewName";
            this.tbViewName.Size = new System.Drawing.Size(267, 20);
            this.tbViewName.TabIndex = 1;
            // 
            // lblViewModel
            // 
            this.lblViewModel.AutoSize = true;
            this.lblViewModel.Location = new System.Drawing.Point(18, 53);
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
            this.comboModel.Location = new System.Drawing.Point(18, 70);
            this.comboModel.Name = "comboModel";
            this.comboModel.Size = new System.Drawing.Size(267, 21);
            this.comboModel.TabIndex = 3;
            // 
            // lblBaseTemplate
            // 
            this.lblBaseTemplate.AutoSize = true;
            this.lblBaseTemplate.Location = new System.Drawing.Point(18, 106);
            this.lblBaseTemplate.Name = "lblBaseTemplate";
            this.lblBaseTemplate.Size = new System.Drawing.Size(133, 13);
            this.lblBaseTemplate.TabIndex = 4;
            this.lblBaseTemplate.Text = "Select  template to extend:";
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(86, 319);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(89, 28);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // checkedListBlocks
            // 
            this.checkedListBlocks.FormattingEnabled = true;
            this.checkedListBlocks.Location = new System.Drawing.Point(18, 190);
            this.checkedListBlocks.Name = "checkedListBlocks";
            this.checkedListBlocks.Size = new System.Drawing.Size(166, 79);
            this.checkedListBlocks.TabIndex = 8;
            this.checkedListBlocks.Visible = false;
            // 
            // lblBlocks
            // 
            this.lblBlocks.AutoSize = true;
            this.lblBlocks.Location = new System.Drawing.Point(18, 156);
            this.lblBlocks.Name = "lblBlocks";
            this.lblBlocks.Size = new System.Drawing.Size(134, 13);
            this.lblBlocks.TabIndex = 9;
            this.lblBlocks.Text = "Choose Blocks to override:";
            this.lblBlocks.Visible = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(205, 319);
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
            this.comboBaseTemplate.Location = new System.Drawing.Point(18, 123);
            this.comboBaseTemplate.Name = "comboBaseTemplate";
            this.comboBaseTemplate.Size = new System.Drawing.Size(267, 21);
            this.comboBaseTemplate.TabIndex = 11;
            this.comboBaseTemplate.SelectedIndexChanged += new System.EventHandler(this.comboBaseTemplate_SelectedIndexChanged);
            // 
            // AddViewDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(312, 359);
            this.Controls.Add(this.comboBaseTemplate);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblBlocks);
            this.Controls.Add(this.checkedListBlocks);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lblBaseTemplate);
            this.Controls.Add(this.comboModel);
            this.Controls.Add(this.lblViewModel);
            this.Controls.Add(this.tbViewName);
            this.Controls.Add(this.lblViewName);
            this.Location = new System.Drawing.Point(150, 150);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddViewDlg";
            this.Text = "Add Django View";
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}