using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SimpleChatClient
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
		}

		void textBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
				button1_Click(sender, null); // treat as enter click
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.textBox1.Text))
			{
				Program.Entered(this.textBox1.Text);
				this.textBox1.Text = "";
			}
		}
	}
}