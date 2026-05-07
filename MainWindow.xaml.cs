using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using PicExif.Models;
using PicExif.Services;

namespace PicExif
{
    public partial class MainWindow : Window
    {
        private static readonly string AmapApiKey = LoadAmapApiKey();
        private static readonly Color UploadHighlightColor = Color.FromArgb(30, 0, 122, 204);
        private static readonly Color UploadBorderColor = Color.FromRgb(0, 122, 204);
        private static readonly Color NormalBorderColor = Color.FromRgb(221, 221, 221);
        private static readonly Brush SuccessBrush = Brushes.Green;
        private static readonly Brush GrayBrush = Brushes.Gray;
        private static readonly Brush WhiteBrush = Brushes.White;

        private static string LoadAmapApiKey()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
                
                var key = config["AmapApiKey"];
                return !string.IsNullOrWhiteSpace(key) ? key : "0c6f4092dd76640806499734d7c6b074";
            }
            catch
            {
                return "0c6f4092dd76640806499734d7c6b074";
            }
        }

        private static readonly string NoLocationHtml = @"
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <title>地图位置</title>
                        <style>
                            body { margin: 0; padding: 0; height: 100%; display: flex; align-items: center; justify-content: center; background-color: #f0f0f0; font-family: Arial, sans-serif; }
                            .no-location { text-align: center; color: #666; }
                        </style>
                    </head>
                    <body>
                        <div class='no-location'>
                            <p>未找到位置信息</p>
                        </div>
                    </body>
                    </html>
                ";

        private readonly ExifService _exifService;
        private readonly ImageService _imageService;
        private ImageInfo _currentImageInfo;

        public MainWindow()
        {
            InitializeComponent();
            
            _exifService = new ExifService();
            _imageService = new ImageService();
            
            // 设置图标
            try
            {
                string iconPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Resources", "favicon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置图标时发生错误: {ex.Message}");
            }
            
            SetupEventHandlers();
            UpdateUIState(false);
        }

        private void SetupEventHandlers()
        {
            // 拖拽事件
            UploadArea.AllowDrop = true;
            UploadArea.DragEnter += OnDragEnter;
            UploadArea.DragOver += OnDragOver;
            UploadArea.DragLeave += OnDragLeave;
            UploadArea.Drop += OnDrop;
            
            // 按钮点击事件
            SelectFileButton.Click += OnSelectFileClick;
            
            // 复制按钮事件
            CopyFileNameButton.Click += (s, e) => CopyToClipboard(FileNameText.Text);
            CopyFileSizeButton.Click += (s, e) => CopyToClipboard(FileSizeText.Text);
            CopyFileFormatButton.Click += (s, e) => CopyToClipboard(FileFormatText.Text);
            CopyCreationTimeButton.Click += (s, e) => CopyToClipboard(CreationTimeText.Text);
            CopyResolutionButton.Click += (s, e) => CopyToClipboard(ResolutionText.Text);
            CopyCameraModelButton.Click += (s, e) => CopyToClipboard(CameraModelText.Text);
            CopyLatitudeButton.Click += (s, e) => CopyToClipboard(LatitudeText.Text);
            CopyLongitudeButton.Click += (s, e) => CopyToClipboard(LongitudeText.Text);
            CopyLatLngButton.Click += (s, e) => CopyToClipboard(LatLngText.Text);
            CopyLngLatButton.Click += (s, e) => CopyToClipboard(LngLatText.Text);
            
            // 打开地图按钮事件
            OpenMapButton.Click += OnOpenMapClick;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                UploadArea.Background = new SolidColorBrush(UploadHighlightColor);
                UploadArea.BorderBrush = new SolidColorBrush(UploadBorderColor);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            UploadArea.Background = WhiteBrush;
            UploadArea.BorderBrush = new SolidColorBrush(NormalBorderColor);
            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            UploadArea.Background = WhiteBrush;
            UploadArea.BorderBrush = new SolidColorBrush(NormalBorderColor);
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    ProcessImageFile(files[0]);
                }
            }
            e.Handled = true;
        }

        private void OnSelectFileClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif;*.gif|所有文件|*.*",
                Title = "选择图片文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ProcessImageFile(openFileDialog.FileName);
            }
        }

        private async void ProcessImageFile(string filePath)
        {
            try
            {
                StatusText.Text = "正在处理图片...";
                
                if (!_imageService.IsSupportedImageFormat(filePath))
                {
                    MessageBox.Show("不支持的文件格式，请选择常见的图片格式（JPG、PNG、BMP等）。", 
                                  "格式错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusText.Text = "文件格式不支持";
                    return;
                }

                // 显示加载状态
                PreviewImage.Source = null;
                UpdateUIState(false);
                
                // 异步加载图片预览
                var bitmap = await System.Threading.Tasks.Task.Run(() => _imageService.LoadImage(filePath));
                
                if (bitmap != null)
                {
                    // 确保在UI线程上设置图片源
                    await Dispatcher.InvokeAsync(() =>
                    {
                        PreviewImage.Source = bitmap;
                        PreviewImage.Visibility = Visibility.Visible;
                        System.Diagnostics.Debug.WriteLine($"图片已加载: {filePath}");
                    });
                }
                else
                {
                    StatusText.Text = "图片加载失败";
                    System.Diagnostics.Debug.WriteLine($"图片加载失败: {filePath}");
                    return;
                }

                // 异步提取EXIF信息
                _currentImageInfo = await System.Threading.Tasks.Task.Run(() => _exifService.ExtractImageInfo(filePath));
                
                // 更新UI
                await Dispatcher.InvokeAsync(() =>
                {
                    UpdateImageInfoDisplay();
                    UpdateUIState(true);
                    StatusText.Text = $"已加载: {_currentImageInfo.FileName}";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理图片时发生错误：{ex.Message}", "错误", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "处理失败";
                UpdateUIState(false);
                System.Diagnostics.Debug.WriteLine($"处理图片时发生错误: {ex.Message}");
            }
        }

        private void UpdateImageInfoDisplay()
        {
            if (_currentImageInfo == null) return;

            FileNameText.Text = _currentImageInfo.FileName ?? "未知";
            FileSizeText.Text = _currentImageInfo.FileSize ?? "未知";
            FileFormatText.Text = _currentImageInfo.FileFormat ?? "未知";
            
            CreationTimeText.Text = _currentImageInfo.CreationTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未知";
            ResolutionText.Text = _currentImageInfo.Resolution ?? "未知";
            CameraModelText.Text = string.IsNullOrEmpty(_currentImageInfo.CameraModel) ? "未知" : _currentImageInfo.CameraModel;
            
            LatitudeText.Text = _currentImageInfo.Latitude?.ToString("F6") ?? "未知";
            LongitudeText.Text = _currentImageInfo.Longitude?.ToString("F6") ?? "未知";
            
            // 纬度,经度格式
            if (_currentImageInfo.Latitude != null && _currentImageInfo.Longitude != null)
            {
                double lat = _currentImageInfo.Latitude.Value;
                double lng = _currentImageInfo.Longitude.Value;
                LatLngText.Text = $"{lat.ToString("F6")}, {lng.ToString("F6")}";
                LngLatText.Text = $"{lng.ToString("F6")}, {lat.ToString("F6")}";
                
                // 显示高德地图
                ShowAmap(lat, lng);
            }
            else
            {
                LatLngText.Text = "未知";
                LngLatText.Text = "未知";
                
                // 显示无位置信息提示
                MapWebBrowser.NavigateToString(NoLocationHtml);
            }
        }
        
        private void ShowAmap(double latitude, double longitude)
        {
            string html = $"<html><head><meta charset='utf-8'><meta name='viewport' content='initial-scale=1.0, user-scalable=no'><title>高德地图</title><style>body, html {{ width: 100%; height: 100%; margin: 0; padding: 0; }} #container {{ width: 100%; height: 100%; }}</style><script type='text/javascript' src='https://webapi.amap.com/maps?v=1.4.15&key={AmapApiKey}'></script></head><body><div id='container'></div><script type='text/javascript'>var map = new AMap.Map('container', {{ zoom: 15, center: [{longitude}, {latitude}], resizeEnable: true }}); var marker = new AMap.Marker({{ position: [{longitude}, {latitude}], title: '图片拍摄位置' }}); marker.setMap(map); var infoWindow = new AMap.InfoWindow({{ content: '<div style=\"padding: 10px;\">图片拍摄位置</div>', offset: new AMap.Pixel(0, -30) }}); infoWindow.open(map, [{longitude}, {latitude}]);</script></body></html>";
            
            MapWebBrowser.NavigateToString(html);
        }

        private void UpdateUIState(bool hasImage)
        {
            if (hasImage)
            {
                InfoPanelBorder.Visibility = Visibility.Visible;
                NoImageText.Visibility = Visibility.Collapsed;
                UploadHintPanel.Visibility = Visibility.Collapsed;
                PreviewImage.Visibility = Visibility.Visible;
                
                // 根据是否有位置信息调整UI
                var hasLocation = _currentImageInfo?.Latitude != null && _currentImageInfo?.Longitude != null;
                var foregroundBrush = hasLocation ? SuccessBrush : GrayBrush;
                
                LatitudeText.Foreground = foregroundBrush;
                LongitudeText.Foreground = foregroundBrush;
                LatLngText.Foreground = foregroundBrush;
                LngLatText.Foreground = foregroundBrush;
            }
            else
            {
                InfoPanelBorder.Visibility = Visibility.Collapsed;
                NoImageText.Visibility = Visibility.Visible;
                UploadHintPanel.Visibility = Visibility.Visible;
                PreviewImage.Visibility = Visibility.Collapsed;
            }
        }

        private async void CopyToClipboard(string text)
        {
            if (string.IsNullOrEmpty(text) || text == "未知" || text == "未找到位置信息")
            {
                return;
            }

            Button clickedButton = null;
            try
            {
                // 获取点击的按钮引用
                clickedButton = FocusManager.GetFocusedElement(this) as Button;
                
                // 使用 SetDataObject 替代 SetText，更稳定
                Clipboard.SetDataObject(text, true);
                
                // 显示复制成功提示
                if (clickedButton != null)
                {
                    clickedButton.Content = "✅";
                }
            }
            catch (Exception)
            {
                // 剪贴板错误很常见（如被其他程序占用），但通常复制已经成功
                // 不显示错误消息，避免打扰用户
                if (clickedButton != null)
                {
                    clickedButton.Content = "✅";
                }
            }
            
            // 延迟恢复按钮图标
            if (clickedButton != null)
            {
                await System.Threading.Tasks.Task.Delay(1000);
                clickedButton.Content = "📋";
            }
        }

        private void OnOpenMapClick(object sender, RoutedEventArgs e)
        {
            if (_currentImageInfo?.Latitude != null && _currentImageInfo?.Longitude != null)
            {
                double lat = _currentImageInfo.Latitude.Value;
                double lng = _currentImageInfo.Longitude.Value;
                
                // 构造高德地图URL，使用更可靠的格式
                string mapUrl = $"https://www.amap.com/?q={lat},{lng}&center={lng},{lat}&zoom=15";
                
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = mapUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开地图失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("未找到位置信息，无法打开地图", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 清理资源
            PreviewImage.Source = null;
            base.OnClosing(e);
        }
    }
}