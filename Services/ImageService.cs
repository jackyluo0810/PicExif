using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PicExif.Services
{
    public class ImageService
    {
        private static readonly string[] SupportedFormats = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".gif" };

        public BitmapImage LoadImage(string filePath, int maxWidth = 300, int maxHeight = 300)
        {
            var bitmap = new BitmapImage();
            
            try
            {
                // 使用Uri方式加载图片，避免文件流问题
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.DecodePixelWidth = maxWidth;
                bitmap.DecodePixelHeight = maxHeight;
                bitmap.UriSource = new Uri(filePath);
                bitmap.EndInit();
                
                bitmap.Freeze();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"图片加载错误: {ex.Message}");
                return null;
            }
            
            return bitmap;
        }

        public bool IsSupportedImageFormat(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLower();
            return SupportedFormats.Contains(extension);
        }
    }
}