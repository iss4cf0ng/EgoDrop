using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;

namespace EgoDrop
{
    public class clsHttpsListener :clsListener
    {
        private TcpListener m_listener { get; init; }
        private X509Certificate m_cert { get; init; }

        private const string AUTH_TOKEN = "mysecrettoken";
        private readonly string[] ALLOW_HOSTS = { "127.0.0.1", "localhost", };

        /// <summary>
        /// HTTPS listener.
        /// </summary>
        /// <param name="szName">Listener's name.</param>
        /// <param name="nPort">Listener's listening port.</param>
        /// <param name="szDescription">Listener's description.</param>
        /// <param name="szCertFilePath">TLS certificate file path.</param>
        /// <param name="szCertPassword">TLS certificate file password.</param>
        public clsHttpsListener(
            string szName,
            int nPort,
            string szDescription,
            string szCertFilePath,
            string szCertPassword
        )
        {
            m_szName = szName;
            m_nPort = nPort;
            m_cert = new X509Certificate(szCertFilePath);
            m_listener = new TcpListener(IPAddress.Any, nPort);
        }

        public override void fnStart()
        {
            //base.fnStart();

            if (m_bIsListening)
            {
                MessageBox.Show($"Listener[{m_szName}] is already in used.", "fnStart()", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            m_listener.Start();
            m_listener.BeginAcceptTcpClient(new AsyncCallback(fnBeginAcceptTcpClient), null);
            m_bIsListening = true;
        }

        public override void fnStop()
        {
            m_bIsListening = false;
            m_listener.Stop();
        }

        private void fnBeginAcceptTcpClient(IAsyncResult ar)
        {

        }
    }
}
