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
        public clsAgent m_agent { get; init; }

        public frmSrvMgr(clsAgent agent)
        {
            InitializeComponent();

            m_agent = agent;
        }

        private List<string> m_lsLinuxColumns = new List<string>()
        {

        };
        private List<string> m_lsWinColumns = new List<string>()
        {

        };

        private struct stLinuxServiceInfo
        {

        }

        private struct stWinServiceInfo
        {

        }

        private void fnRecv(clsListener listener, clsVictim victim, string szSrcVictimID, List<string> lsMsg)
        {
            if (!string.Equals(m_agent.m_szVictimID, szSrcVictimID))
                return;

            Invoke(new Action(() =>
            {
                if (lsMsg[0] == "srv")
                {
                    if (lsMsg[1] == "ls")
                    {
                        if (m_agent.m_bUnixlike)
                        {

                        }

                        toolStripStatusLabel1.Text = $"Service[{listView1.Items.Count}]";
                    }
                }
            }));
        }

        private void fnGetServices()
        {
            toolStripStatusLabel1.Text = "Loading...";

            listView1.Items.Clear();
            m_agent.fnSendCommand("srv|ls");
        }


        private void fnKillService(string szName)
        {

        }

        private void fnStopService(string szName)
        {

        }

        private void fnContiService(string szName)
        {

        }

        void fnSetup()
        {
            //Controls
            toolStripStatusLabel1.Text = "Loading...";
            listView1.View = View.Details;
            listView1.Items.Clear();
            listView1.Columns.Clear();

            if (m_agent.m_bUnixlike)
            {
                for (int i = 0; i < m_lsLinuxColumns.Count; i++)
                {
                    listView1.Columns.Add(m_lsLinuxColumns[i]);
                    listView1.Columns[i].Width = 200;
                }
            }
            else
            {
                for (int i = 0; i < m_lsWinColumns.Count; i++)
                {
                    listView1.Columns.Add(m_lsWinColumns[i]);
                    listView1.Columns[i].Width = 200;
                }
            }

            m_agent.m_victim.m_listener.evtReceivedMessage += fnRecv;

            fnGetServices();
        }

        private void frmSrvMgr_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmSrvMgr_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_agent.m_victim.m_listener.evtReceivedMessage -= fnRecv;
        }
    }
}
