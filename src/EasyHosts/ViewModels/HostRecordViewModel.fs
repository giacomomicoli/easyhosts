namespace EasyHosts.ViewModels

open System
open EasyHosts.Domain

/// ViewModel representing a single host record for display in the UI
type HostRecordViewModel(record: HostRecord) =
    inherit ViewModelBase()
    
    let mutable id = record.Id
    let mutable ipAddress = record.IpAddress
    let mutable hostname = record.Hostname
    let mutable comment = record.Comment |> Option.defaultValue ""
    let mutable isEnabled = record.IsEnabled
    
    /// Unique identifier for the record
    member _.Id 
        with get() = id
        and set(value) = id <- value
    
    /// IP address (IPv4 or IPv6)
    member this.IpAddress 
        with get() = ipAddress
        and set(value) = 
            if this.SetProperty(&ipAddress, value) then ()
    
    /// Hostname or domain name
    member this.Hostname 
        with get() = hostname
        and set(value) = 
            if this.SetProperty(&hostname, value) then ()
    
    /// Optional comment
    member this.Comment 
        with get() = comment
        and set(value) = 
            if this.SetProperty(&comment, value) then ()
    
    /// Whether the record is enabled (not commented out)
    member this.IsEnabled 
        with get() = isEnabled
        and set(value) = 
            if this.SetProperty(&isEnabled, value) then ()
    
    /// Converts the ViewModel back to a domain record
    member this.ToRecord() : HostRecord =
        {
            Id = id
            IpAddress = ipAddress
            Hostname = hostname
            Comment = if String.IsNullOrWhiteSpace(comment) then None else Some comment
            IsEnabled = isEnabled
        }
    
    /// Creates a ViewModel from a domain record
    static member FromRecord(record: HostRecord) =
        HostRecordViewModel(record)
    
    /// Creates a new empty ViewModel for adding a record
    static member CreateNew() =
        HostRecordViewModel({
            Id = Guid.NewGuid()
            IpAddress = ""
            Hostname = ""
            Comment = None
            IsEnabled = true
        })
