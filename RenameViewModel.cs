using System;
using System.Collections.Generic;
using System.Text;

namespace MediaArchiver
{
    public class RenameViewModel
    {
        public string DirectoryPath { get; set; }
        public int OffSetYears { get; set; }
        public int OffSetMonths { get; set; }
        public int OffSetDays { get; set; }
        public int OffSetHours { get; set; }
        public int OffSetMinutes { get; set; }
        public int OffSetSeconds { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public DateTime? ManualDateTime { get; set; }
        public CameraPreset? CameraPresets { get; set; }
        public LocationPreset? LocationPresets { get; set; }
        public bool MP4Only { get; set; } = false;
        public bool UseUTC { get; set; } = false;
        public bool UseDate {  get; set; } = false;
        public bool ForceUpdate { get; set; } = false;


    }
}
