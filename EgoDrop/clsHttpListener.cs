using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    internal class clsHttpListener : clsListener
    {
        private HttpListener m_listener { get; set; }
        private CancellationTokenSource m_cts { get; set; }

        public clsHttpListener(string szName, int nPort, bool bSecure, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_Protocol = clsSqlite.enListenerProtocol.HTTP;
            m_stListener = new clsSqlite.stListener(m_szName, m_Protocol, m_nPort, m_szDescription, DateTime.Now);

            m_listener = new HttpListener();
            m_listener.Prefixes.Add($"http://+:{nPort}/");

            m_cts = new CancellationTokenSource();
        }

        public override void fnStart()
        {
            m_listener.Start();
            _ = Task.Run(() => fnRecvLoop(m_cts.Token));
        }

        public override void fnStop()
        {
            //base.fnStop();

            m_listener.Stop();
        }

        private async Task fnRecvLoop(CancellationToken ct)
        {
            while (m_listener != null && m_listener.IsListening && !ct.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext ctx = await m_listener.GetContextAsync().ConfigureAwait(false);
                    _ = Task.Run(() => fnHandleRequest(ctx), ct);
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
            }
        }

        private async Task fnHandleRequest(HttpListenerContext context)
        {
            try
            {
                var req = context.Request;
                var res = context.Response;

                byte[] abBody = { };
                if (req.HasEntityBody)
                {
                    const long MAX_BODY = 1024 * 1024 * 10;
                    long? nContentLength = req.ContentLength64 >= 0 ? req.ContentLength64 : (long?)null;

                    if (nContentLength.HasValue && nContentLength.Value > MAX_BODY)
                    {
                        res.StatusCode = 413; //Payload too large.
                        await fnWriteStringAsync(res, "Payload Too Large.");

                        return;
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        await req.InputStream.CopyToAsync(ms);
                        abBody = ms.ToArray();
                    }
                }

                await fnHandler(req, res, abBody);
            }
            catch
            {

            }
        }

        private async Task fnWriteStringAsync(HttpListenerResponse res, string szText)
        {
            var abBuffer = Encoding.UTF8.GetBytes(szText);
            res.ContentType = "text/plain; charset=utf-8";
            res.ContentLength64 = abBuffer.Length;

            await res.OutputStream.WriteAsync(abBuffer, 0, abBuffer.Length);
        }

        private async Task fnHandler(HttpListenerRequest req, HttpListenerResponse res, byte[] abBody)
        {
            string szMsg = abBody != null ? Encoding.UTF8.GetString(abBody) : string.Empty;
            if (string.IsNullOrEmpty(szMsg))
                return;

            //todo: handler 1. decrypt message, convert it into list string.

            res.StatusCode = 404; //Fake status code.
            await fnWriteStringAsync(res, "OK");
        }
    }
}
