# EasyHosts

A quality of life tool for Windows users to easily manage their hosts file.

## Features

- **Host File Management**: Add, edit, and remove host records with ease
- **Enable/Disable Entries**: Toggle entries without deleting them
- **Automatic Backup**: Backup before making changes
- **Restore from Backup**: Easily restore previous configurations
- **Input Validation**: Validates IP addresses and hostnames
- **Permission Handling**: Automatically requests administrator privileges

## Requirements

- Windows 10 or later
- .NET 10 Runtime (included in single-file build)

## Building

### Prerequisites

- .NET 10 SDK
- Visual Studio 2024 or VS Code with C#/F# extensions

### Build Commands

```bash
# Restore packages
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Run tests
dotnet test

# Publish as single executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The published executable will be in:
`EasyHosts/bin/Release/net10.0/win-x64/publish/EasyHosts.exe`

## Project Structure

```
src/
├── EasyHosts/
│   ├── Domain/           # Domain types and validation
│   │   ├── Types.fs      # Core domain models
│   │   └── Validation.fs # Input validation logic
│   ├── Services/         # Application services
│   │   ├── HostFileService.fs    # Hosts file operations
│   │   ├── BackupService.fs      # Backup/restore functionality
│   │   ├── PermissionsService.fs # Permission handling
│   │   └── SettingsService.fs    # Application settings
│   ├── ViewModels/       # MVVM ViewModels
│   ├── Views/            # Avalonia XAML views
│   ├── App.axaml         # Application definition
│   └── Program.fs        # Entry point
├── EasyHosts.Tests/      # Unit tests
└── EasyHosts.sln         # Solution file
```

## Usage

1. Run the application as Administrator (required to modify the hosts file)
2. Navigate to "Manage Hosts" to view and edit host records
3. Use "Settings" to configure backup options

## License

© 2026 EasyHosts
