namespace RenameFilesWinForms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dirText = new TextBox();
            StartBtn = new Button();
            label1 = new Label();
            dtChange = new DateTimePicker();
            txtSecs = new TextBox();
            txtMins = new TextBox();
            txtHour = new TextBox();
            txtDay = new TextBox();
            txtMonth = new TextBox();
            txtYear = new TextBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            SuspendLayout();
            // 
            // dirText
            // 
            dirText.Location = new Point(105, 69);
            dirText.Name = "dirText";
            dirText.Size = new Size(484, 23);
            dirText.TabIndex = 0;
            // 
            // StartBtn
            // 
            StartBtn.Location = new Point(224, 258);
            StartBtn.Name = "StartBtn";
            StartBtn.Size = new Size(199, 76);
            StartBtn.TabIndex = 1;
            StartBtn.Text = "Start";
            StartBtn.UseVisualStyleBackColor = true;
            StartBtn.Click += StartBtn_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(27, 77);
            label1.Name = "label1";
            label1.Size = new Size(58, 15);
            label1.TabIndex = 2;
            label1.Text = "Directory:";
            // 
            // dtChange
            // 
            dtChange.Location = new Point(208, 193);
            dtChange.Name = "dtChange";
            dtChange.Size = new Size(232, 23);
            dtChange.TabIndex = 3;
            // 
            // txtSecs
            // 
            txtSecs.Location = new Point(428, 148);
            txtSecs.Name = "txtSecs";
            txtSecs.Size = new Size(33, 23);
            txtSecs.TabIndex = 4;
            txtSecs.Text = "0";
            // 
            // txtMins
            // 
            txtMins.Location = new Point(374, 148);
            txtMins.Name = "txtMins";
            txtMins.Size = new Size(33, 23);
            txtMins.TabIndex = 5;
            txtMins.Text = "0";
            // 
            // txtHour
            // 
            txtHour.Location = new Point(325, 148);
            txtHour.Name = "txtHour";
            txtHour.Size = new Size(33, 23);
            txtHour.TabIndex = 6;
            txtHour.Text = "0";
            // 
            // txtDay
            // 
            txtDay.Location = new Point(286, 148);
            txtDay.Name = "txtDay";
            txtDay.Size = new Size(33, 23);
            txtDay.TabIndex = 7;
            txtDay.Text = "0";
            // 
            // txtMonth
            // 
            txtMonth.Location = new Point(242, 148);
            txtMonth.Name = "txtMonth";
            txtMonth.Size = new Size(33, 23);
            txtMonth.TabIndex = 8;
            txtMonth.Text = "0";
            // 
            // txtYear
            // 
            txtYear.Location = new Point(198, 148);
            txtYear.Name = "txtYear";
            txtYear.Size = new Size(33, 23);
            txtYear.TabIndex = 9;
            txtYear.Text = "0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(419, 110);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 10;
            label2.Text = "Seconds";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(363, 110);
            label3.Name = "label3";
            label3.Size = new Size(50, 15);
            label3.TabIndex = 11;
            label3.Text = "Minutes";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(324, 110);
            label4.Name = "label4";
            label4.Size = new Size(34, 15);
            label4.TabIndex = 12;
            label4.Text = "Hour";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(286, 110);
            label5.Name = "label5";
            label5.Size = new Size(27, 15);
            label5.TabIndex = 13;
            label5.Text = "Day";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(237, 110);
            label6.Name = "label6";
            label6.Size = new Size(43, 15);
            label6.TabIndex = 14;
            label6.Text = "Month";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(198, 110);
            label7.Name = "label7";
            label7.Size = new Size(29, 15);
            label7.TabIndex = 15;
            label7.Text = "Year";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(691, 586);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtYear);
            Controls.Add(txtMonth);
            Controls.Add(txtDay);
            Controls.Add(txtHour);
            Controls.Add(txtMins);
            Controls.Add(txtSecs);
            Controls.Add(dtChange);
            Controls.Add(label1);
            Controls.Add(StartBtn);
            Controls.Add(dirText);
            Name = "Form1";
            Text = "Rename Files";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox dirText;
        private Button StartBtn;
        private Label label1;
        private DateTimePicker dtChange;
        private TextBox txtSecs;
        private TextBox txtMins;
        private TextBox txtHour;
        private TextBox txtDay;
        private TextBox txtMonth;
        private TextBox txtYear;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
    }
}
