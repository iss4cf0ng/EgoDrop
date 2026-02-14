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
        private bool m_bIsRunning = false;

        private enMode m_mode { get; init; } //File transfering mode.
        private clsAgent m_agent { get; init; } //Agent object.
        private string m_szFilePath { get; init; } //Local filepath.
        private int m_nFileChunkSize { get; init; } //File transfering chunk size.
        private long m_nFileSize { get; init; }

        private FileStream m_fileStream { get; init; } //File stream.

        private int m_nIdx = 0; //Index.

        public enum enMode
        {
            Upload,
            Download,
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="agent"></param>
        /// <param name="szFilePath"></param>
        /// <param name="nFileChunkSize"></param>
        public clsFileHandler(enMode mode, clsAgent agent, string szFilePath, int nFileChunkSize = 1024 * 1024 * 5)
        {
            m_mode = mode;
            m_agent = agent;
            m_szFilePath = szFilePath;
            m_nFileChunkSize = nFileChunkSize;

            if (m_mode == enMode.Upload)
            {
                m_nFileSize = new FileInfo(szFilePath).Length;
                m_fileStream = File.Open(szFilePath, FileMode.Open, FileAccess.Read);
            }
            else if (m_mode == enMode.Download)
            {
                m_fileStream = File.Open(szFilePath, FileMode.Create, FileAccess.Write);
            }
        }

        public void fnStart()
        {
            m_bIsRunning = true;
        }

        public void fnStop()
        {
            m_bIsRunning = false;
        }

        public bool fnbIsRunning() => m_bIsRunning;

        /// <summary>
        /// Send file chunk buffer.
        /// </summary>
        public int fnSend()
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

            m_nIdx++;

            return m_nIdx;
        }

        public bool fnbIsLast(int nOffset)
        {
            return nOffset + m_nFileChunkSize >= m_nFileSize;
        }

        /// <summary>
        /// Write file chunk buffer.
        /// </summary>
        /// <param name="nOffset"></param>
        /// <param name="abBuffer"></param>
        public void fnWrite(int nOffset, byte[] abBuffer)
        {
            m_fileStream.Write(abBuffer, nOffset, abBuffer.Length);
        }
    }

    public class clsChunkTimer
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
