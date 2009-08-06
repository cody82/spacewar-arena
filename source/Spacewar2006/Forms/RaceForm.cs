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
    public partial class RaceForm : UserControl, IRuleCreator
    {
        public RaceForm()
        {
            InitializeComponent();
        }

        #region IRuleCreator Members

        public GameRule CreateRule()
        {
            return new Race((int)Laps.Value);
        }

        #endregion
    }
}
