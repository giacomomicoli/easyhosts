namespace EasyHosts.Domain

open System

/// Represents a single host file record
type HostRecord = {
    Id: Guid
    IpAddress: string
    Hostname: string
    Comment: string option
    IsEnabled: bool
}

/// Represents validation errors
type ValidationError =
    | InvalidIpAddress of string
    | InvalidHostname of string
    | EmptyIpAddress
    | EmptyHostname
    | DuplicateRecord of string

/// Result type for operations
type OperationResult<'T> =
    | Success of 'T
    | Failure of string list

/// Application settings
type AppSettings = {
    BackupEnabled: bool
    BackupLocation: string
    AutoBackupOnChange: bool
}

/// Backup metadata
type BackupInfo = {
    Id: Guid
    FileName: string
    CreatedAt: DateTime
    FilePath: string
    RecordCount: int
}

module HostRecord =
    /// Creates a new host record
    let create ipAddress hostname comment isEnabled =
        {
            Id = Guid.NewGuid()
            IpAddress = ipAddress
            Hostname = hostname
            Comment = comment
            IsEnabled = isEnabled
        }
    
    /// Creates a host record with a specific ID (for editing)
    let createWithId id ipAddress hostname comment isEnabled =
        {
            Id = id
            IpAddress = ipAddress
            Hostname = hostname
            Comment = comment
            IsEnabled = isEnabled
        }

module AppSettings =
    /// Default settings
    let defaultSettings = {
        BackupEnabled = true
        BackupLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EasyHosts\\Backups"
        AutoBackupOnChange = true
    }
