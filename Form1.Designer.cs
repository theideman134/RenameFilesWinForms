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
            StartBtn.Location = new Point(209, 180);
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(691, 586);
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
    }
}
