using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TVStation
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (button1.Text == "Capture and transmit")
			{
				button1.Text = "Stop capturing";
				Program.StartCapturing();
			}
			else
			{
				button1.Text = "Capture and transmit";
				Program.StopCapturing();
			}
		}
	}
}