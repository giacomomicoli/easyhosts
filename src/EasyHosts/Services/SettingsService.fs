namespace EasyHosts.Services

open System
open System.IO
open System.Text.Json
open EasyHosts.Domain

module SettingsService =
    
    /// Settings file location
    let private settingsFilePath = 
        let appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        Path.Combine(appData, "EasyHosts", "settings.json")
    
    /// Ensures the settings directory exists
    let private ensureSettingsDirectory () =
        let dir = Path.GetDirectoryName(settingsFilePath)
        if not (Directory.Exists(dir)) then
            Directory.CreateDirectory(dir) |> ignore
    
    /// JSON serializer options
    let private jsonOptions = 
        let options = JsonSerializerOptions()
        options.WriteIndented <- true
        options
    
    /// Loads settings from disk
    let loadSettings () : AppSettings =
        try
            if File.Exists(settingsFilePath) then
                let json = File.ReadAllText(settingsFilePath)
                JsonSerializer.Deserialize<AppSettings>(json, jsonOptions)
            else
                AppSettings.defaultSettings
        with
        | _ -> AppSettings.defaultSettings
    
    /// Saves settings to disk
    let saveSettings (settings: AppSettings) : Result<unit, string> =
        try
            ensureSettingsDirectory()
            let json = JsonSerializer.Serialize(settings, jsonOptions)
            File.WriteAllText(settingsFilePath, json)
            Ok ()
        with
        | ex -> Error $"Failed to save settings: {ex.Message}"
    
    /// Updates a specific setting
    let updateSetting (update: AppSettings -> AppSettings) : Result<AppSettings, string> =
        let current = loadSettings()
        let updated = update current
        match saveSettings updated with
        | Ok () -> Ok updated
        | Error err -> Error err
    
    /// Resets settings to defaults
    let resetSettings () : Result<unit, string> =
        saveSettings AppSettings.defaultSettings
    
    /// Gets the current backup location, creating it if necessary
    let ensureBackupLocation (settings: AppSettings) : string =
        let location = settings.BackupLocation
        if not (Directory.Exists(location)) then
            try
                Directory.CreateDirectory(location) |> ignore
            with _ -> ()
        location
