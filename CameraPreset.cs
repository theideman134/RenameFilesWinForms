using System;
using System.Collections.Generic;
using System.Text;

namespace RenameFilesWinForms
{
    public class CameraPreset
    {
        public int CameraID { get; set; }
        public string DisplayName { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime DateCreated { get; set; }

    }
}