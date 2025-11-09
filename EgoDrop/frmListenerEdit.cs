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
    public partial class frmListenerEdit : Form
    {
        private frmListener m_frmListener { get; set; }
        private clsSqlite.stListener m_listener { get; set; }

        private bool m_bNewListener { get; set; }

        public frmListenerEdit(frmListener f)
        {
            InitializeComponent();

            m_frmListener = f;
        }

        public frmListenerEdit(frmListener f, clsSqlite.stListener listener)
        {
            InitializeComponent();

            m_frmListener = f;
            m_listener = listener;
        }

        void fnSetup()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            m_bNewListener = string.IsNullOrEmpty(m_listener.szName);

            //Combobox
            foreach (string szProto in Enum.GetNames(typeof(clsSqlite.enListenerProtocol)))
                comboBox1.Items.Add(szProto);
            comboBox1.SelectedIndex = 0;

            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

            foreach (string szMethod in Enum.GetNames(typeof(clsSqlite.enHttpMethod)))
                comboBox4.Items.Add(szMethod);
            comboBox4.SelectedIndex = 0;

            if (!m_bNewListener)
            {
                textBox1.Text = m_listener.szName;
                numericUpDown1.Value = m_listener.nPort;

                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    string szProto = comboBox1.Items[i].ToString();
                    if (string.IsNullOrEmpty(szProto))
                        break;

                    if (string.Equals(szProto, Enum.GetName(m_listener.protoListener)))
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }

                var protocol = (clsSqlite.enListenerProtocol)Enum.Parse(typeof(clsSqlite.enListenerProtocol), comboBox1.Text);
                switch (protocol)
                {
                    case clsSqlite.enListenerProtocol.TCP:
                        
                        break;
                    case clsSqlite.enListenerProtocol.TLS:
                        textBox3.Text = m_listener.szCertPath;
                        textBox4.Text = m_listener.szCertPassword;
                        break;
                    case clsSqlite.enListenerProtocol.DNS:

                        break;
                    case clsSqlite.enListenerProtocol.HTTP:

                        break;
                }

                textBox2.Text = m_listener.szDescription;
            }
        }

        private void frmListenerEdit_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var listeners = m_frmListener.m_dicListener.Select(x => m_frmListener.m_dicListener[x.Key].m_stListener.nPort);
            if (listeners.Contains((int)numericUpDown1.Value))
            {
                MessageBox.Show("This port is in used.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var listener = new clsSqlite.stListener(
                textBox1.Text,
                (clsSqlite.enListenerProtocol)Enum.Parse(typeof(clsSqlite.enListenerProtocol), comboBox1.Text),
                (int)numericUpDown1.Value,
                textBox2.Text,
                DateTime.Now
            );

            switch (listener.protoListener)
            {
                case clsSqlite.enListenerProtocol.TCP:
                    
                    break;
                case clsSqlite.enListenerProtocol.TLS:
                    listener.szCertPath = textBox3.Text;
                    listener.szCertPassword = textBox4.Text;
                    break;
            }

            m_frmListener.m_sqlite.fnSaveListener(listener);
            m_frmListener.fnLoadListener();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = comboBox1.SelectedIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = ofd.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            frmSslCert f = new frmSslCert();
            f.ShowDialog();
        }
    }
}
