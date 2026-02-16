namespace EgoDrop
{
    partial class frmHelper
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            textBox1 = new TextBox();
            richTextBox1 = new RichTextBox();
            toolStripMenuItem1 = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1 });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(8, 3, 0, 3);
            menuStrip1.Size = new Size(729, 29);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Top;
            textBox1.Location = new Point(0, 29);
            textBox1.Margin = new Padding(4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(729, 27);
            textBox1.TabIndex = 1;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(0, 56);
            richTextBox1.Margin = new Padding(4);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(729, 372);
            richTextBox1.TabIndex = 2;
            richTextBox1.Text = "";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(74, 23);
            toolStripMenuItem1.Text = "Refresh";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // frmHelper
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(729, 428);
            Controls.Add(richTextBox1);
            Controls.Add(textBox1);
            Controls.Add(menuStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4);
            Name = "frmHelper";
            Text = "frmHelper";
            Load += frmHelper_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private TextBox textBox1;
        private RichTextBox richTextBox1;
        private ToolStripMenuItem toolStripMenuItem1;
    }
}