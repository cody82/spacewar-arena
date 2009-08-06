using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Cheetah;
using SpaceWar2006.Flows;

namespace Spacewar2006.Forms
{
    public partial class ViewerForm : Form
    {
        public ViewerForm()
        {
            InitializeComponent();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.ShowDialog();
            Viewer v = ((Viewer)Root.Instance.CurrentFlow);

            Animations.Items.Clear();
            if (v.m is Model)
            {
                foreach (SkeletalAnimation a in ((Model)v.m).Animations)
                {
                    Animations.Items.Add(a.Name);
                }
            }
        }

        private void SelectLight_CheckedChanged(object sender, EventArgs e)
        {
            if(SelectLight.Checked)
            {
                Viewer v = ((Viewer)Root.Instance.CurrentFlow);
                v.Selection = Viewer.Objects.Light;
            }
        }

        private void SelectCamera_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectCamera.Checked)
            {
                Viewer v = ((Viewer)Root.Instance.CurrentFlow);
                v.Selection = Viewer.Objects.Camera;
            }
        }

        private void ViewerForm_Load(object sender, EventArgs e)
        {
        }

        private void Animations_SelectedValueChanged(object sender, EventArgs e)
        {

        }

        private void Animations_SelectedIndexChanged(object sender, EventArgs e)
        {
            Viewer v = ((Viewer)Root.Instance.CurrentFlow);

            v.ChangeAnim(Animations.SelectedIndex);
        }
    }
}