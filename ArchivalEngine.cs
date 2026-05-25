using Exiftools;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaArchiver
{
    public class ArchivalEngine
    {
        IExiftool _exiftool;
        public ArchivalEngine(IExiftool exiftool) 
        { 
            _exiftool = exiftool;
        }

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

        public void ProcessFiles(IExiftool exiftool, RenameViewModel model, string[] fileNames)
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
                            model.ManualDateTime.Value.Year,
                            model.ManualDateTime.Value.Month,
                            model.ManualDateTime.Value.Day,
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
                    DateTime correctedTime = AdjustMediaTime(mediaDate, model);

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

                        TimeSpan tzOffset = GetSelectedTimeZoneOffset(correctedTime,model);

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
                        exifModel.Title = DetermineTitle(curretExifModel.Title, model.Title, model.ForceUpdate, fileInfo.Extension, mediaDate.Year.ToString());

                        exifModel.Description = $"{model.Description} Original: {backupDateString}, Final: {correctedTime:yyyy-MM-dd HH:mm:ss} (UTC{offsetStr})";

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
        public TimeSpan GetSelectedTimeZoneOffset(DateTime mediaDate, RenameViewModel model)
        {
            TimeZoneInfo tz;

            // Determine which timezone to use based on the model's TimeZone property
            tz = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);
            // Get the UTC offset for the specific date (this automatically handles Daylight Saving Time)
            return tz.GetUtcOffset(mediaDate);
        }
        public string DetermineTitle(string currentTitle, string inputTitle, bool forceUpdate, string extension, string year)
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

    }
}
