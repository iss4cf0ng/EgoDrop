using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsHttpPkt
    {
        private string m_szHost { get; set; }
        private clsSqlite.enHttpMethod m_httpMethod { get; set; }
        private string m_szPath { get; set; }
        private string m_szUA { get; set; }

        public clsHttpPkt(string szHost, clsSqlite.enHttpMethod httpMethod, string szPath, string szUA)
        {
            m_szHost = szHost;
            m_httpMethod = httpMethod;
            m_szPath = szPath;
            m_szUA = szUA;
        }

        public byte[] fnabGetPacket(string szMsg)
        {
            string szBody = $"" +
                $"HTTP/1.1 200 OK\r\n" +
                $"Content-Type: text/plain; charset=utf-8\r\n" +
                $"Content-Length: {Encoding.UTF8.GetByteCount(szMsg)}\r\n" +
                $"Connection: close\r\n\r\n" +
                $"{szMsg}";

            return Encoding.UTF8.GetBytes(szBody);
        }
    }
}
