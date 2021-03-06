using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using SpaceWar2006.GameObjects;
using SpaceWar2006.Rules;

namespace Spacewar2006.Forms
{
    public partial class DeathMatchForm : UserControl, IRuleCreator
    {
        public DeathMatchForm()
        {
            InitializeComponent();
        }

        #region IRuleCreator Members

        public GameRule CreateRule()
        {
            return new DeathMatch((int)FragLimit.Value, (float)TimeLimit.Value * 60);
        }

        #endregion
    }
}
