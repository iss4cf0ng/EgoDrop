using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Timers;

namespace EgoDrop
{
    public class clsFileHandler
    {
        private enMode m_mode { get; init; }
        private clsAgent m_agent { get; init; }
        private string m_szFilePath { get; init; }
        private int m_nFileChunkSize { get; init; }

        private FileStream m_fileStream { get; init; }

        private int m_nIdx = 0;

        public enum enMode
        {
            Upload,
            Download,
        }

        public clsFileHandler(enMode mode, clsAgent agent, string szFilePath, int nFileChunkSize = 1024 * 1024 * 5)
        {
            m_mode = mode;
            m_agent = agent;
            m_szFilePath = szFilePath;
            m_nFileChunkSize = nFileChunkSize;

            if (m_mode == enMode.Upload)
                m_fileStream = File.Open(szFilePath, FileMode.Open, FileAccess.Read);
            else if (m_mode == enMode.Download)
                m_fileStream = File.Open(szFilePath, FileMode.Create, FileAccess.Write);
        }

        public void fnSend()
        {
            byte[] abBuffer = new byte[m_nFileChunkSize];
            int nOffset = m_nIdx * m_nFileChunkSize;
            int nRead = m_fileStream.Read(abBuffer, nOffset, m_nFileChunkSize);
            byte[] abRead = new byte[nRead];
            Buffer.BlockCopy(abBuffer, 0, abRead, 0, nRead);

            m_agent.fnSendCommand(new string[]
            {
                "file",
                "uf",
                nOffset.ToString(),
                Convert.ToBase64String(abRead),
            });
        }

        public void fnWrite(int nOffset, byte[] abBuffer)
        {
            m_fileStream.Write(abBuffer, nOffset, abBuffer.Length);
        }
    }

    internal class clsChunkTimer
    {
        private readonly System.Timers.Timer m_timer;
        private readonly Action m_actOnTimeout;

        public clsChunkTimer(double nMiliSecond, Action actOnTimeout)
        {
            m_timer = new System.Timers.Timer(nMiliSecond);
            m_actOnTimeout = actOnTimeout;

            m_timer.Elapsed += (s, e) => m_actOnTimeout(); //Counting event.
            m_timer.AutoReset = false;
        }

        public void fnStart() => m_timer.Start();          //Start timer.
        public void fnStop()  => m_timer.Stop();            //Stop timer.
    }
}
