namespace Spacewar2006.Forms
{
    partial class TeamDeathMatchForm
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
            this.FragLimit = new System.Windows.Forms.NumericUpDown();
            this.TimeLimit = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Teams = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.FragLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TimeLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // FragLimit
            // 
            this.FragLimit.Location = new System.Drawing.Point(20, 26);
            this.FragLimit.Name = "FragLimit";
            this.FragLimit.Size = new System.Drawing.Size(120, 20);
            this.FragLimit.TabIndex = 0;
            // 
            // TimeLimit
            // 
            this.TimeLimit.Location = new System.Drawing.Point(20, 68);
            this.TimeLimit.Name = "TimeLimit";
            this.TimeLimit.Size = new System.Drawing.Size(120, 20);
            this.TimeLimit.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "TimeLimit";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "FragLimit";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Teams (one team per line)";
            // 
            // Teams
            // 
            this.Teams.AcceptsReturn = true;
            this.Teams.Location = new System.Drawing.Point(22, 119);
            this.Teams.Multiline = true;
            this.Teams.Name = "Teams";
            this.Teams.Size = new System.Drawing.Size(149, 90);
            this.Teams.TabIndex = 6;
            // 
            // TeamDeathMatchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Teams);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TimeLimit);
            this.Controls.Add(this.FragLimit);
            this.Name = "TeamDeathMatchForm";
            this.Size = new System.Drawing.Size(201, 232);
            ((System.ComponentModel.ISupportInitialize)(this.FragLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TimeLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown FragLimit;
        private System.Windows.Forms.NumericUpDown TimeLimit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Teams;
    }
}
