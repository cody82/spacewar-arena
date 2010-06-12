using Cheetah.Graphics;

namespace Cheetah.Forms
{
    partial class EditorForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.SelectTab = new System.Windows.Forms.TabPage();
            this.SelectList = new System.Windows.Forms.ListBox();
            this.PropertyTab = new System.Windows.Forms.TabPage();
            this.Properties = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.CreateTab = new System.Windows.Forms.TabPage();
            this.CreateButton = new System.Windows.Forms.Button();
            this.CreateList = new System.Windows.Forms.ListBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.glControl1 = new Cheetah.Graphics.GlControl();
            this.mapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.SelectTab.SuspendLayout();
            this.PropertyTab.SuspendLayout();
            this.CreateTab.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.SelectTab);
            this.tabControl1.Controls.Add(this.PropertyTab);
            this.tabControl1.Controls.Add(this.CreateTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(196, 528);
            this.tabControl1.TabIndex = 6;
            // 
            // SelectTab
            // 
            this.SelectTab.Controls.Add(this.SelectList);
            this.SelectTab.Location = new System.Drawing.Point(4, 23);
            this.SelectTab.Name = "SelectTab";
            this.SelectTab.Padding = new System.Windows.Forms.Padding(3);
            this.SelectTab.Size = new System.Drawing.Size(188, 501);
            this.SelectTab.TabIndex = 0;
            this.SelectTab.Text = "Select";
            this.SelectTab.UseVisualStyleBackColor = true;
            // 
            // SelectList
            // 
            this.SelectList.FormattingEnabled = true;
            this.SelectList.Location = new System.Drawing.Point(0, 10);
            this.SelectList.Name = "SelectList";
            this.SelectList.Size = new System.Drawing.Size(185, 485);
            this.SelectList.TabIndex = 0;
            this.SelectList.SelectedValueChanged += new System.EventHandler(this.SelectList_SelectedValueChanged);
            // 
            // PropertyTab
            // 
            this.PropertyTab.Controls.Add(this.Properties);
            this.PropertyTab.Location = new System.Drawing.Point(4, 23);
            this.PropertyTab.Name = "PropertyTab";
            this.PropertyTab.Padding = new System.Windows.Forms.Padding(3);
            this.PropertyTab.Size = new System.Drawing.Size(188, 501);
            this.PropertyTab.TabIndex = 1;
            this.PropertyTab.Text = "Properties";
            this.PropertyTab.UseVisualStyleBackColor = true;
            // 
            // Properties
            // 
            this.Properties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.Properties.FullRowSelect = true;
            this.Properties.GridLines = true;
            this.Properties.Location = new System.Drawing.Point(0, 10);
            this.Properties.MultiSelect = false;
            this.Properties.Name = "Properties";
            this.Properties.Size = new System.Drawing.Size(185, 482);
            this.Properties.TabIndex = 0;
            this.Properties.UseCompatibleStateImageBehavior = false;
            this.Properties.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 80;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 80;
            // 
            // CreateTab
            // 
            this.CreateTab.Controls.Add(this.CreateButton);
            this.CreateTab.Controls.Add(this.CreateList);
            this.CreateTab.Location = new System.Drawing.Point(4, 23);
            this.CreateTab.Name = "CreateTab";
            this.CreateTab.Size = new System.Drawing.Size(188, 501);
            this.CreateTab.TabIndex = 2;
            this.CreateTab.Text = "Create";
            this.CreateTab.UseVisualStyleBackColor = true;
            this.CreateTab.Click += new System.EventHandler(this.CreateTab_Click);
            // 
            // CreateButton
            // 
            this.CreateButton.Location = new System.Drawing.Point(25, 458);
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Size = new System.Drawing.Size(75, 23);
            this.CreateButton.TabIndex = 1;
            this.CreateButton.Text = "Create";
            this.CreateButton.UseVisualStyleBackColor = true;
            this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
            // 
            // CreateList
            // 
            this.CreateList.FormattingEnabled = true;
            this.CreateList.Location = new System.Drawing.Point(3, 10);
            this.CreateList.Name = "CreateList";
            this.CreateList.Size = new System.Drawing.Size(182, 433);
            this.CreateList.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.mapToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(854, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(244, 50);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(55, 17);
            this.radioButton1.TabIndex = 3;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Select";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(349, 50);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(52, 17);
            this.radioButton2.TabIndex = 4;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Move";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(452, 50);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(57, 17);
            this.radioButton3.TabIndex = 5;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Rotate";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(196, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(658, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(196, 49);
            this.glControl1.Name = "glControl1";
            //this.glControl1.Renderer = null;
            this.glControl1.Size = new System.Drawing.Size(658, 503);
            this.glControl1.TabIndex = 7;
            // 
            // mapToolStripMenuItem
            // 
            this.mapToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.mapToolStripMenuItem.Name = "mapToolStripMenuItem";
            this.mapToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.mapToolStripMenuItem.Text = "Map";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // EditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 552);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EditorForm";
            this.Text = "Editor";
            this.Load += new System.EventHandler(this.EditorForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.SelectTab.ResumeLayout(false);
            this.PropertyTab.ResumeLayout(false);
            this.CreateTab.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage SelectTab;
        private System.Windows.Forms.TabPage PropertyTab;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ListBox SelectList;
        private System.Windows.Forms.TabPage CreateTab;
        private System.Windows.Forms.ListBox CreateList;
        private System.Windows.Forms.ListView Properties;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button CreateButton;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        public GlControl glControl1;
        private System.Windows.Forms.ToolStripMenuItem mapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
    }
}