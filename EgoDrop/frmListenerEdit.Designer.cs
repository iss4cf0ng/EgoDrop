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
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            comboBox3 = new ComboBox();
            label8 = new Label();
            comboBox2 = new ComboBox();
            label6 = new Label();
            tabPage2 = new TabPage();
            checkBox1 = new CheckBox();
            label9 = new Label();
            textBox4 = new TextBox();
            button3 = new Button();
            button2 = new Button();
            textBox3 = new TextBox();
            label5 = new Label();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            label12 = new Label();
            textBox7 = new TextBox();
            textBox6 = new TextBox();
            label11 = new Label();
            label10 = new Label();
            textBox5 = new TextBox();
            comboBox4 = new ComboBox();
            label7 = new Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage4.SuspendLayout();
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
            textBox1.Size = new Size(237, 27);
            textBox1.TabIndex = 1;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(90, 46);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(237, 27);
            comboBox1.TabIndex = 2;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(90, 79);
            numericUpDown1.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(237, 27);
            numericUpDown1.TabIndex = 3;
            // 
            // button1
            // 
            button1.Location = new Point(11, 340);
            button1.Name = "button1";
            button1.Size = new Size(316, 49);
            button1.TabIndex = 4;
            button1.Text = "Save";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(77, 278);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(250, 56);
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
            label3.Location = new Point(39, 81);
            label3.Name = "label3";
            label3.Size = new Size(45, 19);
            label3.TabIndex = 7;
            label3.Text = "Port :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(28, 281);
            label4.Name = "label4";
            label4.Size = new Size(43, 19);
            label4.TabIndex = 8;
            label4.Text = "Info :";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new Point(12, 112);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(334, 160);
            tabControl1.TabIndex = 9;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(comboBox3);
            tabPage1.Controls.Add(label8);
            tabPage1.Controls.Add(comboBox2);
            tabPage1.Controls.Add(label6);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(326, 128);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "TCP";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // comboBox3
            // 
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.FormattingEnabled = true;
            comboBox3.Items.AddRange(new object[] { "2048", "4096" });
            comboBox3.Location = new Point(74, 36);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(237, 27);
            comboBox3.TabIndex = 12;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(23, 39);
            label8.Name = "label8";
            label8.Size = new Size(45, 19);
            label8.TabIndex = 11;
            label8.Text = "RSA :";
            // 
            // comboBox2
            // 
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "CBC-256" });
            comboBox2.Location = new Point(74, 6);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(237, 27);
            comboBox2.TabIndex = 10;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(25, 9);
            label6.Name = "label6";
            label6.Size = new Size(43, 19);
            label6.TabIndex = 9;
            label6.Text = "AES :";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(checkBox1);
            tabPage2.Controls.Add(label9);
            tabPage2.Controls.Add(textBox4);
            tabPage2.Controls.Add(button3);
            tabPage2.Controls.Add(button2);
            tabPage2.Controls.Add(textBox3);
            tabPage2.Controls.Add(label5);
            tabPage2.Location = new Point(4, 28);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(326, 128);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "TLS";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(243, 41);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(67, 23);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Show";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(8, 42);
            label9.Name = "label9";
            label9.Size = new Size(47, 19);
            label9.TabIndex = 5;
            label9.Text = "Pass :";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(61, 39);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(176, 27);
            textBox4.TabIndex = 4;
            // 
            // button3
            // 
            button3.Location = new Point(12, 72);
            button3.Name = "button3";
            button3.Size = new Size(298, 45);
            button3.TabIndex = 3;
            button3.Text = "Create Cert";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.Location = new Point(243, 6);
            button2.Name = "button2";
            button2.Size = new Size(67, 27);
            button2.TabIndex = 2;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(61, 6);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(176, 27);
            textBox3.TabIndex = 1;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(10, 10);
            label5.Name = "label5";
            label5.Size = new Size(45, 19);
            label5.TabIndex = 0;
            label5.Text = "Cert :";
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(326, 132);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "DNS";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(label12);
            tabPage4.Controls.Add(textBox7);
            tabPage4.Controls.Add(textBox6);
            tabPage4.Controls.Add(label11);
            tabPage4.Controls.Add(label10);
            tabPage4.Controls.Add(textBox5);
            tabPage4.Controls.Add(comboBox4);
            tabPage4.Controls.Add(label7);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(326, 132);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "HTTP";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(20, 5);
            label12.Name = "label12";
            label12.Size = new Size(48, 19);
            label12.TabIndex = 16;
            label12.Text = "Host :";
            // 
            // textBox7
            // 
            textBox7.Location = new Point(74, 2);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(237, 27);
            textBox7.TabIndex = 15;
            // 
            // textBox6
            // 
            textBox6.Location = new Point(74, 101);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(237, 27);
            textBox6.TabIndex = 14;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(31, 104);
            label11.Name = "label11";
            label11.Size = new Size(37, 19);
            label11.TabIndex = 13;
            label11.Text = "UA :";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(21, 71);
            label10.Name = "label10";
            label10.Size = new Size(47, 19);
            label10.TabIndex = 12;
            label10.Text = "Path :";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(74, 68);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(237, 27);
            textBox5.TabIndex = 11;
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(74, 35);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(237, 27);
            comboBox4.TabIndex = 10;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(0, 38);
            label7.Name = "label7";
            label7.Size = new Size(71, 19);
            label7.TabIndex = 9;
            label7.Text = "Method :";
            // 
            // frmListenerEdit
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(350, 407);
            Controls.Add(tabControl1);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(numericUpDown1);
            Controls.Add(label3);
            Controls.Add(button1);
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
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
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
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private Label label5;
        private Button button2;
        private TextBox textBox3;
        private Button button3;
        private ComboBox comboBox3;
        private Label label8;
        private ComboBox comboBox2;
        private Label label6;
        private Label label7;
        private Label label9;
        private TextBox textBox4;
        private CheckBox checkBox1;
        private ComboBox comboBox4;
        private Label label12;
        private TextBox textBox7;
        private TextBox textBox6;
        private Label label11;
        private Label label10;
        private TextBox textBox5;
    }
}