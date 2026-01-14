namespace EasyHosts.ViewModels

open System
open System.Collections.ObjectModel
open EasyHosts.Domain
open EasyHosts.Services

/// ViewModel for the Settings tab
type SettingsViewModel() =
    inherit ViewModelBase()
    
    let mutable settings = SettingsService.loadSettings()
    let mutable backups = ObservableCollection<BackupInfo>()
    let mutable selectedBackup: BackupInfo option = None
    let mutable statusMessage = ""
    let mutable errorMessage = ""
    let mutable isLoading = false
    
    /// Whether backup is enabled
    member this.BackupEnabled 
        with get() = settings.BackupEnabled
        and set(value) = 
            settings <- { settings with BackupEnabled = value }
            this.OnPropertyChanged()
            this.SaveSettings()
    
    /// Backup location path
    member this.BackupLocation 
        with get() = settings.BackupLocation
        and set(value) = 
            settings <- { settings with BackupLocation = value }
            this.OnPropertyChanged()
            this.SaveSettings()
    
    /// Whether to auto-backup on changes
    member this.AutoBackupOnChange 
        with get() = settings.AutoBackupOnChange
        and set(value) = 
            settings <- { settings with AutoBackupOnChange = value }
            this.OnPropertyChanged()
            this.SaveSettings()
    
    /// Collection of available backups
    member this.Backups 
        with get() = backups
        and set(value) = 
            if this.SetProperty(&backups, value) then ()
    
    /// Selected backup
    member this.SelectedBackup 
        with get() = selectedBackup
        and set(value) = 
            if this.SetProperty(&selectedBackup, value) then
                this.OnPropertyChanged("HasBackupSelection")
    
    /// Whether a backup is selected
    member this.HasBackupSelection = selectedBackup.IsSome
    
    /// Status message
    member this.StatusMessage 
        with get() = statusMessage
        and set(value) = 
            if this.SetProperty(&statusMessage, value) then ()
    
    /// Error message
    member this.ErrorMessage 
        with get() = errorMessage
        and set(value) = 
            if this.SetProperty(&errorMessage, value) then
                this.OnPropertyChanged("HasError")
    
    /// Whether there's an error
    member this.HasError = not (String.IsNullOrWhiteSpace(errorMessage))
    
    /// Loading state
    member this.IsLoading 
        with get() = isLoading
        and set(value) = 
            if this.SetProperty(&isLoading, value) then ()
    
    /// Permission status
    member this.PermissionStatus = PermissionsService.getPermissionStatus()
    
    /// Is running as admin
    member this.IsAdmin = PermissionsService.isRunningAsAdmin()
    
    /// Saves the current settings
    member private this.SaveSettings() =
        match SettingsService.saveSettings settings with
        | Ok () -> ()
        | Error err -> this.ErrorMessage <- err
    
    /// Loads the list of backups
    member this.LoadBackups() =
        this.IsLoading <- true
        this.ErrorMessage <- ""
        
        let backupLocation = SettingsService.ensureBackupLocation settings
        match BackupService.listBackups backupLocation with
        | Ok backupList ->
            backups.Clear()
            for backup in backupList do
                backups.Add(backup)
            this.StatusMessage <- $"Found {backupList.Length} backups"
        | Error err ->
            this.ErrorMessage <- err
        
        this.IsLoading <- false
    
    /// Creates a new backup
    member this.CreateBackup() =
        this.IsLoading <- true
        this.ErrorMessage <- ""
        
        let backupLocation = SettingsService.ensureBackupLocation settings
        match BackupService.createBackup backupLocation with
        | Ok backup ->
            backups.Insert(0, backup)
            this.StatusMessage <- $"Backup created: {backup.FileName}"
        | Error err ->
            this.ErrorMessage <- err
        
        this.IsLoading <- false
    
    /// Restores from the selected backup
    member this.RestoreBackup() =
        match selectedBackup with
        | Some backup ->
            this.IsLoading <- true
            this.ErrorMessage <- ""
            
            match BackupService.restoreBackup backup.FilePath with
            | Ok () ->
                this.StatusMessage <- $"Restored from: {backup.FileName}"
            | Error err ->
                this.ErrorMessage <- err
            
            this.IsLoading <- false
        | None ->
            this.ErrorMessage <- "No backup selected"
    
    /// Deletes the selected backup
    member this.DeleteBackup() =
        match selectedBackup with
        | Some backup ->
            this.IsLoading <- true
            this.ErrorMessage <- ""
            
            match BackupService.deleteBackup backup.FilePath with
            | Ok () ->
                backups.Remove(backup) |> ignore
                this.SelectedBackup <- None
                this.StatusMessage <- $"Deleted: {backup.FileName}"
            | Error err ->
                this.ErrorMessage <- err
            
            this.IsLoading <- false
        | None ->
            this.ErrorMessage <- "No backup selected"
    
    /// Resets settings to defaults
    member this.ResetToDefaults() =
        match SettingsService.resetSettings() with
        | Ok () ->
            settings <- SettingsService.loadSettings()
            this.OnPropertyChanged("BackupEnabled")
            this.OnPropertyChanged("BackupLocation")
            this.OnPropertyChanged("AutoBackupOnChange")
            this.StatusMessage <- "Settings reset to defaults"
        | Error err ->
            this.ErrorMessage <- err
    
    /// Requests to run as admin
    member this.RequestAdminPrivileges() =
        if PermissionsService.restartAsAdmin() then
            Environment.Exit(0)
        else
            this.ErrorMessage <- "Failed to restart as Administrator"
    
    /// Clears the error
    member this.ClearError() =
        this.ErrorMessage <- ""
