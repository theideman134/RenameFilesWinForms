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

        private static readonly Dictionary<string, string> MakerMap = new Dictionary<string, string>
    {
        { "apple", "Apple" },
        { "apple inc.", "Apple" },
        { "canon inc.", "Canon" },
        { "canon", "Canon" },
        { "nikon corp.", "Nikon" },
        { "nikon corp", "Nikon" },
        { "nikon", "Nikon" },
        { "samsung","Samsung" },
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

            RenameViewModel model = GetViewModel();

            string[] fileNames = System.IO.Directory.GetFiles(model.DirectoryPath);
            
            ProcessFiles(_exiftool, model, fileNames);

            CleanScreen();
            MessageBox.Show("Processing Complete!");
        }

        private void ProcessFiles(IExiftool exiftool, RenameViewModel model, string[] fileNames)
        {
            string latitudeText = model.LocationPresets?.Latitude.ToString() ?? "";
            string longitudeText = model.LocationPresets?.Longitude.ToString() ?? "";

            // 1. DEFAULT BEHAVIOR: Attempt Auto-Detection first

            // Scan the directory to find a file with valid camera metadata
            ExifModel defaultCamera = GetCameraDefault(fileNames, _exiftool, model);

            if (defaultCamera == null)
            {
                MessageBox.Show(
                      "Could not detect camera metadata automatically. Please manually select the camera profile from the dropdown.",
                      "Camera Metadata Missing",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Warning
                );
                return;
            }

            foreach (string file in fileNames)
            {
                string[] validExtensions = null;

                if (model.MP4Only)
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

                    ExifModel curretExifModel = exiftool.GetFileMetadata(fileInfo.FullName);

                    // 1. Ensure we have a safe model instance right off the bat
                    curretExifModel ??= new ExifModel();

                    // 2. Resolve the Make string cleanly
                    if (!string.IsNullOrWhiteSpace(curretExifModel.Make))
                    {
                        curretExifModel.Make = NormalizeMake(curretExifModel.Make);
                    }
                    else
                    {
                        curretExifModel.Make = defaultCamera.Make; // Pulled from your clean default baseline object!
                    }

                    // 3. Resolve the Model string cleanly (using the resolved Make)
                    if (!string.IsNullOrWhiteSpace(curretExifModel.Model))
                    {
                        curretExifModel.Model = NormalizeModel(curretExifModel.Make, curretExifModel.Model);
                    }
                    else
                    {
                        curretExifModel.Model = defaultCamera.Model; // Pulled from your clean default baseline object!
                    }

                    // 1. Extract raw date and check if actual metadata existed
                    DateTime originalFileDate = GetDateTaken(fileInfo.FullName, model.UseUTC, out bool hasMetadata);
                    DateTime mediaDate;

                    //          bool hasGps = HasGpsMetadata(fileInfo.FullName);

                    if (model.UseDate)
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
                    DateTime correctedTime = AdjustMediaTime(mediaDate,model);

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

                        /*
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
                        */
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
                        exifModel.Make = curretExifModel.Make;
                        exifModel.Model = curretExifModel.Model;
                        exifModel.LatitudeText = latitudeText;
                        exifModel.LongitudeText = longitudeText;
                        exifModel.TzOffset = tzOffset;
                        //   exifModel.FileHasCamera = hasCamera;
                        //   exifModel.FileHasGps = hasGps;
                        exifModel.Title = DetermineTitle(curretExifModel.Title,model.Title,model.ForceUpdate, fileInfo.Extension, mediaDate.Year.ToString());

                        exifModel.Description = $"{txtDesc.Text} Original: {backupDateString}, Final: {correctedTime:yyyy-MM-dd HH:mm:ss} (UTC{offsetStr})";

                        _exiftool.WriteMetadataToDisk(exifModel);
                        //     }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to process {fileInfo.Name}: {ex.Message}");
                    }
                }
            }
        }



        private ExifModel GetCameraDefault(string[] fileNames, IExiftool metadataReader, RenameViewModel model)
        {
            // 1. Pass 1: Try to auto-detect from the file array
            foreach (string file in fileNames)
            {
                string extension = Path.GetExtension(file).ToLower();
                string[] validExtensions = new string[] { ".jpg", ".jpeg", ".png", ".heic", ".mpg", ".mp4", ".mts", ".avi" };

                if (validExtensions.Contains(extension))
                {
                    // Clean interface hit—perfect for testing!
                    ExifModel tempExifModel = metadataReader.GetFileMetadata(file);

                    if (tempExifModel != null && !string.IsNullOrEmpty(tempExifModel.Make))
                    {
                        var detectedCamera = new ExifModel();
                        detectedCamera.Make = NormalizeMake(tempExifModel.Make);
                        detectedCamera.Model = NormalizeModel(detectedCamera.Make, tempExifModel.Model); // Fixed standard naming order
                        return detectedCamera;
                    }
                }
            }

            // 2. Fallback: If auto-detect came up dry, try to use the UI model's preset
            if (model.CameraPresets != null && !string.IsNullOrEmpty(model.CameraPresets.Make))
            {
                return new ExifModel
                {
                    Make = model.CameraPresets.Make,
                    Model = model.CameraPresets.Model
                };
            }

            // 3. Complete Failure: Return null to signal to the caller that no camera could be found anywhere
            return null;
        }

        private RenameViewModel GetViewModel()
        {
            RenameViewModel model = new RenameViewModel
            {
                DirectoryPath = dirText.Text,
                Title = txtTitle.Text,
                Description = txtDesc.Text,
                UseUTC = chkIsUTC.Checked,
                UseDate = chkChangeDate.Checked,
                MP4Only = chkMP4only.Checked,
                ForceUpdate = chkForce.Checked,
                CameraPresets = cboCamera.SelectedIndex > 0 ? (CameraPreset?)cboCamera.SelectedItem : null,
                LocationPresets = cboLocations.SelectedIndex > 0 ? (LocationPreset?)cboLocations.SelectedItem : null
                
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

            return model;
        }

        private string DetermineTitle(string currentTitle, string inputTitle, bool forceUpdate, string extension, string year)
        {
            currentTitle = currentTitle?.Trim() ?? "";
            inputTitle = inputTitle?.Trim() ?? "";

            // Determine if the file is a photo or video based on extension
            string[] videoExtensions = { ".mpg", ".mp4", ".mts", ".avi", ".mov" };
            string fileType = videoExtensions.Contains(extension.ToLower()) ? "video" : "photo";

            // Rule 1: If "Force Update" is checked AND we have an input title, use it regardless
            if (forceUpdate && !string.IsNullOrEmpty(inputTitle))
            {
                return inputTitle;
            }

            // Rule 2: If a valid title already exists (and doesn't start with Archived), LEAVE IT
            if (!string.IsNullOrEmpty(currentTitle) && !currentTitle.StartsWith("Archived", StringComparison.OrdinalIgnoreCase))
            {
                return currentTitle;
            }

            // Rule 3: Title is empty OR starts with "Archived" -> check if we can upgrade it with inputTitle
            if (string.IsNullOrEmpty(currentTitle) || currentTitle.StartsWith("Archived", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(inputTitle))
                {
                    return inputTitle;
                }
            }

            // Rule 4: Fallback if nothing else matches (Create standard "Archived (photo/video) Year")
            // If we have a valid year, use it; otherwise fallback to the current year
            string finalYear = !string.IsNullOrEmpty(year) ? year : DateTime.Now.Year.ToString();
            return $"Archived {fileType} {finalYear}";
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


        public DateTime AdjustMediaTime(DateTime originalTime, RenameViewModel model)
        {
            return originalTime
                .AddYears(model.OffSetYears)
                .AddMonths(model.OffSetMonths)
                .AddDays(model.OffSetDays)
                .AddHours(model.OffSetHours)
                .AddMinutes(model.OffSetMinutes)
                .AddSeconds(model.OffSetSeconds);
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

        public List<CameraPreset> GetActiveCameras()
        {
            using (var db = new SqlConnection(@"Server=.\SQLEXPRESS;Database=PhotoDB;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                // Added ORDER BY DisplayName to keep the list alphabetical
                string sql = "SELECT * FROM CameraPresets WHERE IsActive = 1 ORDER BY DisplayName ASC";

                return db.Query<CameraPreset>(sql).ToList();
            }
        }

        private string NormalizeMake(String currentMaker)
        {
            string lookupKey = currentMaker.ToLowerInvariant();
            string standardName = String.Empty;
            // 3. Check if it matches an inconsistent variation
            MakerMap.TryGetValue(lookupKey, out standardName);

            return standardName;
        }

        // Currently Created the Model Normalization but a passthrew since keeping them regardless.  So the method is a placeholder for potential future logic if you want to clean up model variations as well.
        private string NormalizeModel(string currentMake, String currentModel)
        {
           /*
            string lookupKey = currentModel.ToLowerInvariant();
            string standardName = String.Empty;
            // 3. Check if it matches an inconsistent variation
            MakerMap.TryGetValue(lookupKey, out standardName);
           */
            return currentModel;
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
    }
}