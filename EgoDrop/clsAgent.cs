using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsAgent
    {
        public clsListener m_listener { get; init; }
        public clsVictim m_victim { get; init; }
        public string m_szVictimID { get; init; }
        public bool m_bUnixlike { get; init; }

        public List<uint> m_lnPort = new List<uint>();

        public clsAgent(clsListener listener, clsVictim victim, string szVictimID, bool bUnixlike)
        {
            m_listener = listener;
            m_victim = victim;
            m_szVictimID = szVictimID;
            m_bUnixlike = bUnixlike;
        }

        /// <summary>
        /// Send command to agent.
        /// </summary>
        /// <param name="szMsg">Command.</param>
        public void fnSendCommand(string szMsg) => fnSendCommand(szMsg.Split('|'));

        /// <summary>
        /// Send command to agent.
        /// </summary>
        /// <param name="asMsg">Commands.</param>
        public void fnSendCommand(string[] asMsg) => fnSendCommand(asMsg.ToList());

        /// <summary>
        /// Send command to agent.
        /// </summary>
        /// <param name="lsMsg">Commands.</param>
        public void fnSendCommand(List<string> lsMsg)
        {
            m_victim.fnSendCommand(m_szVictimID, lsMsg);
        }
    }
}
