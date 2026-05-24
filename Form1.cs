using Dapper;
using Exiftools;
using MetadataExtractor;
using MetadataExtractor.Formats.Avi;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        private static readonly Dictionary<string, string> MakerMap = new Dictionary<string, string>
    {
        { "canon inc.", "Canon" },
        { "canon", "Canon" },
        { "nikon corp.", "Nikon" },
        { "nikon corp", "Nikon" },
        { "nikon", "Nikon" },
        { "sony corp.", "Sony" },
        { "sony corporation", "Sony" },
        { "sony", "Sony" },
        { "fujifilm co.", "Fujifilm" },
        { "fujifilm", "Fujifilm" }
    };

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(dirText.Text))
            {
                MessageBox.Show("Please select a valid directory.");
                return;
            }

            string[] fileNames = System.IO.Directory.GetFiles(dirText.Text);

            int offsetYears = 0;
            int offsetMonths = 0;
            int offsetDays = 0;
            int offsetHours = 0;
            int offsetMinutes = 0;
            int offsetSeconds = 0;

            string make = "";
            string model = "";
            LocationPreset location = cboLocations.SelectedItem as LocationPreset;
            string latitudeText = location?.Latitude.ToString() ?? "";
            string longitudeText = location?.Longitude.ToString() ?? "";

            bool isTrueUTC = chkIsUTC.Checked; // If checked, we treat original metadata as true UTC and convert to local before applying offsets  

            try
            {
                // --- MASTER TIME OFFSETS ---
                offsetYears = Convert.ToInt32(txtYear.Text);
                offsetMonths = Convert.ToInt32(txtMonth.Text);
                offsetDays = Convert.ToInt32(txtDay.Text);
                offsetHours = Convert.ToInt32(txtHour.Text);
                offsetMinutes = Convert.ToInt32(txtMins.Text);
                offsetSeconds = Convert.ToInt32(txtSecs.Text);
                // ---------------------------
            }
            catch
            {
                MessageBox.Show("Please enter a valid offset time value.");
                return;
            }

            foreach (string file in fileNames)
            {
                string[] validExtensions = null;

                if (chkMP4only.Checked)
                {
                    validExtensions = new string[] { ".mp4" };
                }
                else
                {
                    validExtensions = new string[] { ".jpg", ".jpeg", ".png", ".heic", ".mpg", ".mp4", ".mts", ".avi" };
                }
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Exists && validExtensions.Contains(fileInfo.Extension.ToLower()))
                {
                    // 1. Extract raw date and check if actual metadata existed
                    DateTime originalFileDate = GetDateTaken(fileInfo.FullName, isTrueUTC, out bool hasMetadata);
                    DateTime mediaDate;

                    ExifModel tempExifModel = _exiftool.GetFileMetadata(fileInfo.FullName);
 
                    if (tempExifModel != null)
                    {
                        make = tempExifModel.Make ?? "";
                        if(make != "")
                        {
                            make = MakerFix(make);
                        }
                        model = tempExifModel.Model ?? "";                        
                    }



                    bool hasCamera;
                    if(!string.IsNullOrEmpty(make) || !string.IsNullOrEmpty(model));
                    {
                        hasCamera = true;
                    }
                    bool hasGps = HasGpsMetadata(fileInfo.FullName);

                    if (chkChangeDate.Checked)
                    {
                        // Override Date, keep Time
                        mediaDate = new DateTime(
                            dtChange.Value.Year,
                            dtChange.Value.Month,
                            dtChange.Value.Day,
                            originalFileDate.Hour,
                            originalFileDate.Minute,
                            originalFileDate.Second
                        );
                    }
                    else
                    {
                        mediaDate = originalFileDate;
                    }

                    // 2. Apply offsets
                    DateTime correctedTime = AdjustMediaTime(mediaDate, offsetYears, offsetMonths, offsetDays, offsetHours, offsetMinutes, offsetSeconds);

                    // 3. Setup renaming
                    string newName = correctedTime.ToString("yyyyMMdd_HHmmss");
                    string extension = fileInfo.Extension;
                    string finalPath = Path.Combine(fileInfo.DirectoryName, newName + extension);

                    int count = 1;
                    while (File.Exists(finalPath) && finalPath.ToLower() != fileInfo.FullName.ToLower())
                    {
                        finalPath = Path.Combine(fileInfo.DirectoryName, $"{newName}_{count:D2}{extension}");
                        count++;
                    }

                    try
                    {
                        // Move/Rename
                        if (finalPath.ToLower() != fileInfo.FullName.ToLower())
                        {
                            fileInfo.MoveTo(finalPath);
                        }

                        // Determine if we need to trigger ExifTool
                        bool needsMetadataWrite = !hasMetadata ||
                                                  offsetYears != 0 ||
                                                  offsetMonths != 0 ||
                                                  offsetDays != 0 ||
                                                  offsetHours != 0 ||
                                                  offsetMinutes != 0 ||
                                                  offsetSeconds != 0 ||
                                                  chkChangeDate.Checked ||
                                                  chkMP4only.Checked ||
                                                  chkForce.Checked ||
                                                  cboLocations.SelectedIndex > 0;                                                  ;

                        if (needsMetadataWrite)
                        {
                            string extension2 = Path.GetExtension(finalPath).ToLower();
                            string backupDateString = originalFileDate.ToString("yyyy:MM:dd HH:mm:ss");

                            // Calculate your specific time variables
                            string offsetStr = correctedTime.ToString("%K"); // e.g., -05:00
                            string offsetShort = correctedTime.ToString("zzz"); // Timezone offset for AndroidTimeZone
                            DateTime utcTime = correctedTime.ToUniversalTime();

                            string exifArgs;

                            TimeSpan tzOffset = GetSelectedTimeZoneOffset(correctedTime);

                            ExifModel exifModel = new ExifModel();
                            exifModel.FinalPath = finalPath;
                            exifModel.OriginalFileDate = originalFileDate;
                            exifModel.CorrectedTime = correctedTime;
                            exifModel.Make = make;
                            exifModel.Model = model;
                            exifModel.LatitudeText = latitudeText;
                            exifModel.LongitudeText = longitudeText;
                            exifModel.ForceUpdate = chkForce.Checked;
                            exifModel.TzOffset = tzOffset;
                            exifModel.FileHasCamera = hasCamera;
                            exifModel.FileHasGps = hasGps;

                            _exiftool.RunExifToolFinalSync(exifModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to process {fileInfo.Name}: {ex.Message}");
                    }
                }
            }
            CleanScreen();
            MessageBox.Show("Processing Complete!");
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
        }


        private DateTime AdjustMediaTime(DateTime originalTime, int offsetYear, int offsetMonth, int offsetDays, int offsetHour, int offsetMinute, int offsetSeconds)
        {
            return originalTime.AddYears(offsetYear).AddMonths(offsetMonth).AddDays(offsetDays).AddHours(offsetHour).AddMinutes(offsetMinute).AddSeconds(offsetSeconds);
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
            List<LocationPreset> locations = GetActiveLocations();
            locations.Insert(0, new LocationPreset { LocationID = 0, LocationName = "None" }); // Add a default "None" option at the top
            // 2. Set the DataSource
            cboLocations.DataSource = locations;

            // 3. Tell the box which property to show the user
            cboLocations.DisplayMember = "LocationName";

            // 4. (Optional) Tell the box which property represents the "Value" 
            // Usually the ID, but we will often just grab the whole SelectedItem
            cboLocations.ValueMember = "LocationID";
        }
        private bool HasCameraMetadata(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // 1. Check Photo Camera Data (Exif IFD0)
                var ifd0Directory = directories.OfType<MetadataExtractor.Formats.Exif.ExifIfd0Directory>().FirstOrDefault();
                if (ifd0Directory != null)
                {
                    bool hasMake = ifd0Directory.ContainsTag(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagMake);
                    bool hasModel = ifd0Directory.ContainsTag(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagModel);
                    if (hasMake || hasModel) return true;
                }

                // 2. Check Video Camera Data (QuickTime Metadata Keys)
                var qtMetaDirectory = directories.OfType<MetadataExtractor.Formats.QuickTime.QuickTimeMetadataHeaderDirectory>().FirstOrDefault();
                if (qtMetaDirectory != null)
                {
                    bool hasTrackModel = qtMetaDirectory.ContainsTag(MetadataExtractor.Formats.QuickTime.QuickTimeMetadataHeaderDirectory.TagModel);
                    if (hasTrackModel) return true;
                }
            }
            catch
            {
                // If file is unreadable, default to false so processing can proceed safely
            }

            return false;
        }

        private bool HasGpsMetadata(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // 1. Check Photo GPS Data
                var gpsDirectory = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();
                if (gpsDirectory != null && gpsDirectory.ContainsTag(MetadataExtractor.Formats.Exif.GpsDirectory.TagLatitude))
                {
                    return true;
                }

                // 2. Check Video GPS Data (QuickTime Locations)
                var qtMetaDirectory = directories.OfType<MetadataExtractor.Formats.QuickTime.QuickTimeMetadataHeaderDirectory>().FirstOrDefault();
                if (qtMetaDirectory != null)
                {
                    bool hasLocation = qtMetaDirectory.ContainsTag(MetadataExtractor.Formats.QuickTime.QuickTimeMetadataHeaderDirectory.TagLocationName);
                    if (hasLocation) return true;
                }
            }
            catch
            {
                // If file is unreadable, default to false
            }

            return false;
        }

        // 1. Update the signature to accept 'TimeSpan tzOffset' at the end
        // 1. Update the signature to accept 'TimeSpan tzOffset' at the end
    

        private string NewFileName(DateTime time)
        {
            // Format: YYYYMMDD_HHMMSS
            return time.ToString("yyyyMMdd_HHmmss");
        }

        private static string AddedFileExtention(FileInfo fileInfo, string fileNameNoExt)
        {
            string ext = fileInfo.Extension.ToLower();

            // Apply your specific extension formatting rules
            string normalizedExt = ext switch
            {
                ".jpeg" => ".jpg",
                ".mov" => ".MOV",
                ".mts" => ".MTS",
                _ => ext // Keeps others like .mp4, .png, .avi as lowercase
            };

            return fileNameNoExt + normalizedExt;
        }

        private TimeSpan GetSelectedTimeZoneOffset(DateTime mediaDate)
        {
            TimeZoneInfo tz;

            // Determine which timezone to use based on the radio buttons
            if (rdoEastern?.Checked == true)
                tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            else if (rdoMountain?.Checked == true)
                tz = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
            else if (rdoPacific?.Checked == true)
                tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            else
                tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); // Default

            // Get the UTC offset for the specific date (this automatically handles Daylight Saving Time)
            return tz.GetUtcOffset(mediaDate);
        }

        static DateTime GetDateTaken(string filePath, bool isTrueUTC, out bool hasMetadata)
        {
            hasMetadata = false;
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // 1. Filename remains the Gold Standard for local time
            if (DateTime.TryParseExact(fileName, "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.AssumeLocal, out DateTime parsedDate))
            {
                hasMetadata = true;
                return DateTime.SpecifyKind(parsedDate, DateTimeKind.Local);
            }

            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // --- NEW: CHECK ANDROID/APPLE KEYS FIRST ---
                // These tags (Keys:CreationDate) usually contain the offset, making them bulletproof.
                var keysDirectory = directories.OfType<QuickTimeMetadataHeaderDirectory>().FirstOrDefault();
                if (keysDirectory != null && keysDirectory.TryGetDateTime(QuickTimeMetadataHeaderDirectory.TagCreationDate, out DateTime keysDate))
                {
                    hasMetadata = true;
                    // If the key was read correctly, MetadataExtractor often handles the offset conversion.
                    // We force it to Local to ensure your downstream offset math (15/20 hrs) is consistent.
                    return keysDate.ToLocalTime();
                }

                // --- PHOTO LOGIC ---
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (subIfdDirectory != null && subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime exifDate))
                {
                    hasMetadata = true;
                    return DateTime.SpecifyKind(exifDate, DateTimeKind.Local);
                }

                // --- VIDEO LOGIC (Fallback for older files without 'Keys') ---
                var movDirectory = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                if (movDirectory != null && movDirectory.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out DateTime movDate))
                {
                    hasMetadata = true;
                    // If we are here, we don't have an offset key, so we use your batch toggle
                    return isTrueUTC ? DateTime.SpecifyKind(movDate, DateTimeKind.Utc).ToLocalTime()
                                     : DateTime.SpecifyKind(movDate, DateTimeKind.Local);
                }
            }
            catch { /* Metadata error */ }

            return File.GetLastWriteTime(filePath);
        }

        public List<LocationPreset> GetActiveLocations()
        {
            using (var db = new SqlConnection(@"Server=.\SQLEXPRESS;Database=PhotoDB;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                // Added ORDER BY LocationName to keep the list alphabetical
                string sql = "SELECT * FROM LocationPresets WHERE IsActive = 1 ORDER BY LocationName ASC";

                return db.Query<LocationPreset>(sql).ToList();
            }
        }

        private string MakerFix(String currentMaker)
        {
            string lookupKey = currentMaker.ToLowerInvariant();
            string standardName = String.Empty;
            // 3. Check if it matches an inconsistent variation
            MakerMap.TryGetValue(lookupKey, out standardName);

            return standardName;
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
        }

        private void cboLocations_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}