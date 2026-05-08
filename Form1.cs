using MetadataExtractor;
using MetadataExtractor.Formats.Avi;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
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
        public Form1()
        {
            InitializeComponent();
        }
        // E:\exiftool\exiftool.exe -time:all -G1 -a -s "C:\Path\To\Your\ProcessedFile.mp4"

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
                string[] validExtensions = { ".jpg", ".jpeg", ".png", ".heic", ".mpg", ".mp4", ".mts", ".avi" };
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Exists && validExtensions.Contains(fileInfo.Extension.ToLower()))
                {
                    // 1. Extract raw date and check if actual metadata existed
                    DateTime originalFileDate = GetDateTaken(fileInfo.FullName,isTrueUTC, out bool hasMetadata);
                    DateTime mediaDate;

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
                                                  dtChange.Value.Date != DateTime.Today;

                        if (needsMetadataWrite)
                        {
                            string extension2 = Path.GetExtension(finalPath).ToLower();
                            string backupDateString = originalFileDate.ToString("yyyy:MM:dd HH:mm:ss");

                            // Calculate your specific time variables
                            string offsetStr = correctedTime.ToString("%K"); // e.g., -05:00
                            string offsetShort = correctedTime.ToString("zzz"); // Timezone offset for AndroidTimeZone
                            DateTime utcTime = correctedTime.ToUniversalTime();

                            string exifArgs;


                            /* exifArgs = $"-QuickTime:CreateDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                       $"-QuickTime:ModifyDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                       $"-Keys:CreationDate=\"{correctedTime:yyyy:MM:dd HH:mm:ss}{offsetStr}\" " +
                                       $"-Keys:AndroidTimeZone=\"{offsetShort}\" " +
                                       $"-Title=\"Archived Video - {backupDateString:yyyy}\" " +
                                       $"-Description=\"OriginalDateBackup: {backupDateString}. Local Time: {correctedTime:t}\" " +
                                       $"-P -overwrite_original \"{finalPath}\"";
                        */

                            if (extension2 == ".mp4" || extension2 == ".mov")
                            {
                                // Your specific MP4 logic
                                exifArgs = $"-QuickTime:CreateDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-QuickTime:ModifyDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-TrackCreateDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-TrackModifyDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-MediaCreateDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-MediaModifyDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-XMP-exif:DateTimeOriginal=\"{correctedTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-XMP-xmp:CreateDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-XMP-xmp:ModifyDate=\"{utcTime:yyyy:MM:dd HH:mm:ss}\" " +
                                                   $"-Keys:CreationDate=\"{correctedTime:yyyy:MM:dd HH:mm:ss}{offsetStr}\" " +
                                                   $"-Keys:AndroidTimeZone=\"{offsetShort}\" " +
                                                   $"-Title=\"Archived Video - {correctedTime:yyyy}\" " +
                                                   $"-Description=\"Original: {backupDateString}. Local Chicago Time: {correctedTime:t}\" " +
                                                   $"-P -overwrite_original \"{finalPath}\"";
                            }
                            else
                            {
                                // Standard Image logic
                                string exifDate = correctedTime.ToString("yyyy:MM:dd HH:mm:ss");
                                exifArgs = $"-AllDates=\"{exifDate}\" -UserComment=\"OriginalDateBackup: {backupDateString}\" " +
                                           $"-overwrite_original \"{finalPath}\"";
                            }

                            // Execute ExifTool
                            ProcessStartInfo exifInfo = new ProcessStartInfo
                            {
                                FileName = @"E:\exiftool\exiftool.exe",
                                Arguments = exifArgs,
                                CreateNoWindow = true,
                                UseShellExecute = false
                            };

                            using (Process p = Process.Start(exifInfo)) { p.WaitForExit(); }

                            // Final Sync for Windows File Explorer
                            // Note: If you used -P in exifArgs, these calls are what finalize the system dates
                            File.SetCreationTime(finalPath, correctedTime);
                            File.SetLastWriteTime(finalPath, correctedTime);
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}