namespace EasyHosts.ViewModels

open System
open System.Collections.ObjectModel
open EasyHosts.Domain
open EasyHosts.Services

/// ViewModel for the Manage Hosts tab
type ManageHostsViewModel() =
    inherit ViewModelBase()
    
    let mutable records = ObservableCollection<HostRecordViewModel>()
    let mutable selectedRecord: HostRecordViewModel option = None
    let mutable statusMessage = ""
    let mutable isLoading = false
    let mutable errorMessage = ""
    
    /// Collection of host records
    member this.Records 
        with get() = records
        and set(value) = 
            if this.SetProperty(&records, value) then ()
    
    /// Currently selected record
    member this.SelectedRecord 
        with get() = selectedRecord
        and set(value) = 
            if this.SetProperty(&selectedRecord, value) then
                this.OnPropertyChanged("HasSelection")
    
    /// Status message for user feedback
    member this.StatusMessage 
        with get() = statusMessage
        and set(value) = 
            if this.SetProperty(&statusMessage, value) then ()
    
    /// Error message if any
    member this.ErrorMessage 
        with get() = errorMessage
        and set(value) = 
            if this.SetProperty(&errorMessage, value) then
                this.OnPropertyChanged("HasError")
    
    /// Loading indicator
    member this.IsLoading 
        with get() = isLoading
        and set(value) = 
            if this.SetProperty(&isLoading, value) then ()
    
    /// Whether a record is selected
    member this.HasSelection = selectedRecord.IsSome
    
    /// Whether there's an error
    member this.HasError = not (String.IsNullOrWhiteSpace(errorMessage))
    
    /// Loads records from the hosts file
    member this.LoadRecords() =
        this.IsLoading <- true
        this.ErrorMessage <- ""
        
        match HostFileService.readHostsFile() with
        | Ok hostRecords ->
            records.Clear()
            for record in hostRecords do
                records.Add(HostRecordViewModel.FromRecord(record))
            this.StatusMessage <- $"Loaded {hostRecords.Length} records"
        | Error err ->
            this.ErrorMessage <- err
            this.StatusMessage <- "Failed to load records"
        
        this.IsLoading <- false
    
    /// Adds a new host record
    member this.AddRecord(vm: HostRecordViewModel) =
        let settings = SettingsService.loadSettings()
        
        // Create backup if enabled
        if settings.AutoBackupOnChange then
            let backupLocation = SettingsService.ensureBackupLocation settings
            match BackupService.createBackup backupLocation with
            | Ok _ -> ()
            | Error err -> this.StatusMessage <- $"Warning: Backup failed - {err}"
        
        let record = vm.ToRecord()
        match HostFileService.addRecord record with
        | Ok () ->
            records.Add(vm)
            this.StatusMessage <- $"Added: {record.Hostname}"
            this.ErrorMessage <- ""
            true
        | Error err ->
            this.ErrorMessage <- err
            false
    
    /// Updates an existing host record
    member this.UpdateRecord(vm: HostRecordViewModel) =
        let settings = SettingsService.loadSettings()
        
        // Create backup if enabled
        if settings.AutoBackupOnChange then
            let backupLocation = SettingsService.ensureBackupLocation settings
            match BackupService.createBackup backupLocation with
            | Ok _ -> ()
            | Error err -> this.StatusMessage <- $"Warning: Backup failed - {err}"
        
        let record = vm.ToRecord()
        match HostFileService.updateRecord record with
        | Ok () ->
            // Update in collection
            let index = 
                records 
                |> Seq.tryFindIndex (fun r -> r.Id = vm.Id)
            
            match index with
            | Some i -> records.[i] <- vm
            | None -> ()
            
            this.StatusMessage <- $"Updated: {record.Hostname}"
            this.ErrorMessage <- ""
            true
        | Error err ->
            this.ErrorMessage <- err
            false
    
    /// Removes a host record
    member this.RemoveRecord(vm: HostRecordViewModel) =
        let settings = SettingsService.loadSettings()
        
        // Create backup if enabled
        if settings.AutoBackupOnChange then
            let backupLocation = SettingsService.ensureBackupLocation settings
            match BackupService.createBackup backupLocation with
            | Ok _ -> ()
            | Error err -> this.StatusMessage <- $"Warning: Backup failed - {err}"
        
        match HostFileService.removeRecord vm.Id with
        | Ok () ->
            records.Remove(vm) |> ignore
            this.SelectedRecord <- None
            this.StatusMessage <- $"Removed: {vm.Hostname}"
            this.ErrorMessage <- ""
            true
        | Error err ->
            this.ErrorMessage <- err
            false
    
    /// Toggles a record's enabled state
    member this.ToggleRecord(vm: HostRecordViewModel) =
        let settings = SettingsService.loadSettings()
        
        // Create backup if enabled
        if settings.AutoBackupOnChange then
            let backupLocation = SettingsService.ensureBackupLocation settings
            match BackupService.createBackup backupLocation with
            | Ok _ -> ()
            | Error err -> this.StatusMessage <- $"Warning: Backup failed - {err}"
        
        match HostFileService.toggleRecord vm.Id with
        | Ok () ->
            vm.IsEnabled <- not vm.IsEnabled
            let state = if vm.IsEnabled then "enabled" else "disabled"
            this.StatusMessage <- $"{vm.Hostname} {state}"
            this.ErrorMessage <- ""
            true
        | Error err ->
            this.ErrorMessage <- err
            false
    
    /// Clears the error message
    member this.ClearError() =
        this.ErrorMessage <- ""
