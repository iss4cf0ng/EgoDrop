using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EgoDrop
{
    public partial class frmFileWGET : Form
    {
        private clsVictim m_victim { get; init; }

        public frmFileWGET(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        void fnRecv(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim) || lsMsg.Count == 0)
                return;

            if (lsMsg[0] == "file")
            {
                if (lsMsg[1] == "wget")
                {

                }
            }
        }

        void fnSetup()
        {
            m_victim.m_listener.evtReceivedMessage += fnRecv;
        }

        private void frmFileWGET_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmFileWGET_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }
    }
}
