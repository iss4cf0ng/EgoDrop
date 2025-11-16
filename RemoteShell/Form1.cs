using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using ConsoleControl;
using ConsoleControlAPI;

namespace RemoteShell
{
    public partial class Form1 : Form
    {
        private Dictionary<int, clsLinux> linuxMap = new();
        private Dictionary<int, ConsoleControl.ConsoleControl> terminalMap = new();
        private int nextLinuxId = 1;

        public Form1()
        {
            InitializeComponent();
            this.Load += (s, e) => Task.Run(() => StartLinuxListener(5555));
        }

        private void StartLinuxListener(int port)
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (true)
            {
                var client = listener.AcceptTcpClient();
                var stream = client.GetStream();
                int id = nextLinuxId++;
                var linux = new clsLinux
                {
                    Id = id,
                    Host = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(),
                    Client = client,
                    Stream = stream
                };
                lock (linuxMap) { linuxMap[id] = linux; }

                Invoke(new Action(() =>
                {
                    var item = new ListViewItem($"{linux.Host} (id:{linux.Id})");
                    item.Tag = linux;
                    listView1.Items.Add(item);
                }));

                Task.Run(() => LinuxReadLoop(linux));
            }
        }

        private void LinuxReadLoop(clsLinux linux)
        {
            var buf = new byte[4096];
            while (true)
            {
                int r;
                try { r = linux.Stream.Read(buf, 0, buf.Length); }
                catch { break; }
                if (r <= 0) break;

                string msg = Encoding.UTF8.GetString(buf, 0, r);

                foreach (var line in msg.Split('\n'))
                {
                    if (line.StartsWith("shell|"))
                    {
                        string b64 = line.Substring("shell|".Length);
                        string text = Encoding.UTF8.GetString(Convert.FromBase64String(b64));

                        // 過濾 VT100 / ANSI 控制碼
                        text = Regex.Replace(text, @"\x1B\[[0-9;]*[A-Za-z]", "");

                        if (terminalMap.TryGetValue(linux.Id, out var console))
                        {
                            console.Invoke(() => console.WriteOutput(text, Color.White));
                        }
                    }
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var item = listView1.SelectedItems[0];
            var linux = item.Tag as clsLinux;
            if (linux == null) return;
            if (terminalMap.ContainsKey(linux.Id)) return;

            // 建立 ConsoleControl
            var console = new ConsoleControl.ConsoleControl
            {
                Font = new System.Drawing.Font("Consolas", 10),
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                Dock = DockStyle.Fill,
                ShowDiagnostics = false
            };

            // 處理鍵盤輸入 + 本地 echo
            console.KeyPress += (s, e) =>
            {
                try
                {
                    // 本地 echo
                    console.WriteOutput(e.KeyChar.ToString(), Color.White);

                    // 發送給 Linux
                    byte[] data = Encoding.UTF8.GetBytes(e.KeyChar.ToString());
                    string b64 = Convert.ToBase64String(data);
                    string msg = $"input|{b64}\n";
                    linux.Stream.Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
                }
                catch { }
                e.Handled = true; // 禁止控件原生 echo
            };

            terminalMap[linux.Id] = console;

            // 新增 Tab
            var tabPage = new TabPage(linux.Host);
            tabPage.Controls.Add(console);
            Invoke(() =>
            {
                tabControl1.TabPages.Add(tabPage);
                tabControl1.SelectedTab = tabPage;
            });
        }
    }

    public class clsLinux
    {
        public int Id;
        public string Host;
        public TcpClient Client;
        public NetworkStream Stream;
    }
}
