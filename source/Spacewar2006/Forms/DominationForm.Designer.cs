namespace Spacewar2006.Forms
{
    partial class DominationForm
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
            this.Teams = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ScoreInterval = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.ScoreLimit = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TimeLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScoreInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScoreLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // TimeLimit
            // 
            this.TimeLimit.Location = new System.Drawing.Point(20, 26);
            this.TimeLimit.Name = "TimeLimit";
            this.TimeLimit.Size = new System.Drawing.Size(120, 20);
            this.TimeLimit.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "TimeLimit";
            // 
            // Teams
            // 
            this.Teams.AcceptsReturn = true;
            this.Teams.Location = new System.Drawing.Point(20, 167);
            this.Teams.Multiline = true;
            this.Teams.Name = "Teams";
            this.Teams.Size = new System.Drawing.Size(149, 70);
            this.Teams.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Teams (one team per line)";
            // 
            // ScoreInterval
            // 
            this.ScoreInterval.Location = new System.Drawing.Point(20, 71);
            this.ScoreInterval.Name = "ScoreInterval";
            this.ScoreInterval.Size = new System.Drawing.Size(120, 20);
            this.ScoreInterval.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "ScoreInterval";
            // 
            // ScoreLimit
            // 
            this.ScoreLimit.Location = new System.Drawing.Point(20, 115);
            this.ScoreLimit.Name = "ScoreLimit";
            this.ScoreLimit.Size = new System.Drawing.Size(120, 20);
            this.ScoreLimit.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "ScoreLimit";
            // 
            // DominationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ScoreLimit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ScoreInterval);
            this.Controls.Add(this.Teams);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TimeLimit);
            this.Name = "DominationForm";
            this.Size = new System.Drawing.Size(193, 270);
            ((System.ComponentModel.ISupportInitialize)(this.TimeLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScoreInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScoreLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown TimeLimit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Teams;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown ScoreInterval;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown ScoreLimit;
        private System.Windows.Forms.Label label4;
    }
}
