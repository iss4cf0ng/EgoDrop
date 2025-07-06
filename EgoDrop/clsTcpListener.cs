using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    internal class clsTcpListener : clsListener
    {
        private Socket m_sktSrv;
        private ListenerType lisType;

        public clsTcpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;

            m_sktSrv = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            lisType = ListenerType.TCP;
        }

        public override void fnStart()
        {
            
        }

        public override void fnStop()
        {

        }
    }
}
