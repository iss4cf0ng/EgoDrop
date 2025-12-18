using Rmg.Windows.ShellControls;

namespace TerminalGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ShellPreviewControl shell = new ShellPreviewControl();
            Controls.Add(shell);
            shell.Dock = DockStyle.Fill;

            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}
