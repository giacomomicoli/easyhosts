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
        this.DataContext :?> SettingsViewModel
    
    member private this.OnBrowseClick(e: RoutedEventArgs) =
        task {
            let window = TopLevel.GetTopLevel(this) :?> Window
            let storageProvider = window.StorageProvider
            
            let options = FolderPickerOpenOptions()
            options.Title <- "Select Backup Location"
            options.AllowMultiple <- false
            
            let! folders = storageProvider.OpenFolderPickerAsync(options)
            
            if folders.Count > 0 then
                let vm = this.GetViewModel()
                vm.BackupLocation <- folders.[0].Path.LocalPath
        } |> ignore
    
    member private this.OnCreateBackupClick(e: RoutedEventArgs) =
        let vm = this.GetViewModel()
        vm.CreateBackup()
    
    member private this.OnResetSettingsClick(e: RoutedEventArgs) =
        let vm = this.GetViewModel()
        vm.ResetToDefaults()
    
    member private this.OnRefreshBackupsClick(e: RoutedEventArgs) =
        let vm = this.GetViewModel()
        vm.LoadBackups()
    
    member private this.OnRestoreBackupClick(e: RoutedEventArgs) =
        let vm = this.GetViewModel()
        vm.RestoreBackup()
    
    member private this.OnDeleteBackupClick(e: RoutedEventArgs) =
        let vm = this.GetViewModel()
        vm.DeleteBackup()
    
    member private this.OnRequestAdminClick(e: RoutedEventArgs) =
        let vm = this.GetViewModel()
        vm.RequestAdminPrivileges()
    
    member private this.OnClearErrorClick(e: RoutedEventArgs) =
        let vm = this.GetViewModel()
        vm.ClearError()
