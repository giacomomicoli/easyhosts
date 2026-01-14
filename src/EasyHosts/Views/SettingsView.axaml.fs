namespace EasyHosts.Views

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open Avalonia.Platform.Storage
open EasyHosts.ViewModels

type SettingsView() as this =
    inherit UserControl()
    
    do this.InitializeComponent()
    
    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        
        // Wire up button events
        let browseButton = this.FindControl<Button>("BrowseButton")
        let createBackupButton = this.FindControl<Button>("CreateBackupButton")
        let resetSettingsButton = this.FindControl<Button>("ResetSettingsButton")
        let refreshBackupsButton = this.FindControl<Button>("RefreshBackupsButton")
        let restoreBackupButton = this.FindControl<Button>("RestoreBackupButton")
        let deleteBackupButton = this.FindControl<Button>("DeleteBackupButton")
        let requestAdminButton = this.FindControl<Button>("RequestAdminButton")
        let clearErrorButton = this.FindControl<Button>("ClearErrorButton")
        
        browseButton.Click.Add(this.OnBrowseClick)
        createBackupButton.Click.Add(this.OnCreateBackupClick)
        resetSettingsButton.Click.Add(this.OnResetSettingsClick)
        refreshBackupsButton.Click.Add(this.OnRefreshBackupsClick)
        restoreBackupButton.Click.Add(this.OnRestoreBackupClick)
        deleteBackupButton.Click.Add(this.OnDeleteBackupClick)
        requestAdminButton.Click.Add(this.OnRequestAdminClick)
        clearErrorButton.Click.Add(this.OnClearErrorClick)
    
    member private this.GetViewModel() =
        match this.DataContext with
        | :? SettingsViewModel as vm -> Some vm
        | _ -> None
    
    member private this.OnBrowseClick(e: RoutedEventArgs) =
        task {
            match TopLevel.GetTopLevel(this) with
            | :? Window as window ->
                let storageProvider = window.StorageProvider
                
                let options = FolderPickerOpenOptions()
                options.Title <- "Select Backup Location"
                options.AllowMultiple <- false
                
                let! folders = storageProvider.OpenFolderPickerAsync(options)
                
                if folders.Count > 0 then
                    match this.GetViewModel() with
                    | Some vm -> vm.BackupLocation <- folders.[0].Path.LocalPath
                    | None -> ()
            | _ -> ()
        } |> ignore
    
    member private this.OnCreateBackupClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.CreateBackup()
        | None -> ()
    
    member private this.OnResetSettingsClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.ResetToDefaults()
        | None -> ()
    
    member private this.OnRefreshBackupsClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.LoadBackups()
        | None -> ()
    
    member private this.OnRestoreBackupClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.RestoreBackup()
        | None -> ()
    
    member private this.OnDeleteBackupClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.DeleteBackup()
        | None -> ()
    
    member private this.OnRequestAdminClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.RequestAdminPrivileges()
        | None -> ()
    
    member private this.OnClearErrorClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.ClearError()
        | None -> ()
