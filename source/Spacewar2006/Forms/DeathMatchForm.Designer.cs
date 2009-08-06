namespace Spacewar2006.Forms
{
    partial class DeathMatchForm
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TimeLimit = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.FragLimit = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TimeLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FragLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // TimeLimit
            // 
            this.TimeLimit.Location = new System.Drawing.Point(16, 45);
            this.TimeLimit.Name = "TimeLimit";
            this.TimeLimit.Size = new System.Drawing.Size(96, 20);
            this.TimeLimit.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "TimeLimit";
            // 
            // FragLimit
            // 
            this.FragLimit.Location = new System.Drawing.Point(16, 103);
            this.FragLimit.Name = "FragLimit";
            this.FragLimit.Size = new System.Drawing.Size(96, 20);
            this.FragLimit.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "FragLimit";
            // 
            // DeathMatchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FragLimit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TimeLimit);
            this.Name = "DeathMatchForm";
            ((System.ComponentModel.ISupportInitialize)(this.TimeLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FragLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown TimeLimit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown FragLimit;
        private System.Windows.Forms.Label label2;
    }
}
