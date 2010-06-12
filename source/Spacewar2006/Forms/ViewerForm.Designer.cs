namespace Spacewar2006.Forms
{
    partial class ViewerForm
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
            this.glControl1 = new Cheetah.Graphics.GlControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SelectLight = new System.Windows.Forms.RadioButton();
            this.SelectCamera = new System.Windows.Forms.RadioButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Animations = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(152, 34);
            this.glControl1.Name = "glControl1";
            this.glControl1.Renderer = null;
            this.glControl1.Size = new System.Drawing.Size(640, 480);
            this.glControl1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SelectCamera);
            this.groupBox1.Controls.Add(this.SelectLight);
            this.groupBox1.Location = new System.Drawing.Point(3, 100);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(86, 87);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select";
            // 
            // SelectLight
            // 
            this.SelectLight.AutoSize = true;
            this.SelectLight.Location = new System.Drawing.Point(15, 33);
            this.SelectLight.Name = "SelectLight";
            this.SelectLight.Size = new System.Drawing.Size(48, 17);
            this.SelectLight.TabIndex = 0;
            this.SelectLight.Text = "Light";
            this.SelectLight.UseVisualStyleBackColor = true;
            this.SelectLight.CheckedChanged += new System.EventHandler(this.SelectLight_CheckedChanged);
            // 
            // SelectCamera
            // 
            this.SelectCamera.AutoSize = true;
            this.SelectCamera.Checked = true;
            this.SelectCamera.Location = new System.Drawing.Point(15, 56);
            this.SelectCamera.Name = "SelectCamera";
            this.SelectCamera.Size = new System.Drawing.Size(61, 17);
            this.SelectCamera.TabIndex = 1;
            this.SelectCamera.TabStop = true;
            this.SelectCamera.Text = "Camera";
            this.SelectCamera.UseVisualStyleBackColor = true;
            this.SelectCamera.CheckedChanged += new System.EventHandler(this.SelectCamera_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(833, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadToolStripMenuItem.Text = "Load...";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // Animations
            // 
            this.Animations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Animations.FormattingEnabled = true;
            this.Animations.Location = new System.Drawing.Point(3, 258);
            this.Animations.Name = "Animations";
            this.Animations.Size = new System.Drawing.Size(121, 21);
            this.Animations.TabIndex = 3;
            this.Animations.SelectedIndexChanged += new System.EventHandler(this.Animations_SelectedIndexChanged);
            this.Animations.SelectedValueChanged += new System.EventHandler(this.Animations_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 242);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Animation";
            // 
            // ViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 562);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Animations);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ViewerForm";
            this.Text = "Viewer";
            this.Load += new System.EventHandler(this.ViewerForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public Cheetah.Graphics.GlControl glControl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton SelectCamera;
        private System.Windows.Forms.RadioButton SelectLight;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ComboBox Animations;
        private System.Windows.Forms.Label label1;
    }
}