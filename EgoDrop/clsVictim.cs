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

        public clsVictim(Socket sktClnt)
        {
            m_sktClnt = sktClnt;
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
