# 🧹 PC TuneUp

A lightweight Windows PC cleanup utility built with WPF and .NET 8. Clean up junk files and optimize your system with a modern, user-friendly interface.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

- **Scan → Select → Fix** workflow - See what's taking up space before cleaning
- **Windows Temp Files** - Clean temporary files from Windows and applications
- **Windows Update Cache** - Remove old Windows Update download files
- **Recycle Bin** - Empty deleted files from all drives
- **Browser Cache** - Clean Chrome, Edge, and Firefox caches
- **DNS Cache** - Flush DNS resolver to fix connectivity issues
- **Smart Browser Detection** - Automatically detects running browsers and offers to close them

## Screenshots

The app features a modern dark theme with clear status indicators:
- 🔴 Red badges for large items (>100MB)
- 🟡 Yellow badges for smaller items
- Progress tracking and detailed logs

## Requirements

- Windows 10/11
- .NET 8.0 Runtime
- Administrator privileges (for cleaning system folders)

## Installation

### Option 1: Download Release
Download the latest release from the [Releases](../../releases) page.

### Option 2: Build from Source
```powershell
git clone https://github.com/mjtpena/PCTuneUp.git
cd PCTuneUp/PCTuneUp
dotnet build --configuration Release
```

The executable will be in `bin/Release/net8.0-windows/PCTuneUp.exe`

## Usage

1. **Run as Administrator** - Right-click the exe and select "Run as administrator"
2. **Click Scan** - The app will analyze your system and show cleanable items
3. **Select Items** - Check/uncheck items you want to clean
4. **Click Fix Selected** - The app will clean the selected items

## What Gets Cleaned

| Category | Locations |
|----------|-----------|
| Temp Files | `%TEMP%`, `C:\Windows\Temp` |
| Windows Update | `C:\Windows\SoftwareDistribution\Download` |
| Chrome Cache | `%LOCALAPPDATA%\Google\Chrome\User Data\Default\Cache` |
| Edge Cache | `%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Cache` |
| Firefox Cache | `%LOCALAPPDATA%\Mozilla\Firefox\Profiles\*\cache2` |

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This software is provided as-is. Always ensure you have backups of important data before running cleanup utilities. The authors are not responsible for any data loss.
