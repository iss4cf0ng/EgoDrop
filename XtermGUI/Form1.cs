using CefSharp;
using CefSharp.WinForms;
using System.Text;

namespace XtermGUI
{
    public class JsBridge
    {
        public void sendInput(string b64)
        {
            string szInput = Encoding.UTF8.GetString(Convert.FromBase64String(b64));

        }
    }

    public partial class Form1 : Form
    {
        private ChromiumWebBrowser browser;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string szHtmlPath = Path.Combine(new string[] { Application.StartupPath, "Xterm", "index.html" });
            browser = new ChromiumWebBrowser()
            {
                Dock = DockStyle.Fill,
            };
            browser.LoadUrl(szHtmlPath);

            Controls.Add(browser);
            browser.JavascriptObjectRepository.
        }
    }
}
