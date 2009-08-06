using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using Lidgren.Library.Network;

namespace GenerateEncryptionKeys
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			byte[] pubKey;
			byte[] privKey;

			// generate a fresh set of keys
			NetEncryption.GenerateRandomKeyPair(out pubKey, out privKey);

			string pubKeyString = Convert.ToBase64String(pubKey);
			string privKeyString = Convert.ToBase64String(privKey);

			StringBuilder strb = new StringBuilder();

			/*
			 * Config.EnableEncryption(
				"AQABtk+MEd2cv1tKvrdJu4ofF1HsmmntRCUqgq3zEzqASbWhNI9MJ7vPD2zZu2HU5" +
				"Ogis9EsAuGmx63FZyhDnyPgcwubEGUHXlM+2bPSHzoFo2t69oRQ3GG9tQSq4u2LOQ" +
				"yP9kaWnudejKoW1Le+bMPfcXjl/q7c7+rvJJL4NOtArXs=",
				null
			);
			 * */
			strb.AppendLine("myConfig.EnableEncryption(");
			strb.Append(" \"");
			int num = 0;
			foreach (char c in pubKeyString)
			{
				strb.Append(c);
				num++;
				if (num > 64)
				{
					strb.AppendLine("\" +");
					strb.Append(" \"");
					num = 0;
				}
			}
			strb.AppendLine("\", null);");
			this.richTextBox1.Text = strb.ToString();

			strb = new StringBuilder();
			strb.AppendLine("myConfig.EnableEncryption(");
			strb.Append(" \"");
			num = 0;
			foreach (char c in pubKeyString)
			{
				strb.Append(c);
				num++;
				if (num > 64)
				{
					strb.AppendLine("\" +");
					strb.Append(" \"");
					num = 0;
				}
			}
			strb.AppendLine("\",");
			strb.Append(" \"");
			num = 0;
			foreach (char c in privKeyString)
			{
				strb.Append(c);
				num++;
				if (num > 64)
				{
					strb.AppendLine("\" +");
					strb.Append(" \"");
					num = 0;
				}
			}
			strb.AppendLine("\");");

			this.richTextBox2.Text = strb.ToString();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetDataObject(this.richTextBox1.Text, true);
			}
			catch { }
		}

		private void button3_Click(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetDataObject(this.richTextBox2.Text, true);
			}
			catch { }
		}
	}
}