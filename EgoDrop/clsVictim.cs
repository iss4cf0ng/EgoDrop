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

        public void fnSendRaw()
        {

        }

        public void fnSendEnc()
        {

        }

        public void fnSendCmdParam(int nCmd, int nParam)
        {

        }

        public void fnSendCommand()
        {

        }
    }
}
