namespace EgoDrop
{
    partial class frmFileTransfer
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
            statusStrip1 = new StatusStrip();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(684, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 405);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(684, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader3, columnHeader4, columnHeader2 });
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(0, 25);
            listView1.Name = "listView1";
            listView1.Size = new Size(684, 380);
            listView1.TabIndex = 2;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "FileName";
            columnHeader1.Width = 200;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Size";
            columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Method";
            columnHeader4.Width = 120;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Progress";
            columnHeader2.Width = 150;
            // 
            // frmFileTransfer
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(684, 427);
            Controls.Add(listView1);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            Margin = new Padding(4);
            Name = "frmFileTransfer";
            Text = "frmFileTransfer";
            Load += frmFileTransfer_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private StatusStrip statusStrip1;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader2;
    }
}