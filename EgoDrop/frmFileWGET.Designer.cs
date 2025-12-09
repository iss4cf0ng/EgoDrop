namespace EgoDrop
{
    partial class frmFileWGET
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
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
            button2 = new Button();
            progressBar1 = new ProgressBar();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(45, 19);
            label1.TabIndex = 0;
            label1.Text = "URL :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 41);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(392, 155);
            textBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(12, 202);
            button1.Name = "button1";
            button1.Size = new Size(193, 63);
            button1.TabIndex = 2;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(211, 202);
            button2.Name = "button2";
            button2.Size = new Size(193, 63);
            button2.TabIndex = 3;
            button2.Text = "Start";
            button2.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 271);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(392, 31);
            progressBar1.TabIndex = 4;
            // 
            // frmFileWGET
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(413, 314);
            Controls.Add(progressBar1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmFileWGET";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmFileWGET";
            FormClosed += frmFileWGET_FormClosed;
            Load += frmFileWGET_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private Button button1;
        private Button button2;
        private ProgressBar progressBar1;
    }
}