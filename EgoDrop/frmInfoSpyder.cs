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

            }));
        }

        private void fnSetup()
        {

        }

        private void frmInfoSpyder_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
