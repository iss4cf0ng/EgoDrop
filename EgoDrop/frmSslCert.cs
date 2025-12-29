using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EgoDrop
{
    public partial class frmSslCert : Form
    {
        public frmSslCert()
        {
            InitializeComponent();
        }

        private void fnSetup()
        {
            textBox1.Text = "CN=MyTestServer";
            comboBox1.SelectedIndex = 0;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            numericUpDown1.Value = 1;
            numericUpDown1.Minimum = 1;
        }

        private void frmSslCert_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string szCertName = textBox1.Text;
            int nLength = int.Parse(comboBox1.Text);
            string szPassword = textBox2.Text;
            int nYear = (int)numericUpDown1.Value;

            using (var rsa = RSA.Create(nLength))
            {
                byte[] abKey = rsa.ExportPkcs8PrivateKey();

                var req = new CertificateRequest(
                    szCertName, 
                    rsa, 
                    HashAlgorithmName.SHA256, 
                    RSASignaturePadding.Pkcs1
                );

                req.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") },
                        false
                    )
                );

                var cert = req.CreateSelfSigned(
                    DateTimeOffset.Now, 
                    DateTimeOffset.Now.AddYears(nYear)
                );

                byte[] abPFX = cert.Export(X509ContentType.Pfx, szPassword);
                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, abPFX);

                    if (File.Exists(sfd.FileName))
                        MessageBox.Show("Created certificate: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Cannot create certificate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); ;
                }
            }
        }
    }
}
