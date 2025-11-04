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
    public partial class frmBuilder : Form
    {
        private frmMain m_fMain { get; set; }
        private clsSqlite m_sqlite { get; set; }
        private clsIniMgr m_iniMgr { get; set; }

        public frmBuilder(frmMain fMain, clsSqlite sqlite, clsIniMgr iniMgr)
        {
            InitializeComponent();

            m_fMain = fMain;
            m_sqlite = sqlite;
            m_iniMgr = iniMgr;
        }

        void fnSetup()
        {

        }

        private void frmBuilder_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
