namespace EgoDrop
{
    public partial class frmMain : Form
    {
        private clsSqlite m_sqlite;
        private clsListener m_listener;

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
            frmListener f = new frmListener();

            f.ShowDialog();
        }

        //Builder
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        //Setting
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {

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
    }
}
