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
        try
        {
            if (args.Length < 3) { Console.WriteLine("Usage: RemoteShellConsole <server_ip> <server_port> <linuxId>"); return; }

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

            Task.Run(() => ReadLoop());
            InputLoop();

            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
        finally
        {
            Console.ReadKey();
        }
    }

    static void ReadLoop()
    {
        var sb = new StringBuilder();
        var buf = new byte[4096];

        while (true)
        {
            int r;
            try { r = stream.Read(buf, 0, buf.Length); }
            catch { Task.Delay(10).Wait(); continue; }

            if (r <= 0)
            {
                Task.Delay(10).Wait();
                continue;
            }

            sb.Append(Encoding.UTF8.GetString(buf, 0, r));

            while (true)
            {
                string s = sb.ToString();
                int idx = s.IndexOf('\n');
                if (idx == -1) break;

                string line = s.Substring(0, idx).TrimEnd('\r');
                sb.Remove(0, idx + 1);

                if (line.StartsWith("output|"))
                {
                    string b64 = line.Substring("output|".Length);
                    byte[] data;
                    try { data = Convert.FromBase64String(b64); }
                    catch { continue; }

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
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                byte[] bytes;

                switch (key.Key)
                {
                    case ConsoleKey.Enter: bytes = Encoding.UTF8.GetBytes("\n"); break;
                    case ConsoleKey.Backspace: bytes = new byte[] { 0x7f }; break;
                    case ConsoleKey.UpArrow: bytes = Encoding.UTF8.GetBytes("\x1b[A"); break;
                    case ConsoleKey.DownArrow: bytes = Encoding.UTF8.GetBytes("\x1b[B"); break;
                    case ConsoleKey.LeftArrow: bytes = Encoding.UTF8.GetBytes("\x1b[D"); break;
                    case ConsoleKey.RightArrow: bytes = Encoding.UTF8.GetBytes("\x1b[C"); break;
                    default: bytes = Encoding.UTF8.GetBytes(new char[] { key.KeyChar }); break;
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