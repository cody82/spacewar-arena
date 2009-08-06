using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Spacewar2006.Forms
{
    public class Helper : Cheetah.ITickable
    {
        #region ITickable Members

        public void Tick(float dtime)
        {
            Application.DoEvents();
        }

        #endregion
    }
}
