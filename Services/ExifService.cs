using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using PicExif.Models;

namespace PicExif.Services
{
    public class ExifService
    {
        private readonly CoordinateConverterService _converter = new CoordinateConverterService();

        public ImageInfo ExtractImageInfo(string filePath)
        {
            var imageInfo = new ImageInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                var fileInfo = new FileInfo(filePath);
                imageInfo.FileSize = FormatFileSize(fileInfo.Length);
                imageInfo.FileFormat = Path.GetExtension(filePath).ToUpper().TrimStart('.');
                imageInfo.CreationTime = fileInfo.CreationTime;

                var directories = ImageMetadataReader.ReadMetadata(filePath);
                
                // 只遍历我们需要的目录
                foreach (var directory in directories)
                {
                    if (directory.Name is "Exif SubIFD" or "Exif IFD0" or "GPS")
                    {
                        foreach (var tag in directory.Tags)
                        {
                            ProcessTag(imageInfo, directory.Name, tag.Name, tag.Description);
                        }
                    }
                }

                // 在所有标签处理完后再设置Location
                if (imageInfo.Latitude.HasValue && imageInfo.Longitude.HasValue)
                {
                    imageInfo.Location = $"{imageInfo.Latitude.Value:F6}, {imageInfo.Longitude.Value:F6}";
                    
                    // 转换为GCJ-02坐标系
                    var (gcjLat, gcjLng) = _converter.Wgs84ToGcj02(imageInfo.Latitude.Value, imageInfo.Longitude.Value);
                    imageInfo.GcjLatitude = gcjLat;
                    imageInfo.GcjLongitude = gcjLng;
                }

                ExtractResolutionFromImage(filePath, imageInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXIF提取错误: {ex.Message}");
            }

            return imageInfo;
        }

        private void ProcessTag(ImageInfo imageInfo, string directoryName, string tagName, string tagValue)
        {
            switch (directoryName)
            {
                case "Exif SubIFD":
                case "Exif IFD0":
                    ProcessExifTags(imageInfo, tagName, tagValue);
                    break;
                case "GPS":
                    ProcessGpsTags(imageInfo, tagName, tagValue);
                    break;
            }
        }

        private void ProcessExifTags(ImageInfo imageInfo, string tagName, string tagValue)
        {
            switch (tagName)
            {
                case "Date/Time":
                case "Date/Time Original":
                    if (DateTime.TryParse(tagValue, out var dateTime))
                        imageInfo.CreationTime = dateTime;
                    break;
                case "Model":
                    imageInfo.CameraModel = tagValue;
                    break;
            }
        }

        private void ProcessGpsTags(ImageInfo imageInfo, string tagName, string tagValue)
        {
            switch (tagName)
            {
                case "GPS Latitude":
                    imageInfo.Latitude = ParseGpsCoordinate(tagValue);
                    break;
                case "GPS Longitude":
                    imageInfo.Longitude = ParseGpsCoordinate(tagValue);
                    break;
            }
        }

        private double? ParseGpsCoordinate(string coordinate)
        {
            try
            {
                if (string.IsNullOrEmpty(coordinate)) return null;
                
                var parts = coordinate.Split('°', '\'', '\"').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                if (parts.Length >= 3)
                {
                    var degrees = double.Parse(parts[0].Trim());
                    var minutes = double.Parse(parts[1].Trim());
                    var seconds = double.Parse(parts[2].Trim().Split(' ')[0]);
                    
                    var result = degrees + minutes / 60.0 + seconds / 3600.0;
                    
                    if (coordinate.Contains("S") || coordinate.Contains("W"))
                        result = -result;
                        
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GPS坐标解析错误: {ex.Message}");
            }
            
            return null;
        }

        private void ExtractResolutionFromImage(string filePath, ImageInfo imageInfo)
        {
            try
            {
                using (var image = Image.FromFile(filePath))
                {
                    imageInfo.Resolution = $"{image.Width} × {image.Height}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"分辨率提取错误: {ex.Message}");
                imageInfo.Resolution = "未知";
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }
}