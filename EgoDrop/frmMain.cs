namespace EgoDrop
{
    public partial class frmMain : Form
    {
        private clsSqlite m_sqlite;

        public frmMain()
        {
            InitializeComponent();
        }

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

        }
    }
}
