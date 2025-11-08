using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;

namespace EgoDrop
{
    public class clsTlsListener : clsListener
    {
        private X509Certificate m_certificate { get; set; }
        private TcpListener m_listener { get; set; }

        public clsTlsListener(
            string szName, 
            int nPort, 
            string szDescription, 
            string szCertificatePath, 
            string szCertificatePassword
        )
        {
            m_certificate = new X509Certificate(szCertificatePath, szCertificatePassword);
            m_listener = new TcpListener(IPAddress.Any, nPort);
        }

        public override void fnStart()
        {
            //base.fnStart();


        }

        public override void fnStop()
        {
            //base.fnStop();


        }
    }
}
