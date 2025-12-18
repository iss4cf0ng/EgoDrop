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

        public delegate void dlgNewVictim(clsListener listener, clsVictim victim);
        public event dlgNewVictim evtNewVictim;
        public delegate void dlgReceivedMessage(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg);
        public event dlgReceivedMessage evtReceivedMessage;
        public delegate void dlgVictimDisconnected(clsListener listener, clsVictim victim, string szVictimID);
        public event dlgVictimDisconnected evtVictimDisconnected;
        public delegate void dlgAddChain(List<string> lsVictim, string szOS, string szUsername, bool bRoot, string szIPv4);
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

        public void fnOnNewVictim(clsVictim victim)
        {
            evtNewVictim?.Invoke(this, victim);
        }

        public void fnOnReceivedMessage(clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            evtReceivedMessage?.Invoke(this, victim, szSrcVictimID, lsMsg);
        }

        public void fnOnVictimDisconnected(clsVictim victim, string szVictimID)
        {
            evtVictimDisconnected?.Invoke(this, victim, szVictimID);
        }

        public void fnOnAddChain(List<string> lsVictim, string szOS, string szUsername, bool bRoot, string szIPv4)
        {
            evtAddChain?.Invoke(lsVictim, szOS, szUsername, bRoot, szIPv4);
        }
    }
}
