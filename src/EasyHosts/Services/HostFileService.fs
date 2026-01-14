namespace EasyHosts.Services

open System
open System.IO
open System.Text.RegularExpressions
open EasyHosts.Domain

module HostFileService =
    
    /// Windows hosts file path
    let hostsFilePath = @"C:\Windows\System32\drivers\etc\hosts"
    
    /// Parses a single line from the hosts file
    let private parseLine (line: string) : HostRecord option =
        if String.IsNullOrWhiteSpace(line) then
            None
        else
            let trimmed = line.Trim()
            
            // Check if line is entirely a comment
            if trimmed.StartsWith("#") then
                // Check if it's a disabled entry (commented out host record)
                let uncommented = trimmed.Substring(1).TrimStart()
                let parts = Regex.Split(uncommented, @"\s+")
                
                if parts.Length >= 2 then
                    let ip = parts.[0]
                    let hostname = parts.[1]
                    
                    // Validate if this looks like a host record
                    match Validation.validateIpAddress ip with
                    | Ok _ ->
                        let comment = 
                            if parts.Length > 2 then
                                let commentStart = 
                                    parts 
                                    |> Array.skip 2 
                                    |> Array.tryFindIndex (fun p -> p.StartsWith("#"))
                                match commentStart with
                                | Some idx -> 
                                    Some (parts |> Array.skip (2 + idx + 1) |> String.concat " ")
                                | None -> None
                            else None
                        
                        Some {
                            Id = Guid.NewGuid()
                            IpAddress = ip
                            Hostname = hostname
                            Comment = comment
                            IsEnabled = false
                        }
                    | Error _ -> None
                else
                    None
            else
                // Active entry
                let parts = Regex.Split(trimmed, @"\s+")
                
                if parts.Length >= 2 then
                    let ip = parts.[0]
                    let hostname = parts.[1]
                    
                    // Extract comment if present
                    let comment =
                        let fullLine = trimmed
                        let commentIndex = fullLine.IndexOf('#')
                        if commentIndex > 0 then
                            Some (fullLine.Substring(commentIndex + 1).Trim())
                        else
                            None
                    
                    match Validation.validateIpAddress ip with
                    | Ok _ ->
                        Some {
                            Id = Guid.NewGuid()
                            IpAddress = ip
                            Hostname = hostname
                            Comment = comment
                            IsEnabled = true
                        }
                    | Error _ -> None
                else
                    None
    
    /// Reads and parses the hosts file
    let readHostsFile () : Result<HostRecord list, string> =
        try
            if File.Exists(hostsFilePath) then
                let lines = File.ReadAllLines(hostsFilePath)
                let records = 
                    lines 
                    |> Array.choose parseLine
                    |> Array.toList
                Ok records
            else
                Error $"Hosts file not found at: {hostsFilePath}"
        with
        | :? UnauthorizedAccessException ->
            Error "Access denied. Please run the application as Administrator."
        | ex ->
            Error $"Error reading hosts file: {ex.Message}"
    
    /// Converts a host record to a hosts file line
    let private recordToLine (record: HostRecord) : string =
        let baseLine = 
            let entry = $"{record.IpAddress}\t{record.Hostname}"
            match record.Comment with
            | Some comment -> $"{entry}\t# {comment}"
            | None -> entry
        
        if record.IsEnabled then
            baseLine
        else
            $"# {baseLine}"
    
    /// Writes records to the hosts file
    let writeHostsFile (records: HostRecord list) : Result<unit, string> =
        try
            // Read the original file to preserve comments and formatting
            let originalLines = 
                if File.Exists(hostsFilePath) then
                    File.ReadAllLines(hostsFilePath)
                else
                    [||]
            
            // Preserve header comments (lines that start with # and are not disabled entries)
            let headerComments =
                originalLines
                |> Array.takeWhile (fun line ->
                    let trimmed = line.Trim()
                    if String.IsNullOrWhiteSpace(trimmed) then true
                    elif trimmed.StartsWith("#") then
                        // Check if it's NOT a disabled host entry
                        let uncommented = trimmed.Substring(1).TrimStart()
                        let parts = Regex.Split(uncommented, @"\s+")
                        if parts.Length >= 2 then
                            match Validation.validateIpAddress parts.[0] with
                            | Ok _ -> false
                            | Error _ -> true
                        else
                            true
                    else
                        false
                )
            
            // Build the new file content
            let newLines = 
                [|
                    yield! headerComments
                    if headerComments.Length > 0 && not (String.IsNullOrWhiteSpace(headerComments.[headerComments.Length - 1])) then
                        yield ""
                    yield "# Managed by EasyHosts"
                    yield ""
                    for record in records do
                        yield recordToLine record
                |]
            
            File.WriteAllLines(hostsFilePath, newLines)
            Ok ()
        with
        | :? UnauthorizedAccessException ->
            Error "Access denied. Please run the application as Administrator."
        | ex ->
            Error $"Error writing hosts file: {ex.Message}"
    
    /// Adds a new record to the hosts file
    let addRecord (record: HostRecord) : Result<unit, string> =
        match readHostsFile () with
        | Ok records ->
            // Validate the new record
            match Validation.validateHostRecord record with
            | Ok validRecord ->
                match Validation.checkDuplicate records validRecord with
                | Ok _ ->
                    let newRecords = records @ [validRecord]
                    writeHostsFile newRecords
                | Error err ->
                    Error (Validation.getErrorMessage err)
            | Error errors ->
                Error (errors |> List.map Validation.getErrorMessage |> String.concat "; ")
        | Error err -> Error err
    
    /// Updates an existing record
    let updateRecord (record: HostRecord) : Result<unit, string> =
        match readHostsFile () with
        | Ok records ->
            match Validation.validateHostRecord record with
            | Ok validRecord ->
                let otherRecords = records |> List.filter (fun r -> r.Id <> record.Id)
                match Validation.checkDuplicate otherRecords validRecord with
                | Ok _ ->
                    let newRecords = 
                        records 
                        |> List.map (fun r -> if r.Id = record.Id then validRecord else r)
                    writeHostsFile newRecords
                | Error err ->
                    Error (Validation.getErrorMessage err)
            | Error errors ->
                Error (errors |> List.map Validation.getErrorMessage |> String.concat "; ")
        | Error err -> Error err
    
    /// Removes a record from the hosts file
    let removeRecord (recordId: Guid) : Result<unit, string> =
        match readHostsFile () with
        | Ok records ->
            let newRecords = records |> List.filter (fun r -> r.Id <> recordId)
            writeHostsFile newRecords
        | Error err -> Error err
    
    /// Toggles a record's enabled state
    let toggleRecord (recordId: Guid) : Result<unit, string> =
        match readHostsFile () with
        | Ok records ->
            let newRecords = 
                records 
                |> List.map (fun r -> 
                    if r.Id = recordId then 
                        { r with IsEnabled = not r.IsEnabled }
                    else r
                )
            writeHostsFile newRecords
        | Error err -> Error err
