namespace EgoDrop
{
    public partial class frmMain : Form
    {
        public Dictionary<string, clsListener> m_dicListener = new Dictionary<string, clsListener>();

        private clsSqlite m_sqlite { get; set; }
        private clsListener m_listener { get; set; }

        public frmMain()
        {
            InitializeComponent();
        }

        private clsVictim fnGetVictimFromTag(ListViewItem item) => (clsVictim)item.Tag;

        private void fnSetup()
        {
            m_sqlite = new clsSqlite("data.db");
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Listener
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmListener f = new frmListener(m_sqlite, m_dicListener);

            f.ShowDialog();
        }

        //Builder
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            frmBuilder f = new frmBuilder();
            f.Show();
        }

        //Setting
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            frmSetting f = new frmSetting();

            f.ShowDialog();
        }

        //About
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            new frmAbout().Show();
        }
        //Info
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmInfoSpyder f = new frmInfoSpyder(fnGetVictimFromTag(item));
                f.Show();
            }
        }
        //File
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmFileMgr f = new frmFileMgr(fnGetVictimFromTag(item));
                f.Show();
            }
        }
        //Process
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {

        }
        //Service
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {

        }
        //Monitor
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                
            }
        }
        //Camera
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            frmCamera f = new frmCamera();

            f.Show();
        }
    }
}
