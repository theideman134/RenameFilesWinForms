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
            chkChangeDate = new CheckBox();
            chkIsUTC = new CheckBox();
            chkMP4only = new CheckBox();
            chkForce = new CheckBox();
            cboLocations = new ComboBox();
            cboCamera = new ComboBox();
            lblTitle = new Label();
            txtTitle = new TextBox();
            lblDesc = new Label();
            txtDesc = new TextBox();
            rdoCentral = new RadioButton();
            rdoMountain = new RadioButton();
            rdoPacific = new RadioButton();
            rdoEastern = new RadioButton();
            chkForceCamera = new CheckBox();
            chkForceGPS = new CheckBox();
            chkForceTitleUpdate = new CheckBox();
            label8 = new Label();
            label9 = new Label();
            lblCity = new Label();
            lblState = new Label();
            chkInActive = new CheckBox();
            txtCity = new TextBox();
            txtState = new TextBox();
            btnUpdateGPS = new Button();
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
            StartBtn.Location = new Point(237, 498);
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
            // chkChangeDate
            // 
            chkChangeDate.AutoSize = true;
            chkChangeDate.Location = new Point(456, 193);
            chkChangeDate.Name = "chkChangeDate";
            chkChangeDate.Size = new Size(72, 19);
            chkChangeDate.TabIndex = 16;
            chkChangeDate.Text = "Use Date";
            chkChangeDate.UseVisualStyleBackColor = true;
            // 
            // chkIsUTC
            // 
            chkIsUTC.AutoSize = true;
            chkIsUTC.Location = new Point(490, 148);
            chkIsUTC.Name = "chkIsUTC";
            chkIsUTC.Size = new Size(70, 19);
            chkIsUTC.TabIndex = 17;
            chkIsUTC.Text = "Use UTC";
            chkIsUTC.UseVisualStyleBackColor = true;
            chkIsUTC.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // chkMP4only
            // 
            chkMP4only.AutoSize = true;
            chkMP4only.Location = new Point(574, 152);
            chkMP4only.Name = "chkMP4only";
            chkMP4only.Size = new Size(76, 19);
            chkMP4only.TabIndex = 18;
            chkMP4only.Text = "MP4 only";
            chkMP4only.UseVisualStyleBackColor = true;
            chkMP4only.CheckedChanged += checkBox1_CheckedChanged_1;
            // 
            // chkForce
            // 
            chkForce.AutoSize = true;
            chkForce.Location = new Point(574, 177);
            chkForce.Name = "chkForce";
            chkForce.Size = new Size(96, 19);
            chkForce.TabIndex = 19;
            chkForce.Text = "Force Update";
            chkForce.UseVisualStyleBackColor = true;
            // 
            // cboLocations
            // 
            cboLocations.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboLocations.AutoCompleteSource = AutoCompleteSource.ListItems;
            cboLocations.FormattingEnabled = true;
            cboLocations.Location = new Point(176, 419);
            cboLocations.Name = "cboLocations";
            cboLocations.Size = new Size(352, 23);
            cboLocations.TabIndex = 28;
            cboLocations.SelectedIndexChanged += cboLocations_SelectedIndexChanged;
            // 
            // cboCamera
            // 
            cboCamera.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboCamera.AutoCompleteSource = AutoCompleteSource.ListItems;
            cboCamera.FormattingEnabled = true;
            cboCamera.Location = new Point(176, 458);
            cboCamera.Name = "cboCamera";
            cboCamera.Size = new Size(352, 23);
            cboCamera.TabIndex = 29;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(12, 243);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(33, 15);
            lblTitle.TabIndex = 31;
            lblTitle.Text = "Title:";
            // 
            // txtTitle
            // 
            txtTitle.Location = new Point(64, 243);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(541, 23);
            txtTitle.TabIndex = 30;
            // 
            // lblDesc
            // 
            lblDesc.AutoSize = true;
            lblDesc.Location = new Point(3, 286);
            lblDesc.Name = "lblDesc";
            lblDesc.Size = new Size(67, 15);
            lblDesc.TabIndex = 33;
            lblDesc.Text = "Description";
            // 
            // txtDesc
            // 
            txtDesc.Location = new Point(76, 286);
            txtDesc.Multiline = true;
            txtDesc.Name = "txtDesc";
            txtDesc.Size = new Size(529, 51);
            txtDesc.TabIndex = 32;
            // 
            // rdoCentral
            // 
            rdoCentral.AutoSize = true;
            rdoCentral.Location = new Point(27, 133);
            rdoCentral.Name = "rdoCentral";
            rdoCentral.Size = new Size(63, 19);
            rdoCentral.TabIndex = 34;
            rdoCentral.TabStop = true;
            rdoCentral.Text = "Central";
            rdoCentral.UseVisualStyleBackColor = true;
            // 
            // rdoMountain
            // 
            rdoMountain.AutoSize = true;
            rdoMountain.Location = new Point(27, 158);
            rdoMountain.Name = "rdoMountain";
            rdoMountain.Size = new Size(77, 19);
            rdoMountain.TabIndex = 35;
            rdoMountain.TabStop = true;
            rdoMountain.Text = "Mountain";
            rdoMountain.UseVisualStyleBackColor = true;
            // 
            // rdoPacific
            // 
            rdoPacific.AutoSize = true;
            rdoPacific.Location = new Point(27, 183);
            rdoPacific.Name = "rdoPacific";
            rdoPacific.Size = new Size(60, 19);
            rdoPacific.TabIndex = 36;
            rdoPacific.TabStop = true;
            rdoPacific.Text = "Pacific";
            rdoPacific.UseVisualStyleBackColor = true;
            // 
            // rdoEastern
            // 
            rdoEastern.AutoSize = true;
            rdoEastern.Location = new Point(27, 108);
            rdoEastern.Name = "rdoEastern";
            rdoEastern.Size = new Size(63, 19);
            rdoEastern.TabIndex = 37;
            rdoEastern.TabStop = true;
            rdoEastern.Text = "Eastern";
            rdoEastern.UseVisualStyleBackColor = true;
            // 
            // chkForceCamera
            // 
            chkForceCamera.AutoSize = true;
            chkForceCamera.Location = new Point(544, 460);
            chkForceCamera.Name = "chkForceCamera";
            chkForceCamera.Size = new Size(99, 19);
            chkForceCamera.TabIndex = 38;
            chkForceCamera.Text = "Force Camera";
            chkForceCamera.UseVisualStyleBackColor = true;
            // 
            // chkForceGPS
            // 
            chkForceGPS.AutoSize = true;
            chkForceGPS.Location = new Point(544, 423);
            chkForceGPS.Name = "chkForceGPS";
            chkForceGPS.Size = new Size(79, 19);
            chkForceGPS.TabIndex = 39;
            chkForceGPS.Text = "Force GPS";
            chkForceGPS.UseVisualStyleBackColor = true;
            // 
            // chkForceTitleUpdate
            // 
            chkForceTitleUpdate.AutoSize = true;
            chkForceTitleUpdate.Location = new Point(574, 202);
            chkForceTitleUpdate.Name = "chkForceTitleUpdate";
            chkForceTitleUpdate.Size = new Size(111, 19);
            chkForceTitleUpdate.TabIndex = 40;
            chkForceTitleUpdate.Text = "Force Title/Desc";
            chkForceTitleUpdate.UseVisualStyleBackColor = true;
            chkForceTitleUpdate.CheckedChanged += checkBox3_CheckedChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(105, 427);
            label8.Name = "label8";
            label8.Size = new Size(28, 15);
            label8.TabIndex = 41;
            label8.Text = "GPS";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(105, 466);
            label9.Name = "label9";
            label9.Size = new Size(48, 15);
            label9.TabIndex = 42;
            label9.Text = "Camera";
            // 
            // lblCity
            // 
            lblCity.AutoSize = true;
            lblCity.Location = new Point(105, 391);
            lblCity.Name = "lblCity";
            lblCity.Size = new Size(28, 15);
            lblCity.TabIndex = 43;
            lblCity.Text = "City";
            // 
            // lblState
            // 
            lblState.AutoSize = true;
            lblState.Location = new Point(387, 392);
            lblState.Name = "lblState";
            lblState.Size = new Size(33, 15);
            lblState.TabIndex = 44;
            lblState.Text = "State";
            // 
            // chkInActive
            // 
            chkInActive.AutoSize = true;
            chkInActive.Location = new Point(481, 390);
            chkInActive.Name = "chkInActive";
            chkInActive.Size = new Size(67, 19);
            chkInActive.TabIndex = 45;
            chkInActive.Text = "Inactive";
            chkInActive.UseVisualStyleBackColor = true;
            // 
            // txtCity
            // 
            txtCity.Location = new Point(176, 388);
            txtCity.Name = "txtCity";
            txtCity.Size = new Size(205, 23);
            txtCity.TabIndex = 46;
            // 
            // txtState
            // 
            txtState.Location = new Point(426, 388);
            txtState.MaxLength = 2;
            txtState.Name = "txtState";
            txtState.Size = new Size(49, 23);
            txtState.TabIndex = 47;
            // 
            // btnUpdateGPS
            // 
            btnUpdateGPS.Location = new Point(546, 387);
            btnUpdateGPS.Name = "btnUpdateGPS";
            btnUpdateGPS.Size = new Size(100, 25);
            btnUpdateGPS.TabIndex = 48;
            btnUpdateGPS.Text = "Update GPS";
            btnUpdateGPS.UseVisualStyleBackColor = true;
            btnUpdateGPS.Click += btnUpdateGPS_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(691, 586);
            Controls.Add(btnUpdateGPS);
            Controls.Add(txtState);
            Controls.Add(txtCity);
            Controls.Add(chkInActive);
            Controls.Add(lblState);
            Controls.Add(lblCity);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(chkForceTitleUpdate);
            Controls.Add(chkForceGPS);
            Controls.Add(chkForceCamera);
            Controls.Add(rdoEastern);
            Controls.Add(rdoPacific);
            Controls.Add(rdoMountain);
            Controls.Add(rdoCentral);
            Controls.Add(lblDesc);
            Controls.Add(txtDesc);
            Controls.Add(lblTitle);
            Controls.Add(txtTitle);
            Controls.Add(cboCamera);
            Controls.Add(cboLocations);
            Controls.Add(chkForce);
            Controls.Add(chkMP4only);
            Controls.Add(chkIsUTC);
            Controls.Add(chkChangeDate);
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
            Load += Form1_Load;
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
        private CheckBox chkChangeDate;
        private CheckBox chkIsUTC;
        private CheckBox chkMP4only;
        private CheckBox chkForce;
        private ComboBox comboBox1;
        private ComboBox cboLocations;
        private ComboBox cboCamera;
        private Label lblTitle;
        private TextBox txtTitle;
        private Label lblDesc;
        private TextBox txtDesc;
        private RadioButton rdoCentral;
        private RadioButton rdoMountain;
        private RadioButton rdoPacific;
        private RadioButton rdoEastern;
        private CheckBox chkForceCamera;
        private CheckBox chkForceGPS;
        private CheckBox chkForceTitleUpdate;
        private Label label8;
        private Label label9;
        private Label lblCity;
        private Label lblState;
        private CheckBox chkInActive;
        private TextBox txtCity;
        private TextBox txtState;
        private Button btnUpdateGPS;
    }
}
