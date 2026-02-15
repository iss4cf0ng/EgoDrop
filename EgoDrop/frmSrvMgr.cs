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

        private struct stLinuxServiceInfo
        {
            public string Name { get; set; }
            public string Status { get; set; }
            public string Description { get; set; }
        }

        private struct stWinServiceInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public uint nProcId { get; set; }
            public string Status { get; set; }
            public string ServiceType { get; set; }
            public string Description { get; set; }
            public DateTime InstalledDate { get; set; }
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

        /// <summary>
        /// Send kill service command.
        /// </summary>
        /// <param name="szName"></param>
        private void fnKillService(string szName)
        {

        }

        /// <summary>
        /// Send stop service command.
        /// </summary>
        /// <param name="szName"></param>
        private void fnStopService(string szName)
        {

        }

        /// <summary>
        /// Send continue service command.
        /// </summary>
        /// <param name="szName"></param>
        private void fnResumeService(string szName)
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
                //Unix-like

            }
            else
            {
                //Windows
                listView1.Columns.Add("Name", 80);
                listView1.Columns.Add("DisplayName", 100);
                listView1.Columns.Add("ProcId", 50);
                listView1.Columns.Add("State", 80);
                listView1.Columns.Add("ServiceType", 100);
                listView1.Columns.Add("Description", 150);
                listView1.Columns.Add("InstalledDate", 120);
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
