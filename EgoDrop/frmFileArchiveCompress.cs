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
    public partial class frmFileArchiveCompress : Form
    {
        private List<frmFileMgr.stFileInfo> m_lsFile { get; init; }
        private bool m_bCompress { get; init; }

        public frmFileArchiveCompress(List<frmFileMgr.stFileInfo> lsFile, bool bCompress)
        {
            InitializeComponent();

            m_lsFile = lsFile;
            m_bCompress = bCompress;
        }

        void fnSetup()
        {
            //listView1
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("FileName", 200);
            listView1.Columns.Add("State", 80);

            //listView2
            listView2.View = View.Details;
            listView2.FullRowSelect = true;
            listView2.Columns.Add("FileName", 200);
            listView2.Columns.Add("State", 80);

            tabControl1.SelectedIndex = m_bCompress ? 0 : 1;

            foreach (var file in m_lsFile)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(file.szFilePath));
                item.SubItems.Add("?");
                item.Tag = file;

                if (m_bCompress)
                    listView1.Items.Add(item);
                else
                    listView2.Items.Add(item);
            }
        }

        private void frmFileArchiveCompress_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
