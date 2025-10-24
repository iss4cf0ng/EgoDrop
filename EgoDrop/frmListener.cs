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
    public partial class frmListener : Form
    {

        public frmListener()
        {
            InitializeComponent();
        }

        private void fnLoadListener()
        {
            listView1.Items.Clear();



            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
        }

        private void fnSetup()
        {

        }

        private void frmListener_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Start
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        //Stop
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }
        //Add
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            frmListenerEdit f = new frmListenerEdit(this);

            f.ShowDialog();
        }
    }
}
