namespace EasyHosts.Services

open System
open System.Diagnostics
open System.IO
open System.Security.Principal
open System.Runtime.InteropServices

module PermissionsService =
    
    /// Checks if the current process is running with administrator privileges
    let isRunningAsAdmin () : bool =
        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
            use identity = WindowsIdentity.GetCurrent()
            let principal = WindowsPrincipal(identity)
            principal.IsInRole(WindowsBuiltInRole.Administrator)
        else
            // On non-Windows platforms, check if running as root
            Environment.GetEnvironmentVariable("USER") = "root"
    
    /// Checks if the hosts file is writable
    let canWriteHostsFile () : bool =
        try
            let hostsPath = HostFileService.hostsFilePath
            if File.Exists(hostsPath) then
                use fs = File.Open(hostsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                fs.Close()
                true
            else
                false
        with
        | _ -> false
    
    /// Restarts the application with elevated privileges
    let restartAsAdmin () : bool =
        try
            let processInfo = ProcessStartInfo()
            processInfo.UseShellExecute <- true
            processInfo.FileName <- Environment.ProcessPath
            processInfo.Verb <- "runas" // This triggers UAC prompt on Windows
            
            Process.Start(processInfo) |> ignore
            true
        with
        | _ -> false
    
    /// Gets the current permission status
    let getPermissionStatus () : string =
        if isRunningAsAdmin() then
            "Running as Administrator"
        elif canWriteHostsFile() then
            "Hosts file is writable"
        else
            "Elevated privileges required"
    
    /// Checks if elevation is needed and provides guidance
    let checkElevation () : Result<unit, string> =
        if isRunningAsAdmin() then
            Ok ()
        elif canWriteHostsFile() then
            Ok ()
        else
            Error "This operation requires administrator privileges. Please restart the application as Administrator."
