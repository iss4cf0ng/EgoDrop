// Program.cs
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static TcpClient client;
    static NetworkStream stream;

    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: RemoteShellConsole <server_ip> <server_port> <linuxId>");
            return;
        }

        string serverIp = args[0];
        int serverPort = int.Parse(args[1]);
        int linuxId = int.Parse(args[2]);

        client = new TcpClient();
        client.Connect(serverIp, serverPort);
        stream = client.GetStream();

        // attach
        var attachMsg = $"attach|{linuxId}\n";
        var attachBytes = Encoding.UTF8.GetBytes(attachMsg);
        stream.Write(attachBytes, 0, attachBytes.Length);

        // start reading server -> output
        Task.Run(() => ReadLoop());

        // start input loop
        InputLoop();
    }

    static void ReadLoop()
    {
        var sb = new StringBuilder();
        var buf = new byte[4096];
        while (true)
        {
            int r;
            try { r = stream.Read(buf, 0, buf.Length); }
            catch { break; }
            if (r <= 0) break;

            sb.Append(Encoding.UTF8.GetString(buf, 0, r));
            while (true)
            {
                var s = sb.ToString();
                var idx = s.IndexOf('\n');
                if (idx == -1) break;
                string line = s.Substring(0, idx).TrimEnd('\r');
                sb.Remove(0, idx + 1);

                if (line.StartsWith("output|"))
                {
                    string b64 = line.Substring("output|".Length);
                    byte[] data;
                    try { data = Convert.FromBase64String(b64); }
                    catch { continue; }

                    // Directly write decoded bytes to console (supports ANSI)
                    string text = Encoding.UTF8.GetString(data);
                    Console.Write(text);
                }
            }
        }
    }

    static void InputLoop()
    {
        while (true)
        {
            // Non-blocking key read approach
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);

                byte[] bytes;
                if (key.Key == ConsoleKey.Enter)
                    bytes = Encoding.UTF8.GetBytes("\n");
                else if (key.Key == ConsoleKey.Backspace)
                    bytes = new byte[] { 0x7f }; // DEL
                else if (key.Key == ConsoleKey.UpArrow)
                    bytes = Encoding.UTF8.GetBytes("\x1b[A");
                else if (key.Key == ConsoleKey.DownArrow)
                    bytes = Encoding.UTF8.GetBytes("\x1b[B");
                else if (key.Key == ConsoleKey.LeftArrow)
                    bytes = Encoding.UTF8.GetBytes("\x1b[D");
                else if (key.Key == ConsoleKey.RightArrow)
                    bytes = Encoding.UTF8.GetBytes("\x1b[C");
                else
                {
                    char c = key.KeyChar;
                    bytes = Encoding.UTF8.GetBytes(new char[] { c });
                }

                string b64 = Convert.ToBase64String(bytes);
                string msg = $"input|{b64}\n";
                var outb = Encoding.UTF8.GetBytes(msg);
                try { stream.Write(outb, 0, outb.Length); }
                catch { return; }
            }

            Task.Delay(10).Wait();
        }
    }
}