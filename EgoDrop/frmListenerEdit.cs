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
    public partial class frmListenerEdit : Form
    {
        private frmListener m_frmListener { get; set; }

        public frmListenerEdit(frmListener f)
        {
            InitializeComponent();

            m_frmListener = f;
        }

        void fnSetup()
        {

        }

        private void frmListenerEdit_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
