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
    public partial class frmFileTransfer : Form
    {
        private clsVictim m_victim { get; set; }
        private List<string> m_lsFilePath { get; set; }
        private enTransfer m_enTransfer { get; set; }

        private Dictionary<string, clsFileHandler> m_dicHandler = new Dictionary<string, clsFileHandler>();

        public enum enTransfer
        {
            Upload,
            Download,
        };

        public frmFileTransfer(clsVictim victim, List<string> lsFilePath, enTransfer enTransfer)
        {
            InitializeComponent();

            m_victim = victim;
            m_lsFilePath = lsFilePath;
            m_enTransfer = enTransfer;
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim))
                return;

            if (lsMsg[0] == "file")
            {
                if (lsMsg[1] == "uf") //Upload File.
                {
                    //Received ACK.
                    int nSeq = int.Parse(lsMsg[2]);
                    string szFilePath = lsMsg[3];

                    clsFileHandler handler = m_dicHandler[szFilePath];
                    handler.fnAck(nSeq);


                }
                else if (lsMsg[1] == "df") //Download File.
                {

                }
            }
        }

        void fnUpdateProgress(string szFilePath, int nValue)
        {
            Invoke(new Action(() =>
            {   
                ListViewItem item = listView1.FindItemWithText(szFilePath, true, 0);
                if (item == null)
                    return;

                item.SubItems[3].Text = nValue.ToString();
                listView1.Invalidate(item.SubItems[3].Bounds);
            }));
        }

        void fnStartTransfer()
        {

        }

        void fnSetup()
        {
            m_dicHandler.Clear();

            //ListView initialization.
            listView1.OwnerDraw = true;
            listView1.DrawColumnHeader += (s, e) =>
            {
                e.DrawDefault = true;
            };
            listView1.DrawSubItem += (s, e) =>
            {
                if (e.ColumnIndex == 3)
                {
                    int nProgress = int.Parse(e.SubItem.Text);
                    Rectangle rect = e.Bounds;
                    rect.Inflate(-2, -4);
                    ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rect);
                    rect.Inflate(-1, -1);
                    rect.Width = rect.Width * nProgress / 100;
                    e.Graphics.FillRectangle(Brushes.LightGreen, rect);
                }
                else
                {
                    e.DrawDefault = true;
                }
            };

            foreach (string szFilePath in m_lsFilePath)
            {
                string szFileName = szFilePath.Split('/').Last();
                ListViewItem item = new ListViewItem(szFileName);
                item.SubItems.Add(szFilePath);
                item.SubItems.Add(Enum.GetName(m_enTransfer));
                item.SubItems.Add("0");

                listView1.Items.Add(item);

                clsFileHandler handler = new clsFileHandler(
                    m_victim,
                    szFilePath,
                    m_enTransfer,
                    1024 * 5,
                    5
                );

                m_dicHandler[szFilePath] = handler;
            }

            
        }

        private void frmFileTransfer_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
