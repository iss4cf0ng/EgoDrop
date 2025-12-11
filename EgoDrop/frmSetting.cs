using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EgoDrop
{
    public partial class frmSetting : Form
    {
        private clsIniMgr m_iniMgr { get; init; }

        public frmSetting(clsIniMgr iniMgr)
        {
            InitializeComponent();

            m_iniMgr = iniMgr;
        }
    }
}
