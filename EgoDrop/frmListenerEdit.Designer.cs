namespace EgoDrop
{
    partial class frmListenerEdit
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
            comboBox1 = new ComboBox();
            numericUpDown1 = new NumericUpDown();
            button1 = new Button();
            textBox2 = new TextBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(28, 16);
            label1.Name = "label1";
            label1.Size = new Size(58, 19);
            label1.TabIndex = 0;
            label1.Text = "Name :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(90, 13);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(238, 27);
            textBox1.TabIndex = 1;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(90, 46);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(238, 27);
            comboBox1.TabIndex = 2;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(91, 79);
            numericUpDown1.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(237, 27);
            numericUpDown1.TabIndex = 3;
            // 
            // button1
            // 
            button1.Location = new Point(12, 174);
            button1.Name = "button1";
            button1.Size = new Size(316, 49);
            button1.TabIndex = 4;
            button1.Text = "Save";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(91, 112);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(237, 56);
            textBox2.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 49);
            label2.Name = "label2";
            label2.Size = new Size(74, 19);
            label2.TabIndex = 6;
            label2.Text = "Protocol :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(40, 81);
            label3.Name = "label3";
            label3.Size = new Size(45, 19);
            label3.TabIndex = 7;
            label3.Text = "Port :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(42, 112);
            label4.Name = "label4";
            label4.Size = new Size(43, 19);
            label4.TabIndex = 8;
            label4.Text = "Info :";
            // 
            // frmListenerEdit
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(346, 235);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(button1);
            Controls.Add(numericUpDown1);
            Controls.Add(comboBox1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmListenerEdit";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmListenerEdit";
            Load += frmListenerEdit_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private ComboBox comboBox1;
        private NumericUpDown numericUpDown1;
        private Button button1;
        private TextBox textBox2;
        private Label label2;
        private Label label3;
        private Label label4;
    }
}