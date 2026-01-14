namespace EasyHosts.ViewModels

/// ViewModel for the Landing page
type LandingViewModel() =
    inherit ViewModelBase()
    
    /// Application name
    member _.AppName = "EasyHosts"
    
    /// Application version
    member _.Version = "1.0.0"
    
    /// Application description
    member _.Description = 
        "EasyHosts is a quality of life tool that allows Windows users to easily manage their hosts file. " +
        "No more navigating to system folders or editing files manually - manage your host records with a " +
        "clean, modern interface."
    
    /// Features list
    member _.Features = [
        "📝 Add, edit, and remove host records with ease"
        "✅ Enable or disable entries without deleting them"
        "🔒 Automatic backup before changes"
        "💾 Restore from previous backups"
        "✨ Input validation for IP addresses and hostnames"
        "🛡️ Safe permission handling"
    ]
    
    /// Quick start instructions
    member _.QuickStart = 
        "Get started by clicking on the 'Manage Hosts' tab to view and edit your host records, " +
        "or visit 'Settings' to configure backup options."
    
    /// Copyright info
    member _.Copyright = "© 2026 EasyHosts"
