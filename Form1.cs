using Dapper;
using Exiftools;
using MediaArchiver;
using MetadataExtractor;
using MetadataExtractor.Formats.Avi;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using Microsoft.Data.SqlClient;
using RenameFilesWinForms;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Forms;

namespace RenameFilesWinForms
{
    public partial class Form1 : Form
    {
        private readonly IExiftool _exiftool;

        public Form1(IExiftool exiftool)
        {
            InitializeComponent();
            _exiftool = exiftool;
            rdoCentral.Checked = true;

        }
        // E:\exiftool\exiftool.exe -make -model -time:all -G1 -a -s "C:\Path\To\Your\ProcessedFile.mp4"

        // E:\exiftool\exiftool.exe -make -model -title -description -time:all -G1 -a -s "E:\picture3\2011-02-02 - Day after big storm - Copy\20110202_120246.jpg"

        //E:\exiftool\exiftool.exe -make -model -title -description -n -GPSLatitude -GPSlongitude -time:all -G1 -a -s "E:\mobile\2026-04\20260417_142152.heic"
        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(dirText.Text))
            {
                MessageBox.Show("Please select a valid directory.");
                return;
            }
            try
            {
                RenameViewModel model = GetViewModel();

                string[] fileNames = System.IO.Directory.GetFiles(model.DirectoryPath);

                ArchivalEngine archivalEngine = new ArchivalEngine(_exiftool);
                archivalEngine.ProcessFiles(_exiftool, model, fileNames);
                CleanScreen();
                MessageBox.Show("Processing Complete!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

        }





        private RenameViewModel GetViewModel()
        {
            RenameViewModel model = new RenameViewModel
            {
                DirectoryPath = dirText.Text,
                AltName = txtAltName.Text,
                UseUTC = chkIsUTC.Checked,
                UseDate = chkChangeDate.Checked,
                MP4Only = chkMP4only.Checked,
                ForceUpdate = chkForce.Checked,
                CameraPresets = cboCamera.SelectedIndex > 0 ? (CameraPreset?)cboCamera.SelectedItem : null,
                LocationPresets = cboLocations.SelectedIndex > 0 ? (LocationPreset?)cboLocations.SelectedItem : null,
                ForceTitleDescUpdate = chkForceTitleUpdate.Checked,
                ForceGPSUpdate = chkForceGPS.Checked,
                ForceCameraUpdate = chkForceCamera.Checked.
                KeepOrginalFileName = chkKeepOriginal.Checked

            };

            try
            {
                // --- MASTER TIME OFFSETS ---
                model.OffSetYears = Convert.ToInt32(txtYear.Text);
                model.OffSetMonths = Convert.ToInt32(txtMonth.Text);
                model.OffSetDays = Convert.ToInt32(txtDay.Text);
                model.OffSetHours = Convert.ToInt32(txtHour.Text);
                model.OffSetMinutes = Convert.ToInt32(txtMins.Text);
                model.OffSetSeconds = Convert.ToInt32(txtSecs.Text);
                // ---------------------------
            }
            catch
            {
                MessageBox.Show("Please enter a valid offset time value.");
                return null;
            }

            if (model.UseDate)
                model.ManualDateTime = model.UseDate ? (DateTime?)dtChange.Value : null;
            else
                model.ManualDateTime = null;

            model.TimeZone = rdoEastern.Checked ? "Eastern Standard Time" :
                             rdoMountain.Checked ? "Mountain Standard Time" :
                             rdoPacific.Checked ? "Pacific Standard Time" : "Central Standard Time";

            return model;
        }


        private void CleanScreen()
        {
            dirText.Text = "";
            dirText.Update();
            txtYear.Text = "0";
            txtYear.Update();
            txtMonth.Text = "0";
            txtMonth.Update();
            txtDay.Text = "0";
            txtDay.Update();
            txtHour.Text = "0";
            txtHour.Update();
            txtMins.Text = "0";
            txtMins.Update();
            txtSecs.Text = "0";
            txtSecs.Update();
            dtChange.Value = DateTime.Today;
            dtChange.Update();
            chkChangeDate.Checked = false;
            chkChangeDate.Update();
            chkForceGPS.Checked = false;
            chkForceGPS.Update();
            chkForceTitleUpdate.Checked = false;
            chkForceTitleUpdate.Update();
            chkForceCamera.Checked = false;
            chkForceCamera.Update();
            cboCamera.SelectedIndex = 0;
            cboLocations.SelectedIndex = 0;
            chkKeepOriginal.Checked = false;
            chkKeepOriginal.Update();

        }




        public static string GetNewFileName(FileInfo file, DateTime? metaDate)
        {
            // 1. Choose the most reliable date available
            // Priority: Metadata Date (from ExifTool/Handbrake) > Creation Date > Last Modified
            DateTime finalDate = metaDate ?? (file.CreationTime < file.LastWriteTime ? file.CreationTime : file.LastWriteTime);

            // 2. Format the date (YYYY-MM-DD_HH-mm-ss)
            string dateString = finalDate.ToString("yyyyMMdd_HHmmss");

            // 3. Get the extension (includes the dot, e.g., ".mov")
            string extension = file.Extension;

            // 4. Combine them
            string newName = $"{dateString}{extension}";

            // Handle potential collisions (same second, multiple shots)
            int count = 1;
            string finalPath = Path.Combine(file.DirectoryName, newName);

            while (File.Exists(finalPath))
            {
                newName = $"{dateString}_{count}{extension}";
                finalPath = Path.Combine(file.DirectoryName, newName);
                count++;
            }

            return newName;
        }

        private void LoadLocationComboBox()
        {
            // 1. Get the list from your SQL DB (ordered alphabetically as requested)
            List<LocationPreset> locations = GetFilteredLocations(txtCity.Text, txtState.Text, chkInActive.Checked);
            locations.Insert(0, new LocationPreset { LocationID = 0, LocationName = "None" }); // Add a default "None" option at the top
            // 2. Set the DataSource
            cboLocations.DataSource = locations;

            // 3. Tell the box which property to show the user
            cboLocations.DisplayMember = "LocationName";

            // 4. (Optional) Tell the box which property represents the "Value" 
            // Usually the ID, but we will often just grab the whole SelectedItem
            cboLocations.ValueMember = "LocationID";
        }

        private void LoadCameraComboBox()
        {
            // 1. Get the list from your SQL DB (ordered alphabetically as requested)
            List<CameraPreset> cameras = GetActiveCameras();
            cameras.Insert(0, new CameraPreset { CameraID = 0, DisplayName = "Unknown" }); // Add a default "None" option at the top
            // 2. Set the DataSource
            cboCamera.DataSource = cameras;

            // 3. Tell the box which property to show the user
            cboCamera.DisplayMember = "DisplayName";

            // 4. (Optional) Tell the box which property represents the "Value" 
            // Usually the ID, but we will often just grab the whole SelectedItem
            cboCamera.ValueMember = "CameraID";
        }

        public List<LocationPreset> GetFilteredLocations(string city = null, string state = null, bool includeInactive = false)
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=PhotoDB;Trusted_Connection=True;TrustServerCertificate=True;";
            using (var db = new SqlConnection(connectionString))
            {
                // We use a CASE statement to append '[HISTORIC]' or '[CLOSED]' to the name if IsActive = 0
                string sql = @"
            SELECT *
            FROM dbo.Locations 
            WHERE 1=1";
                if (!includeInactive) sql += " AND IsActive = 1";
                if (!string.IsNullOrEmpty(state)) sql += " AND State = @State";
                if (!string.IsNullOrEmpty(city)) sql += " AND City = @City";

                sql += " ORDER BY IsActive DESC, LocationName ASC"; // Keeps active ones at the top, or pure alphabetical

                return db.Query<LocationPreset>(sql, new { State = state, City = city }).ToList();
            }
        }


        public List<CameraPreset> GetActiveCameras()
        {
            using (var db = new SqlConnection(@"Server=.\SQLEXPRESS;Database=PhotoDB;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                // Added ORDER BY DisplayName to keep the list alphabetical
                string sql = "SELECT * FROM Cameras WHERE IsActive = 1 ORDER BY DisplayName ASC";

                return db.Query<CameraPreset>(sql).ToList();
            }
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadLocationComboBox();
            LoadCameraComboBox();
        }

        private void cboLocations_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnUpdateGPS_Click(object sender, EventArgs e)
        {
            LoadLocationComboBox();
        }

    }
}
