namespace EasyHosts.ViewModels

open EasyHosts.Services

/// Main window ViewModel managing tabs
type MainWindowViewModel() =
    inherit ViewModelBase()
    
    let mutable selectedTabIndex = 0
    let landingViewModel = LandingViewModel()
    let manageHostsViewModel = ManageHostsViewModel()
    let settingsViewModel = SettingsViewModel()
    
    /// Currently selected tab index
    member this.SelectedTabIndex 
        with get() = selectedTabIndex
        and set(value) = 
            if this.SetProperty(&selectedTabIndex, value) then
                // Refresh data when switching tabs
                match value with
                | 1 -> manageHostsViewModel.LoadRecords()
                | 2 -> settingsViewModel.LoadBackups()
                | _ -> ()
    
    /// Landing page ViewModel
    member _.LandingViewModel = landingViewModel
    
    /// Manage Hosts tab ViewModel
    member _.ManageHostsViewModel = manageHostsViewModel
    
    /// Settings tab ViewModel
    member _.SettingsViewModel = settingsViewModel
    
    /// Window title
    member _.Title = 
        let adminIndicator = 
            if PermissionsService.isRunningAsAdmin() then " [Administrator]"
            else ""
        $"EasyHosts{adminIndicator}"
    
    /// Initializes the ViewModel
    member this.Initialize() =
        // Load initial data
        manageHostsViewModel.LoadRecords()
        settingsViewModel.LoadBackups()
