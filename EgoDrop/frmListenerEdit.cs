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
            m_bNewListener = string.IsNullOrEmpty(m_listener.szName);

            //Combobox
            foreach (string szProto in Enum.GetNames(typeof(clsSqlite.enListenerProtocol)))
                comboBox1.Items.Add(szProto);

            comboBox1.SelectedIndex = 0;

            if (!m_bNewListener)
            {
                textBox1.Text = m_listener.szName;

                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    string szProto = comboBox1.Items[i].ToString();
                    if (string.IsNullOrEmpty(szProto))
                        break;

                    if (string.Equals(szProto, m_listener.szName))
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }

                numericUpDown1.Value = m_listener.nPort;
                textBox2.Text = m_listener.szDescription;
            }
        }

        private void frmListenerEdit_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var listener = new clsSqlite.stListener(
                textBox1.Text,
                (clsSqlite.enListenerProtocol)Enum.Parse(typeof(clsSqlite.enListenerProtocol), comboBox1.Text),
                (int)numericUpDown1.Value,
                textBox2.Text,
                DateTime.Now
            );

            m_frmListener.m_sqlite.fnSaveListener(listener);
            m_frmListener.fnLoadListener();
        }
    }
}
