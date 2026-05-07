using System;

namespace PicExif.Models
{
    public class ImageInfo
    {
        public string FileName { get; init; } = string.Empty;
        public string FilePath { get; init; } = string.Empty;
        public DateTime? CreationTime { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public string CameraModel { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? GcjLatitude { get; set; }
        public double? GcjLongitude { get; set; }
        public string FileSize { get; set; } = string.Empty;
        public string FileFormat { get; set; } = string.Empty;
    }
}