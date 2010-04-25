namespace Cheetah
{
    partial class GlControl
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

        public void Flip()
        {
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GlControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "GlControl";
            this.Size = new System.Drawing.Size(384, 281);
            this.Load += new System.EventHandler(this.GlControl_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GlControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GlControl_MouseMove);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GlControl_KeyUp);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GlControl_MouseUp);
            this.SizeChanged += new System.EventHandler(this.GlControl_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GlControl_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
