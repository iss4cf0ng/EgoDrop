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
    public partial class frmGroup : Form
    {
        private frmMain m_frmMain { get; init; }
        private clsSqlite m_sqlConn { get; init; }

        public frmGroup(frmMain frmMain, clsSqlite sqlConn)
        {
            InitializeComponent();

            m_frmMain = frmMain;
            m_sqlConn = sqlConn;
        }

        void fnSetup()
        {

        }

        private void frmGroup_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
