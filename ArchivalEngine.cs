using Exiftools;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace MediaArchiver
{
    public class ArchivalEngine
    {
        private readonly IExiftool _exiftool;

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
            { "nikon corporation", "Nikon" },
            { "nikon", "Nikon" },
            { "samsung","Samsung" },
            { "sony corp.", "Sony" },
            { "sony corporation", "Sony" },
            { "sony", "Sony" },
            { "fujifilm co.", "Fujifilm" },
            { "fujifilm", "Fujifilm" }
        };

        // --- ORCHESTRATION LAYER: Handles the physical file system side effects ---
        public void ProcessFiles(IExiftool exiftool, RenameViewModel model, string[] fileNames)
        {
            ExifModel defaultCamera = GetCameraDefault(fileNames, _exiftool, model);

            if (defaultCamera == null)
            {
                throw new Exception("Could not detect camera metadata automatically. Please manually select the camera profile from the dropdown.");
            }

            foreach (string file in fileNames)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!fileInfo.Exists) continue;

                // 1. In-memory data retrieval side effects
                ExifModel currentExifModel = exiftool.GetFileMetadata(fileInfo.FullName) ?? new ExifModel();
                DateTime originalFileDate = GetDateTaken(fileInfo.FullName, model.UseUTC, out bool hasMetadata);

                // 2. CALL PURE ENGINE: Calculate the blueprint in-memory. 
                // We pass a delegate function 'File.Exists' so the loop can check duplication without hardcoding disk access!
                ExifModel targetBlueprint = BuildTargetMetadata(
                    model,
                    defaultCamera,
                    fileInfo.DirectoryName,
                    fileInfo.FullName,
                    currentExifModel,
                    originalFileDate,
                    File.Exists
                );

                if (targetBlueprint == null) continue;

                // 3. PHYSICAL DISK EXECUTION LAYER: Run side effects sequentially
                if (!string.Equals(targetBlueprint.FinalPath, fileInfo.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    // Ensure destination directory exists (insurance policy)
                    string targetDir = Path.GetDirectoryName(targetBlueprint.FinalPath);
                    if (!System.IO.Directory.Exists(targetDir))
                    {
                        System.IO.Directory.CreateDirectory(targetDir);
                    }

                    fileInfo.MoveTo(targetBlueprint.FinalPath);
                }

                // Write the updated EXIF metadata back to the file at its final destination
                SaveMetaData(targetBlueprint);
            }
            // 2. Evaluate if the folder name needs a GPS prepend before moving
            DirectoryInfo dirInfo = new DirectoryInfo(model.DirectoryPath);
            string dirName = dirInfo.Name;

            // Check if the current folder name starts with a 4-digit year
            bool startsWithYear = Regex.IsMatch(dirName, @"^\d{4}");
            string workingDirName = dirName;

            if (!startsWithYear && model.LocationPresets != null && !string.IsNullOrEmpty(model.LocationPresets.LocationName))
            {
                workingDirName = $"{model.LocationPresets.LocationName}";
            }

            // 3. Set up the destination path ("...\timefix\MCD")
            // dirInfo.Parent.FullName gets us "E:\picture3\2014"
            string parentPath = dirInfo.Parent.FullName;
            string timefixPath = Path.Combine(parentPath, "timefix");

            // Create the 'timefix' directory if it doesn't exist yet
            if (!System.IO.Directory.Exists(timefixPath))
            {
                System.IO.Directory.CreateDirectory(timefixPath);
            }


            // 4. Move the directory to mark it done
            string finalDestinationPath = Path.Combine(timefixPath, workingDirName);

            if (System.IO.Directory.Exists(finalDestinationPath))
            {
                int counter = 1;
                string uniqueDirName = workingDirName;

                // Keep incrementing the counter until a unique folder name is found
                while (System.IO.Directory.Exists(Path.Combine(timefixPath, uniqueDirName)))
                {
                    uniqueDirName = $"{workingDirName}_{counter}";
                    counter++;
                }

                finalDestinationPath = Path.Combine(timefixPath, uniqueDirName);
            }

            // 5. Safely move the directory
            System.IO.Directory.Move(model.DirectoryPath, finalDestinationPath);
        }
    

        // --- PURE CALCULATION ENGINE: 100% Testable in memory with zero disk access ---
        // --- PURE CALCULATION ENGINE: 100% Automated Scenario C ---
        public ExifModel BuildTargetMetadata(
            RenameViewModel model,
            ExifModel defaultCamera,
            string directoryName,
            string fullName,
            ExifModel currentExifModel,
            DateTime originalFileDate,
            Func<string, bool> fileExistsCheck)
        {
            currentExifModel ??= new ExifModel();
            ExifModel exifModel = currentExifModel.Clone();

            string rawExtension = Path.GetExtension(fullName);

            string cleanExtension = rawExtension.ToLowerInvariant();

            if (cleanExtension == ".jfif")
                cleanExtension = ".jpg";

            // 1. Set Hardware & GPS standard baselines
            SetCameraValues(model, defaultCamera, currentExifModel, exifModel);
            SetGPS(model, exifModel, currentExifModel);

            currentExifModel.IsMobile = IsMobileDevice(exifModel);

            // 2. Timeline Adjustments
            DateTime mediaDate = model.UseDate && model.ManualDateTime.HasValue
                ? new DateTime(model.ManualDateTime.Value.Year, model.ManualDateTime.Value.Month, model.ManualDateTime.Value.Day, originalFileDate.Hour, originalFileDate.Minute, originalFileDate.Second)
                : originalFileDate;

            DateTime correctedTime = AdjustMediaTime(mediaDate, model, currentExifModel);

            // 3. Unique Filename Generation & Conflict Resolution (Natural Keys)
            string newName = "";
            if(!String.IsNullOrEmpty(model.AltName))
            {
                newName = model.AltName.Trim();
            }
            else
            {
                newName = correctedTime.ToString("yyyyMMdd_HHmmss");
            }
            
                      
            
            string finalPath = Path.Combine(directoryName, newName + cleanExtension);

            int count = 1;
            while (fileExistsCheck(finalPath) && !string.Equals(finalPath, fullName, StringComparison.OrdinalIgnoreCase))
            {
                if(String.IsNullOrEmpty(model.AltName))
                {
                    finalPath = Path.Combine(directoryName, $"{newName}_{count:D2}{cleanExtension}");
                }
                else
                {
                    finalPath = Path.Combine(directoryName, $"{newName} {count:D2}{cleanExtension}");
                }
                count++;
            }

            // 4. Calculate Timezone Triggers
            TimeSpan tzOffset = GetSelectedTimeZoneOffset(correctedTime, model);
            string sign = tzOffset.Ticks >= 0 ? "+" : "-";
            string offsetStr = $"{sign}{Math.Abs(tzOffset.Hours):D2}:{Math.Abs(tzOffset.Minutes):D2}";

            exifModel.FinalPath = finalPath;
            exifModel.OriginalFileDate = originalFileDate;
            exifModel.CorrectedTime = correctedTime;
            exifModel.TzOffset = tzOffset;

            // 5. THE LEAN TITLE EXCEPTION: Photos return string.Empty; Videos get "Archived video [Year]"
            exifModel.Title = DetermineTitle(currentExifModel.Title, model.ForceUpdate, cleanExtension, mediaDate.Year.ToString());

            // 6. SCENARIO C TRACKING STRING: No text gluing, no UI lookups. Pure immutable technical audit trail.
            string cleanOriginalDateStr = ParseOriginalDateDescription(currentExifModel, originalFileDate);
            string tzAbbreviation = GetTimeZoneAbbreviation(model.TimeZone, correctedTime);

            if (model.LocationPresets != null && !string.IsNullOrWhiteSpace(model.LocationPresets.LocationCode))
            {
                string locationSuffix = BuildLocationSuffix(model);
                exifModel.Description = $"Location: {model.LocationPresets.LocationCode}, Original: {cleanOriginalDateStr}, Final: {correctedTime:yyyy-MM-dd HH:mm:ss} ({tzAbbreviation}{offsetStr})";
            }
            else
            {
                exifModel.Description = $"Original: {cleanOriginalDateStr}, Final: {correctedTime:yyyy-MM-dd HH:mm:ss} ({tzAbbreviation}{offsetStr})";
            }

            return exifModel;
        }
        private bool IsMobileDevice(ExifModel exif)
        {
            if (exif == null) return false;

            // Grab the Make or Model (adjust these property names to match your ExifModel class)
            string make = exif.Make?.ToLowerInvariant() ?? "";
            string model = exif.Model?.ToLowerInvariant() ?? "";

            // Common mobile manufacturers and indicators
            return make.Contains("apple") ||
                   make.Contains("google") ||
                   make.Contains("samsung") ||
                   make.Contains("huawei") ||
                   make.Contains("htc") ||
                   make.Contains("nokia") ||
                   make.Contains("qcom-aa") ||
                   make.Contains("xiaomi") ||
                   model.Contains("iphone") ||
                   model.Contains("htc") ||
                   model.Contains("qcom-aa") ||
                   model.Contains("android");
        }
        public void SetCameraValues(RenameViewModel model, ExifModel defaultCamera, ExifModel currentExifModel, ExifModel exifModel)
        {
            // LAYER 1: If box is CHECKED, unconditionally force the UI Screen Dropdown Preset
            if (model.ForceCameraUpdate)
            {
                if (model.CameraPresets != null && !string.IsNullOrWhiteSpace(model.CameraPresets.Make))
                {
                    exifModel.Make = model.CameraPresets.Make;
                    exifModel.Model = model.CameraPresets.Model;
                }
                return;
            }

            // LAYER 2: Box is UNCHECKED, protect existing file history
            if (currentExifModel != null && !string.IsNullOrWhiteSpace(currentExifModel.Make))
            {
                exifModel.Make = NormalizeMake(currentExifModel.Make);
                exifModel.Model = NormalizeModel(currentExifModel.Make, currentExifModel.Model);
            }
            // LAYER 3: EXCEPTION - Box is unchecked & file is empty -> Use the Directory Default Camera profile
            else if (defaultCamera != null && !string.IsNullOrWhiteSpace(defaultCamera.Make))
            {
                exifModel.Make = defaultCamera.Make;
                exifModel.Model = defaultCamera.Model;
            }
            // LAYER 4: FALLBACK - Box is unchecked, file is empty, and no directory camera exists -> Use UI Screen Preset
            else if (model.CameraPresets != null && !string.IsNullOrWhiteSpace(model.CameraPresets.Make))
            {
                exifModel.Make = model.CameraPresets.Make;
                exifModel.Model = model.CameraPresets.Model;
            }
        }

        public void SetGPS(RenameViewModel model, ExifModel exifModel, ExifModel currentExifModel)
        {
            // LAYER 1: If box is CHECKED, explicitly force the UI screen coordinates
            if (model.ForceGPSUpdate)
            {
                exifModel.LatitudeText = model.LocationPresets?.Latitude.ToString() ?? "";
                exifModel.LongitudeText = model.LocationPresets?.Longitude.ToString() ?? "";
            }
            // LAYER 2: If box is UNCHECKED, protect the history if the file has data
            else if (currentExifModel != null && !string.IsNullOrEmpty(currentExifModel.LatitudeText))
            {
                exifModel.LatitudeText = currentExifModel.LatitudeText;
                exifModel.LongitudeText = currentExifModel.LongitudeText;
            }
            // LAYER 3: FALLBACK - Box is unchecked but file is empty, use the UI screen coordinates
            else
            {
                exifModel.LatitudeText = model.LocationPresets?.Latitude.ToString() ?? "";
                exifModel.LongitudeText = model.LocationPresets?.Longitude.ToString() ?? "";
            }
        }

        private string GetTimeZoneAbbreviation(string windowsTimeZoneId, DateTime correctedTime)
        {
            if (string.IsNullOrEmpty(windowsTimeZoneId)) return "UTC";

            try
            {
                // Fetch the actual timezone rule object from the Windows registry
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);

                // Ask Windows: "Is this specific file date currently in Daylight Saving Time?"
                bool isDaylight = tzInfo.IsDaylightSavingTime(correctedTime);

                // Map the official Windows Zone ID to its baseline US root abbreviation
                return windowsTimeZoneId switch
                {
                    "Eastern Standard Time" => isDaylight ? "EDT" : "EST",
                    "Central Standard Time" => isDaylight ? "CDT" : "CST",
                    "Mountain Standard Time" => isDaylight ? "MDT" : "MST",
                    "Pacific Standard Time" => isDaylight ? "PDT" : "PST",
                    "Alaskan Standard Time" => isDaylight ? "AKDT" : "AKST",
                    "Hawaiian Standard Time" => isDaylight ? "HDT" : "HST", // Note: Hawaii doesn't observe DST, but the registry key supports it safely
                    _ => "UTC" // Failsafe fallback
                };
            }
            catch (TimeZoneNotFoundException)
            {
                return "UTC"; // Failsafe if a strange zone string slips through
            }
        }

        private string BuildLocationSuffix(RenameViewModel model)
        {
            string locationName = model.LocationPresets?.LocationName?.Trim() ?? string.Empty;
            string city = model.LocationPresets?.City?.Trim() ?? string.Empty;
            string state = model.LocationPresets?.State?.Trim() ?? string.Empty;

            // Build the geographical string safely (e.g., "Chicago, IL")
            var locationParts = new List<string>();
            if (!string.IsNullOrEmpty(city)) locationParts.Add(city);
            if (!string.IsNullOrEmpty(state)) locationParts.Add(state);
            string geoPlace = string.Join(", ", locationParts);

            // Formulate the caption modifier text
            if (!string.IsNullOrEmpty(locationName) && !string.IsNullOrEmpty(geoPlace))
            {
                return $" at {locationName} ({geoPlace})";
            }
            if (!string.IsNullOrEmpty(locationName))
            {
                return $" at {locationName}";
            }
            if (!string.IsNullOrEmpty(geoPlace))
            {
                return $" in {geoPlace}";
            }

            return string.Empty;
        }

        private string ResolveBaseDescription(RenameViewModel model, ExifModel currentMetadata, string calculatedTitle)
        {
            // ─── FIXED FLAG CHECK: Return Description, not Title! ───────────────
            if (model.ForceTitleDescUpdate && !string.IsNullOrEmpty(model.Description))
            {
                return model.Description.Trim();
            }
            // ──────────────────────────────────────────────────────────────────────

            // 1. Fallback to standard UI model state description text
            string baseDescription = model.Description?.Trim() ?? string.Empty;

            // 2. STICKY STATE PROTECTION: If UI is blank, rescue the historical caption
            if (string.IsNullOrEmpty(baseDescription) && !string.IsNullOrEmpty(currentMetadata?.Description))
            {
                if (currentMetadata.Description.Contains("Original:"))
                {
                    int markerIndex = currentMetadata.Description.IndexOf("Original:");
                    string oldCaption = currentMetadata.Description.Substring(0, markerIndex).Trim();

                    if (oldCaption.EndsWith("."))
                    {
                        oldCaption = oldCaption.Substring(0, oldCaption.Length - 1).Trim();
                    }

                    baseDescription = oldCaption;
                }
                else
                {
                    baseDescription = currentMetadata.Description.Trim();
                }
            }

            return baseDescription;
        }

        private string ParseOriginalDateDescription(ExifModel currentMetadata, DateTime originalFileDate)
    {
        if (!string.IsNullOrEmpty(currentMetadata?.Description))
        {
            // This pattern looks for "Original: " followed by exactly: 4 digits, 2 digits, 2 digits (date) 
            // and then 2 digits, 2 digits, 2 digits (time), separated by colons and spaces.
            var match = Regex.Match(currentMetadata.Description, @"Original:\s*(\d{4}:\d{2}:\d{2}\s+\d{2}:\d{2}:\d{2})");

            if (match.Success)
            {
                // Group[1] captures just the clean timestamp block inside the parentheses!
                return match.Groups[1].Value.Trim();
            }
        }

        // First-time processing fallback: establish the fresh original baseline from the file
        return originalFileDate.ToString("yyyy:MM:dd HH:mm:ss");
    }

        private string ResolveBaseDescription(RenameViewModel model, ExifModel currentMetadata)
        {
            // 1. Resolve the baseline text from the current UI model state
            string baseDescription = model.Description?.Trim() ?? string.Empty;

            // 2. STICKY STATE PROTECTION: If the UI description is blank, rescue the historical caption!
            if (string.IsNullOrEmpty(baseDescription) && !string.IsNullOrEmpty(currentMetadata?.Description))
            {
                // If the old description contains our tracking marker, split and grab everything BEFORE it
                if (currentMetadata.Description.Contains("Original:"))
                {
                    int markerIndex = currentMetadata.Description.IndexOf("Original:");
                    string oldCaption = currentMetadata.Description.Substring(0, markerIndex).Trim();

                    // Clean up trailing periods so they don't compound (e.g. "Graduation Day... Original:")
                    if (oldCaption.EndsWith("."))
                    {
                        oldCaption = oldCaption.Substring(0, oldCaption.Length - 1).Trim();
                    }

                    baseDescription = oldCaption;
                }
                else
                {
                    // If it doesn't contain the token yet, the entire string is our raw legacy caption baseline
                    baseDescription = currentMetadata.Description.Trim();
                }
            }

            return baseDescription;
        }
        private void SaveMetaData(ExifModel exifModel)
        {
            try
            {
                _exiftool.WriteMetadataToDisk(exifModel);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to write metadata for {exifModel.FinalPath}: {ex.Message}");
            }
        }

        private ExifModel GetCameraDefault(string[] fileNames, IExiftool metadataReader, RenameViewModel model)
        {
            foreach (string file in fileNames)
            {
                string extension = Path.GetExtension(file).ToLower();
                string[] validExtensions = { ".jpg", ".jpeg", ".png", ".heic", ".mpg", ".mp4", ".mts", ".avi", ".webp" };

                if (validExtensions.Contains(extension))
                {
                    ExifModel tempExifModel = metadataReader.GetFileMetadata(file);

                    if (tempExifModel != null && !string.IsNullOrEmpty(tempExifModel.Make))
                    {
                        var detectedCamera = new ExifModel();
                        detectedCamera.Make = NormalizeMake(tempExifModel.Make);
                        detectedCamera.Model = NormalizeModel(detectedCamera.Make, tempExifModel.Model);
                        return detectedCamera;
                    }
                }
            }

            if (model.CameraPresets != null && !string.IsNullOrEmpty(model.CameraPresets.Make))
            {
                return new ExifModel
                {
                    Make = model.CameraPresets.Make,
                    Model = model.CameraPresets.Model
                };
            }

            return null;
        }

        public DateTime AdjustMediaTime(DateTime originalTime, RenameViewModel model, ExifModel currentExifModel)
        {
            // If it's a mobile phone, bypass the adjustment entirely and return the original time
            if (currentExifModel?.IsMobile == true)
            {
                return originalTime;
            }

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
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone);
            return tz.GetUtcOffset(mediaDate);
        }

        public string DetermineTitle(string currentTitle, bool forceUpdate, string extension, string year)
        {
            currentTitle = currentTitle?.Trim() ?? "";
       

            string[] videoExtensions = { ".mpg", ".mp4", ".mts", ".avi", ".mov" };
            string fileType = videoExtensions.Contains(extension.ToLower()) ? "video" : "photo";

            /*
            if (forceUpdate) return inputTitle;
            if (!string.IsNullOrEmpty(currentTitle) && !currentTitle.StartsWith("Archived", StringComparison.OrdinalIgnoreCase)) return currentTitle;

            if (string.IsNullOrEmpty(currentTitle) || currentTitle.StartsWith("Archived", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(inputTitle)) return inputTitle;
            }
            */

            string finalYear = !string.IsNullOrEmpty(year) ? year : DateTime.Now.Year.ToString();
            return $"Archived {fileType} {finalYear}";
        }

        public string NormalizeMake(string currentMaker)
        {
            if (string.IsNullOrWhiteSpace(currentMaker)) return string.Empty;
            string cleanMaker = new string(currentMaker.Where(c => !char.IsControl(c)).ToArray());
            string lookupKey = cleanMaker.ToLowerInvariant().Trim();

            foreach (var kvp in MakerMap)
            {
                if (lookupKey.Contains(kvp.Key)) return kvp.Value;
            }

            return cleanMaker.Trim();
        }

        public string NormalizeModel(string currentMake, string currentModel)
        {
            return currentModel;
        }

        private static DateTime GetDateTaken(string filePath, bool isTrueUTC, out bool hasMetadata)
        {
            hasMetadata = false;
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            if (DateTime.TryParseExact(fileName, "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.AssumeLocal, out DateTime parsedDate))
            {
                hasMetadata = true;
                return DateTime.SpecifyKind(parsedDate, DateTimeKind.Local);
            }

            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                var keysDirectory = directories.OfType<QuickTimeMetadataHeaderDirectory>().FirstOrDefault();
                if (keysDirectory != null && keysDirectory.TryGetDateTime(QuickTimeMetadataHeaderDirectory.TagCreationDate, out DateTime keysDate))
                {
                    hasMetadata = true;
                    return keysDate.ToLocalTime();
                }

                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (subIfdDirectory != null && subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime exifDate))
                {
                    hasMetadata = true;
                    return DateTime.SpecifyKind(exifDate, DateTimeKind.Local);
                }

                var movDirectory = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                if (movDirectory != null && movDirectory.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out DateTime movDate))
                {
                    hasMetadata = true;
                    return isTrueUTC ? DateTime.SpecifyKind(movDate, DateTimeKind.Utc).ToLocalTime()
                                     : DateTime.SpecifyKind(movDate, DateTimeKind.Local);
                }
            }
            catch { /* Metadata error */ }

            return File.GetLastWriteTime(filePath);
        }

    }
}