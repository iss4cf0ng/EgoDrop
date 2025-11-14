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
    public partial class frmProcMgr : Form
    {
        private clsVictim m_victim { get; set; }

        public frmProcMgr(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim))
                return;

            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "proc")
                {
                    if (lsMsg[1] == "ls")
                    {
                        
                    }
                }
            }));
        }

        private void fnGetProcesses()
        {

        }

        void fnSetup()
        {
            m_victim.m_listener.evtReceivedMessage += fnRecv;
        }

        private void frmProcMgr_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmProcMgr_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }
    }
}
