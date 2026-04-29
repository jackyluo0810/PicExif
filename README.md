# PicExif - Image EXIF & GPS Information Extractor

![GitHub](https://img.shields.io/github/license/jackyluo0810/PicExif)
![GitHub last commit](https://img.shields.io/github/last-commit/jackyluo0810/PicExif)
![GitHub stars](https://img.shields.io/github/stars/jackyluo0810/PicExif)

## Overview

PicExif is a Windows desktop application that extracts and displays GPS location information and other metadata from images. It supports drag-and-drop image upload, real-time EXIF data extraction, and map visualization of photo locations.

### Screenshot

![PicExif Screenshot](screenshots/screenshot.png)

### Features

1. **Image Upload**:
   - Drag and drop images onto the upload area
   - Click the button to select image files

2. **Image Preview**:
   - Automatically displays image thumbnail after upload

3. **Information Display**:
   - **Basic Info**: File name, file size, file format
   - **EXIF Info**: Creation time, resolution, camera model
   - **GPS Location**: Latitude, longitude, two coordinate formats (lat,lng and lng,lat)
   - **Map Display**: Built-in AMap (Gaode Map) showing photo location

4. **Copy Function**:
   - Each information field has a copy button
   - Click to copy the corresponding information to clipboard

## System Requirements

- Windows 10 or higher
- .NET 6.0 Runtime

## Quick Start

### ⭐ No Compilation Needed! Use Directly

If you don't want to compile from source, you can download the pre-built ZIP package from the project's [Releases](https://github.com/jackyluo0810/PicExif/releases) page, extract it, and double-click `PicExif.exe` to use it!

### 1. Configure API Key (Optional)

The project uses AMap API to display maps. Before first use:

1. Copy `appsettings.json.example` to `appsettings.json`
2. Edit `appsettings.json` and fill in your AMap API key
3. If not configured, the default key will be used

### 2. Build and Run

```bash
dotnet build
dotnet run
```

Or run the compiled executable at `bin\Debug\net6.0-windows\PicExif.exe`

### Usage

1. Drag an image onto the upload area or click "Select Image File"
2. View the image information displayed on the right
3. Click the copy button next to any information field
4. Click "Open Map" to view the detailed location in browser

## Technical Details

- Framework: WPF (.NET 6.0)
- Image Metadata Extraction: MetadataExtractor library
- Image Processing: System.Drawing.Common
- Configuration: Microsoft.Extensions.Configuration
- Map Service: AMap (Gaode Map) API
- UI Design: Modern WPF interface

## Project Structure

```
PicExif/
├── Models/                  # Data models
│   └── ImageInfo.cs         # Image information model
├── Services/                # Services
│   ├── ExifService.cs       # EXIF data extraction service
│   └── ImageService.cs      # Image processing service
├── Resources/               # Resource files
│   ├── app_icon.ico
│   ├── favicon.ico
│   └── favicon-16x16.png
├── App.xaml                 # Application entry
├── App.xaml.cs
├── MainWindow.xaml          # Main window UI
├── MainWindow.xaml.cs       # Main window logic
├── appsettings.json.example # Configuration example
├── PicExif.csproj           # Project configuration
└── README.md
```

## Development Guide

### Build from Source

1. Clone or download the project
2. Copy `appsettings.json.example` to `appsettings.json` and configure (optional)
3. Run `dotnet restore` to restore dependencies
4. Run `dotnet build` to build the project
5. Run `dotnet run` to start the application

### Publish Application

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) for more information.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Tags

#exif #gps #image-metadata #photo-gps #wpf #csharp #dotnet #windows-desktop #image-processing #metadata-extraction
