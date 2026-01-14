namespace EasyHosts.Domain

open System
open System.Text.RegularExpressions

module Validation =
    
    /// Validates an IPv4 address
    let private isValidIPv4 (ip: string) =
        if String.IsNullOrWhiteSpace(ip) then false
        else
            let parts = ip.Split('.')
            if parts.Length <> 4 then false
            else
                parts |> Array.forall (fun part ->
                    match Int32.TryParse(part) with
                    | true, num -> num >= 0 && num <= 255
                    | _ -> false
                )
    
    /// Validates an IPv6 address
    let private isValidIPv6 (ip: string) =
        if String.IsNullOrWhiteSpace(ip) then false
        else
            // Simple IPv6 validation pattern
            let ipv6Pattern = @"^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$|^::$|^([0-9a-fA-F]{1,4}:){1,7}:$|^::[0-9a-fA-F]{1,4}(:[0-9a-fA-F]{1,4}){0,6}$|^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|^[0-9a-fA-F]{1,4}:(:[0-9a-fA-F]{1,4}){1,6}$|^::1$"
            Regex.IsMatch(ip, ipv6Pattern)
    
    /// Validates an IP address (IPv4 or IPv6)
    let validateIpAddress (ip: string) : Result<string, ValidationError> =
        if String.IsNullOrWhiteSpace(ip) then
            Error EmptyIpAddress
        elif isValidIPv4 ip || isValidIPv6 ip then
            Ok ip
        else
            Error (InvalidIpAddress ip)
    
    /// Validates a hostname
    let validateHostname (hostname: string) : Result<string, ValidationError> =
        if String.IsNullOrWhiteSpace(hostname) then
            Error EmptyHostname
        else
            // Hostname validation:
            // - Can contain alphanumeric characters, hyphens, and dots
            // - Each label (part between dots) must start and end with alphanumeric
            // - Total length <= 253 characters
            // - Each label <= 63 characters
            let hostname = hostname.Trim().ToLowerInvariant()
            
            if hostname.Length > 253 then
                Error (InvalidHostname "Hostname too long (max 253 characters)")
            else
                let labels = hostname.Split('.')
                let isValidLabel (label: string) =
                    if label.Length = 0 || label.Length > 63 then false
                    elif label.StartsWith("-") || label.EndsWith("-") then false
                    else
                        label |> Seq.forall (fun c -> 
                            Char.IsLetterOrDigit(c) || c = '-'
                        )
                
                if labels |> Array.forall isValidLabel then
                    Ok hostname
                else
                    Error (InvalidHostname hostname)
    
    /// Validates a complete host record
    let validateHostRecord (record: HostRecord) : Result<HostRecord, ValidationError list> =
        let ipResult = validateIpAddress record.IpAddress
        let hostnameResult = validateHostname record.Hostname
        
        match ipResult, hostnameResult with
        | Ok _, Ok _ -> Ok record
        | Error e1, Ok _ -> Error [e1]
        | Ok _, Error e2 -> Error [e2]
        | Error e1, Error e2 -> Error [e1; e2]
    
    /// Checks for duplicate records
    let checkDuplicate (existing: HostRecord list) (newRecord: HostRecord) : Result<HostRecord, ValidationError> =
        let isDuplicate = 
            existing 
            |> List.exists (fun r -> 
                r.Id <> newRecord.Id && 
                r.Hostname.Equals(newRecord.Hostname, StringComparison.OrdinalIgnoreCase) &&
                r.IpAddress = newRecord.IpAddress
            )
        
        if isDuplicate then
            Error (DuplicateRecord $"Record for {newRecord.Hostname} -> {newRecord.IpAddress} already exists")
        else
            Ok newRecord
    
    /// Gets a human-readable error message
    let getErrorMessage (error: ValidationError) : string =
        match error with
        | InvalidIpAddress ip -> $"Invalid IP address: {ip}"
        | InvalidHostname host -> $"Invalid hostname: {host}"
        | EmptyIpAddress -> "IP address cannot be empty"
        | EmptyHostname -> "Hostname cannot be empty"
        | DuplicateRecord msg -> msg
