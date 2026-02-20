namespace EgoDrop
{
    partial class frmFileArchiveCompress
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
            toolStrip1 = new ToolStrip();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            listView1 = new ListView();
            tabPage2 = new TabPage();
            listView2 = new ListView();
            statusStrip1 = new StatusStrip();
            statusStrip2 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            statusStrip1.SuspendLayout();
            statusStrip2.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(415, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 25);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(415, 423);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(statusStrip1);
            tabPage1.Controls.Add(listView1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(407, 391);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Compress";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            listView1.Location = new Point(8, 50);
            listView1.Name = "listView1";
            listView1.Size = new Size(389, 289);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(statusStrip2);
            tabPage2.Controls.Add(listView2);
            tabPage2.Location = new Point(4, 28);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(407, 391);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Extract";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // listView2
            // 
            listView2.Location = new Point(8, 70);
            listView2.Name = "listView2";
            listView2.Size = new Size(391, 261);
            listView2.TabIndex = 1;
            listView2.UseCompatibleStateImageBehavior = false;
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(3, 364);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(401, 24);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusStrip2
            // 
            statusStrip2.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            statusStrip2.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel2 });
            statusStrip2.Location = new Point(3, 364);
            statusStrip2.Name = "statusStrip2";
            statusStrip2.Size = new Size(401, 24);
            statusStrip2.TabIndex = 2;
            statusStrip2.Text = "statusStrip2";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(158, 19);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(158, 19);
            toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // frmFileArchiveCompress
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(415, 448);
            Controls.Add(tabControl1);
            Controls.Add(toolStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            Margin = new Padding(4);
            Name = "frmFileArchiveCompress";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmFileArchiveCompress";
            Load += frmFileArchiveCompress_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            statusStrip2.ResumeLayout(false);
            statusStrip2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ToolStrip toolStrip1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private ListView listView1;
        private ListView listView2;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private StatusStrip statusStrip2;
        private ToolStripStatusLabel toolStripStatusLabel2;
    }
}