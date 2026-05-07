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

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(dirText.Text))
            {
                MessageBox.Show("Please select a valid directory.");
                return;
            }

            string[] fileNames = System.IO.Directory.GetFiles(dirText.Text);

            // --- MASTER TIME OFFSETS ---
            int offsetYears = Convert.ToInt32(txtYear.Text);
            int offsetMonths = Convert.ToInt32(txtMonth.Text);
            int offsetDays = Convert.ToInt32(txtDay.Text);
            int offsetHours = Convert.ToInt32(txtHour.Text);
            int offsetMinutes = Convert.ToInt32(txtMins.Text);
            int offsetSeconds = Convert.ToInt32(txtSecs.Text);
            // ---------------------------

            foreach (string file in fileNames)
            {
                string[] validExtensions = { ".jpg", ".jpeg", ".mpg", ".mp4", ".mts", ".png", ".avi", ".heic" };
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Exists && validExtensions.Contains(fileInfo.Extension.ToLower()))
                {
                    // 1. Extract raw date and check if actual metadata existed
                    DateTime originalFileDate = GetDateTaken(fileInfo.FullName, out bool hasMetadata);
                    DateTime mediaDate;

                    if (dtChange.Value.Date != DateTime.Today)
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
                    DateTime correctedTime = AdjustMediaTime(mediaDate,offsetYears,offsetMonths, offsetDays, offsetHours, offsetMinutes,offsetSeconds);

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
                            string exifDate = correctedTime.ToString("yyyy:MM:dd HH:mm:ss");
                            string backupDateString = originalFileDate.ToString("yyyy:MM:dd HH:mm:ss");

                            // -AllDates updates DateTimeOriginal, CreateDate, and ModifyDate.
                            // -UserComment stores our safe backup string.
                            string exifArgs = $"-AllDates=\"{exifDate}\" -UserComment=\"OriginalDateBackup: {backupDateString}\" -overwrite_original \"{finalPath}\"";

                            ProcessStartInfo exifInfo = new ProcessStartInfo
                            {
                                FileName = @"E:\exiftool\exiftool.exe", // Update to your path
                                Arguments = exifArgs,
                                CreateNoWindow = true,
                                UseShellExecute = false
                            };
                            using (Process p = Process.Start(exifInfo)) { p.WaitForExit(); }

                            // Sync Windows File System dates
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
            txtYear.Text = "0";
            txtMonth.Text = "0";
            txtDay.Text = "0";
            txtHour.Text = "0";
            txtMins.Text = "0";
            txtSecs.Text = "0";
            dtChange.Value = DateTime.Today;
        }


        private DateTime AdjustMediaTime(DateTime originalTime,int offsetYear,int offsetMonth,int offsetDays,int offsetHour,int offsetMinute,int offsetSeconds)
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

        static DateTime GetDateTaken(string filePath, out bool hasMetadata)
        {
            hasMetadata = false; // Default to false
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // --- PHOTO LOGIC (JPG, PNG, HEIC) ---
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (subIfdDirectory != null && subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime exifDate))
                {
                    hasMetadata = true;
                    return exifDate;
                }

                // --- VIDEO LOGIC (MOV, MP4) ---
                var movDirectory = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                if (movDirectory != null && movDirectory.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out DateTime movDate))
                {
                    hasMetadata = true;
                    return DateTime.SpecifyKind(movDate, DateTimeKind.Unspecified);
                }

                // --- AVI LOGIC ---
                var aviDirectory = directories.OfType<AviDirectory>().FirstOrDefault();
                if (aviDirectory != null && aviDirectory.TryGetDateTime(AviDirectory.TagDateTimeOriginal, out DateTime aviDate))
                {
                    hasMetadata = true;
                    return DateTime.SpecifyKind(aviDate, DateTimeKind.Unspecified);
                }
            }
            catch
            {
                // If metadata is corrupt, fall back to file system dates
            }

            return File.GetLastWriteTime(filePath);
        }
    }
}