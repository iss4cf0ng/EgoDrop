using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EgoDrop
{
    public partial class frmFileImageSaveAll : Form
    {
        private List<frmFileImage.stImageInfo> m_lsImage { get; init; }
        private string m_szDirName { get; init; }

        public frmFileImageSaveAll(List<frmFileImage.stImageInfo> lsImage, string szDirName)
        {
            InitializeComponent();

            m_lsImage = lsImage;
            m_szDirName = szDirName;

            Text = $"Image[{lsImage.Count}]";
        }

        void fnSave()
        {
            if (!Directory.Exists(m_szDirName))
            {
                DialogResult dr = MessageBox.Show("Directory not found, create a new one?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr != DialogResult.Yes)
                {
                    MessageBox.Show("Task is termianted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                Directory.CreateDirectory(m_szDirName);
            }

            foreach (var image in m_lsImage)
            {
                var img = image.img;
                string szFilePath = Path.Combine(m_szDirName, image.szFileName);

                try
                {
                    using (Bitmap bmp = new Bitmap(img))
                    {
                        bmp.Save(szFilePath, ImageFormat.Png);
                    }

                    richTextBox1.AppendText($"Image is saved: " + szFilePath);
                    richTextBox1.AppendText(Environment.NewLine);

                    toolStripProgressBar1.Increment(1);

                }
                catch (Exception ex)
                {
                    richTextBox1.AppendText($"Save image failed[{szFilePath}]: {ex.Message}");
                    richTextBox1.AppendText(Environment.NewLine);
                }
            }

            if (toolStripProgressBar1.Value == toolStripProgressBar1.Maximum)
            {
                MessageBox.Show("Save images successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (toolStripProgressBar1.Value == 0)
            {
                MessageBox.Show("Save image failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (toolStripProgressBar1.Value < toolStripProgressBar1.Maximum)
            {
                MessageBox.Show(
                    $"Saved [{toolStripProgressBar1.Value}] {(toolStripProgressBar1.Value > 1 ? "images" : "image")} successfully. Error: {toolStripProgressBar1.Maximum - toolStripProgressBar1.Value}",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        void fnSetup()
        {
            toolStripProgressBar1.Maximum = m_lsImage.Count;
            toolStripProgressBar1.Value = 0;

            fnSave();
        }

        private void frmFileImageSaveAll_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = m_szDirName,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            Process.Start(psi);
        }
    }
}
