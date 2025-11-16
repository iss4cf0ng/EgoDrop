using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteShell
{
    public class TerminalTabXterm
    {
        public clsLinux Linux { get; }
        public RichTextBox RichTextBox { get; }

        private Color currentColor = Color.White;
        private int cursorPos = 0;
        private string customPrompt = "[MyShell]$ ";

        // ANSI 顏色控制碼
        private static Regex ansiRegex = new Regex(@"\x1B\[[0-9;]*m", RegexOptions.Compiled);
        // 擴展 xterm 控制序列清理
        private static Regex xtermControlRegex = new Regex(
            @"\x1B\[\?2004[h,l]|\x1B\]0;.*?\x07|\x1b\[1;.*?R|\x1b\[?25[h,l]",
            RegexOptions.Compiled);

        public TerminalTabXterm(clsLinux linux, string prompt = "[MyShell]$ ")
        {
            Linux = linux;
            customPrompt = prompt;

            RichTextBox = new RichTextBox
            {
                Font = new Font("Consolas", 10),
                BackColor = Color.Black,
                ForeColor = Color.White,
                Multiline = true,
                Dock = DockStyle.Fill
            };

            RichTextBox.KeyPress += RichTextBox_KeyPress;
            RichTextBox.KeyDown += RichTextBox_KeyDown;

            SafeInvoke(() => ShowPrompt());
        }

        // -------------------- UI Thread 安全呼叫 --------------------
        private void SafeInvoke(Action action)
        {
            if (RichTextBox.IsHandleCreated && RichTextBox.InvokeRequired)
                RichTextBox.Invoke(action);
            else
                action();
        }

        // -------------------- 顯示 prompt --------------------
        public void ShowPrompt()
        {
            AppendTextWithANSI(customPrompt);
            cursorPos = RichTextBox.TextLength;
        }

        // -------------------- KeyPress: 可打印字符 --------------------
        private void RichTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            SendToLinux(e.KeyChar.ToString());
            e.Handled = true; // 禁止本地 echo
        }

        // -------------------- KeyDown: 特殊鍵 --------------------
        private void RichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string seq = null;
            bool handled = false;

            switch (e.KeyCode)
            {
                case Keys.Back: seq = "\x7f"; handled = true; break;
                case Keys.Enter: seq = "\n"; handled = true; break;
                case Keys.Up: seq = "\x1b[A"; handled = true; break;
                case Keys.Down: seq = "\x1b[B"; handled = true; break;
                case Keys.Left: seq = "\x1b[D"; MoveLocalCursor(-1); handled = true; break;
                case Keys.Right: seq = "\x1b[C"; MoveLocalCursor(1); handled = true; break;
                case Keys.Home: seq = "\x1b[H"; handled = true; break;
                case Keys.End: seq = "\x1b[F"; handled = true; break;
                case Keys.Delete: seq = "\x1b[3~"; handled = true; break;
            }

            if (seq != null)
                SendToLinux(seq);

            if (handled)
                e.Handled = true;
        }

        // -------------------- 發送 Base64 --------------------
        private void SendToLinux(string s)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(s);
                string b64 = Convert.ToBase64String(data);
                string msg = $"input|{b64}\n";
                Linux.Stream.Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
            }
            catch { }
        }

        // -------------------- ANSI 解析 append --------------------
        public void AppendTextWithANSI(string text)
        {
            SafeInvoke(() =>
            {
                int lastIndex = 0;
                foreach (Match m in ansiRegex.Matches(text))
                {
                    // 先 append 文字
                    if (m.Index > lastIndex)
                    {
                        string chunk = text.Substring(lastIndex, m.Index - lastIndex);
                        AppendPlainText(chunk);
                    }

                    // 處理 ANSI 控制碼
                    ProcessAnsiCode(m.Value);
                    lastIndex = m.Index + m.Length;
                }

                // 剩餘文字
                if (lastIndex < text.Length)
                {
                    string chunk = text.Substring(lastIndex);
                    AppendPlainText(chunk);
                }

                RichTextBox.ScrollToCaret();
            });
        }

        private void AppendPlainText(string chunk)
        {
            chunk = xtermControlRegex.Replace(chunk, "");

            foreach (char c in chunk)
            {
                if (c == '\r')
                {
                    int lastLine = RichTextBox.GetLineFromCharIndex(RichTextBox.TextLength);
                    int lineStart = RichTextBox.GetFirstCharIndexFromLine(lastLine);
                    RichTextBox.Select(lineStart, RichTextBox.TextLength - lineStart);
                    RichTextBox.SelectedText = "";
                    cursorPos = RichTextBox.TextLength;
                }
                else if (c == '\n')
                {
                    RichTextBox.AppendText(Environment.NewLine);
                    cursorPos = RichTextBox.TextLength;
                }
                else
                {
                    RichTextBox.SelectionStart = RichTextBox.TextLength;
                    RichTextBox.SelectionColor = currentColor;
                    RichTextBox.AppendText(c.ToString());
                    cursorPos = RichTextBox.TextLength;
                }
            }
        }

        private void ProcessAnsiCode(string code)
        {
            if (code.EndsWith("m"))
                currentColor = ANSIColor256(code, currentColor);
        }

        private void MoveLocalCursor(int delta)
        {
            cursorPos = Math.Max(0, Math.Min(RichTextBox.TextLength, cursorPos + delta));
            SafeInvoke(() =>
            {
                RichTextBox.SelectionStart = cursorPos;
            });
        }

        private Color ANSIColor256(string code, Color fallback)
        {
            var codes = code.Trim('\x1b', '[').TrimEnd('m').Split(';');
            foreach (var c in codes)
            {
                if (int.TryParse(c, out int n))
                {
                    switch (n)
                    {
                        case 0: fallback = Color.White; break;
                        case 30: fallback = Color.Black; break;
                        case 31: fallback = Color.Red; break;
                        case 32: fallback = Color.Green; break;
                        case 33: fallback = Color.Yellow; break;
                        case 34: fallback = Color.Blue; break;
                        case 35: fallback = Color.Magenta; break;
                        case 36: fallback = Color.Cyan; break;
                        case 37: fallback = Color.White; break;
                        case 90: fallback = Color.Gray; break;
                        case 91: fallback = Color.OrangeRed; break;
                        case 92: fallback = Color.LightGreen; break;
                        case 93: fallback = Color.LightYellow; break;
                        case 94: fallback = Color.LightBlue; break;
                        case 95: fallback = Color.LightPink; break;
                        case 96: fallback = Color.LightCyan; break;
                        case 97: fallback = Color.White; break;
                    }
                }
            }
            return fallback;
        }
    }
}
