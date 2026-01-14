namespace EasyHosts.Services

open System
open System.IO
open EasyHosts.Domain

module BackupService =
    
    /// Ensures the backup directory exists
    let private ensureBackupDirectory (path: string) =
        if not (Directory.Exists(path)) then
            Directory.CreateDirectory(path) |> ignore
    
    /// Generates a backup filename with timestamp
    let private generateBackupFileName () =
        let timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
        $"hosts_backup_{timestamp}.txt"
    
    /// Creates a backup of the current hosts file
    let createBackup (backupLocation: string) : Result<BackupInfo, string> =
        try
            ensureBackupDirectory backupLocation
            
            let fileName = generateBackupFileName()
            let backupPath = Path.Combine(backupLocation, fileName)
            
            if File.Exists(HostFileService.hostsFilePath) then
                File.Copy(HostFileService.hostsFilePath, backupPath, true)
                
                // Count records in the backup
                let recordCount = 
                    match HostFileService.readHostsFile() with
                    | Ok records -> records.Length
                    | Error _ -> 0
                
                Ok {
                    Id = Guid.NewGuid()
                    FileName = fileName
                    CreatedAt = DateTime.Now
                    FilePath = backupPath
                    RecordCount = recordCount
                }
            else
                Error "Hosts file not found"
        with
        | ex -> Error $"Failed to create backup: {ex.Message}"
    
    /// Lists all available backups
    let listBackups (backupLocation: string) : Result<BackupInfo list, string> =
        try
            if Directory.Exists(backupLocation) then
                let files = 
                    Directory.GetFiles(backupLocation, "hosts_backup_*.txt")
                    |> Array.map (fun filePath ->
                        let fileInfo = FileInfo(filePath)
                        {
                            Id = Guid.NewGuid()
                            FileName = fileInfo.Name
                            CreatedAt = fileInfo.CreationTime
                            FilePath = filePath
                            RecordCount = 
                                try
                                    File.ReadAllLines(filePath)
                                    |> Array.filter (fun line -> 
                                        let trimmed = line.Trim()
                                        not (String.IsNullOrWhiteSpace(trimmed)) &&
                                        not (trimmed.StartsWith("#") && 
                                             not (trimmed.Contains("\t") || trimmed.Contains("  ")))
                                    )
                                    |> Array.length
                                with _ -> 0
                        }
                    )
                    |> Array.sortByDescending (fun b -> b.CreatedAt)
                    |> Array.toList
                Ok files
            else
                Ok []
        with
        | ex -> Error $"Failed to list backups: {ex.Message}"
    
    /// Restores the hosts file from a backup
    let restoreBackup (backupPath: string) : Result<unit, string> =
        try
            if File.Exists(backupPath) then
                File.Copy(backupPath, HostFileService.hostsFilePath, true)
                Ok ()
            else
                Error $"Backup file not found: {backupPath}"
        with
        | :? UnauthorizedAccessException ->
            Error "Access denied. Please run the application as Administrator."
        | ex -> Error $"Failed to restore backup: {ex.Message}"
    
    /// Deletes a backup file
    let deleteBackup (backupPath: string) : Result<unit, string> =
        try
            if File.Exists(backupPath) then
                File.Delete(backupPath)
                Ok ()
            else
                Error "Backup file not found"
        with
        | ex -> Error $"Failed to delete backup: {ex.Message}"
    
    /// Cleans up old backups, keeping only the specified number of recent ones
    let cleanupOldBackups (backupLocation: string) (keepCount: int) : Result<int, string> =
        try
            match listBackups backupLocation with
            | Ok backups ->
                let toDelete = 
                    backups 
                    |> List.skip (min keepCount backups.Length)
                
                toDelete |> List.iter (fun b -> 
                    try File.Delete(b.FilePath) with _ -> ()
                )
                
                Ok toDelete.Length
            | Error err -> Error err
        with
        | ex -> Error $"Failed to cleanup backups: {ex.Message}"
