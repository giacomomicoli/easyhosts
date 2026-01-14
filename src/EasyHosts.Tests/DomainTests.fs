module EasyHosts.Tests.DomainTests

open System
open Xunit
open EasyHosts.Domain

module HostRecordTests =
    
    [<Fact>]
    let ``Create host record should generate new ID`` () =
        let record = HostRecord.create "127.0.0.1" "localhost" None true
        Assert.NotEqual(Guid.Empty, record.Id)
    
    [<Fact>]
    let ``Create host record should preserve all properties`` () =
        let record = HostRecord.create "192.168.1.1" "myserver" (Some "test comment") false
        
        Assert.Equal("192.168.1.1", record.IpAddress)
        Assert.Equal("myserver", record.Hostname)
        Assert.Equal(Some "test comment", record.Comment)
        Assert.False(record.IsEnabled)
    
    [<Fact>]
    let ``CreateWithId should use provided ID`` () =
        let id = Guid.NewGuid()
        let record = HostRecord.createWithId id "127.0.0.1" "localhost" None true
        
        Assert.Equal(id, record.Id)
    
    [<Fact>]
    let ``Two created records should have different IDs`` () =
        let record1 = HostRecord.create "127.0.0.1" "host1" None true
        let record2 = HostRecord.create "127.0.0.1" "host2" None true
        
        Assert.NotEqual(record1.Id, record2.Id)

module AppSettingsTests =
    
    [<Fact>]
    let ``Default settings should have backup enabled`` () =
        let settings = AppSettings.defaultSettings
        Assert.True(settings.BackupEnabled)
    
    [<Fact>]
    let ``Default settings should have auto backup on change enabled`` () =
        let settings = AppSettings.defaultSettings
        Assert.True(settings.AutoBackupOnChange)
    
    [<Fact>]
    let ``Default settings should have non-empty backup location`` () =
        let settings = AppSettings.defaultSettings
        Assert.False(String.IsNullOrWhiteSpace(settings.BackupLocation))
    
    [<Fact>]
    let ``Default backup location should contain EasyHosts folder`` () =
        let settings = AppSettings.defaultSettings
        Assert.Contains("EasyHosts", settings.BackupLocation)

module ValidationErrorTests =
    
    [<Fact>]
    let ``InvalidIpAddress error message should contain the IP`` () =
        let error = InvalidIpAddress "999.999.999.999"
        let message = Validation.getErrorMessage error
        Assert.Contains("999.999.999.999", message)
    
    [<Fact>]
    let ``InvalidHostname error message should contain the hostname`` () =
        let error = InvalidHostname "-badhost"
        let message = Validation.getErrorMessage error
        Assert.Contains("-badhost", message)
    
    [<Fact>]
    let ``EmptyIpAddress error message should indicate empty`` () =
        let error = EmptyIpAddress
        let message = Validation.getErrorMessage error
        Assert.Contains("empty", message.ToLower())
    
    [<Fact>]
    let ``EmptyHostname error message should indicate empty`` () =
        let error = EmptyHostname
        let message = Validation.getErrorMessage error
        Assert.Contains("empty", message.ToLower())
    
    [<Fact>]
    let ``DuplicateRecord error message should contain provided message`` () =
        let error = DuplicateRecord "Custom duplicate message"
        let message = Validation.getErrorMessage error
        Assert.Equal("Custom duplicate message", message)
