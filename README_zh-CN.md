# PicExif - 图片地理位置信息提取工具

## 功能介绍

PicExif是一款Windows桌面应用程序，用于提取和显示图片的地理位置信息及其他元数据。

### 软件截图

![PicExif界面截图](screenshots/screenshot.png)

### 主要功能

1. **图片上传**：
   - 支持拖拽图片到上传区域
   - 支持点击按钮选择图片文件

2. **图片预览**：
   - 上传后自动显示图片缩略图

3. **信息显示**：
   - **基本信息**：文件名称、文件大小、文件格式
   - **EXIF信息**：创建时间、分辨率、相机型号
   - **地理位置**：纬度、经度、两种坐标格式（纬度,经度和经度,纬度）
   - **地图显示**：内置高德地图显示图片拍摄位置

4. **复制功能**：
   - 每个信息项旁边都有复制按钮
   - 点击即可复制对应信息到剪贴板

## 系统要求

- Windows 10或更高版本
- .NET 6.0运行时

## 快速开始

### ⭐ 无需编译！直接使用

如果不想从源码编译，可以直接在项目的 Releases 页面下载预编译的 ZIP 包，解压后双击 `PicExif.exe` 即可使用！

### 1. 配置API密钥（可选）

项目使用高德地图API来显示地图。首次使用前：

1. 复制 `appsettings.json.example` 为 `appsettings.json`
2. 在 `appsettings.json` 中填入您的高德地图API密钥
3. 如果没有配置，将使用默认密钥

### 2. 构建和运行

```bash
dotnet build
dotnet run
```

或者直接运行编译后的 `bin\Debug\net6.0-windows\PicExif.exe`

### 使用方法

1. 拖拽图片到上传区域或点击"选择图片文件"按钮
2. 查看右侧显示的图片信息
3. 点击信息项旁边的复制按钮复制对应信息
4. 点击"打开地图"按钮在浏览器中查看详细位置

## 技术说明

- 开发框架：WPF (.NET 6.0)
- 图片元数据提取：MetadataExtractor库
- 图片处理：System.Drawing.Common
- 配置管理：Microsoft.Extensions.Configuration
- 地图服务：高德地图API
- 界面设计：现代化WPF界面

## 项目结构

```
PicExif/
├── Models/                  # 数据模型
│   └── ImageInfo.cs         # 图片信息模型
├── Services/                # 服务类
│   ├── ExifService.cs       # EXIF数据提取服务
│   └── ImageService.cs      # 图像处理服务
├── Resources/               # 资源文件
│   ├── app_icon.ico
│   ├── favicon.ico
│   └── favicon-16x16.png
├── App.xaml                 # 应用入口
├── App.xaml.cs
├── MainWindow.xaml          # 主窗口UI
├── MainWindow.xaml.cs       # 主窗口逻辑
├── appsettings.json.example # 配置文件示例
├── PicExif.csproj           # 项目配置
└── README.md
```

## 开发指南

### 从源码构建

1. 克隆或下载项目
2. 复制 `appsettings.json.example` 为 `appsettings.json` 并配置（可选）
3. 运行 `dotnet restore` 恢复依赖
4. 运行 `dotnet build` 构建项目
5. 运行 `dotnet run` 启动应用

### 发布应用

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

## 注意事项

- 仅支持常见的图片格式（JPG、PNG、BMP、TIFF、GIF等）
- 地理位置信息仅在图片包含GPS数据时显示
- 首次运行可能需要一些时间加载依赖项
- 请妥善保管您的API密钥，不要提交到公共仓库

## 版本信息

- 版本：1.0.0
- 发布日期：2026-04-27
