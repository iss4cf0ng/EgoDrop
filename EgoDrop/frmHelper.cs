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
    public partial class frmHelper : Form
    {
        private string m_szPath { get; init; }

        public frmHelper(string szPath)
        {
            InitializeComponent();

            m_szPath = szPath;
            Text = szPath;
        }

        void fnSetup()
        {
            string szFilePath = Path.Combine(Application.StartupPath, "Docs", "RTF", m_szPath) + ".md";
            if (!File.Exists(szFilePath))
            {
                MessageBox.Show("File not found: " + szFilePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            textBox1.Text = szFilePath;
            textBox1.ReadOnly = true;

            richTextBox1.LoadFile(szFilePath);
        }

        private void frmHelper_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string szFilePath = textBox1.Text;
            if (!File.Exists(szFilePath))
            {
                MessageBox.Show("File not found: " + szFilePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            richTextBox1.LoadFile(szFilePath);
        }
    }
}
