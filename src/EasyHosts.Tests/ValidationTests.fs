module EasyHosts.Tests.ValidationTests

open System
open Xunit
open EasyHosts.Domain

module IpAddressValidationTests =
    
    [<Fact>]
    let ``Valid IPv4 address should pass validation`` () =
        let result = Validation.validateIpAddress "192.168.1.1"
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Valid localhost IPv4 should pass validation`` () =
        let result = Validation.validateIpAddress "127.0.0.1"
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Valid edge case IPv4 (0.0.0.0) should pass validation`` () =
        let result = Validation.validateIpAddress "0.0.0.0"
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Valid edge case IPv4 (255.255.255.255) should pass validation`` () =
        let result = Validation.validateIpAddress "255.255.255.255"
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Invalid IPv4 with out-of-range octet should fail validation`` () =
        let result = Validation.validateIpAddress "192.168.1.256"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Invalid IPv4 with negative octet should fail validation`` () =
        let result = Validation.validateIpAddress "192.168.-1.1"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Invalid IPv4 with missing octets should fail validation`` () =
        let result = Validation.validateIpAddress "192.168.1"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Invalid IPv4 with extra octets should fail validation`` () =
        let result = Validation.validateIpAddress "192.168.1.1.1"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Invalid IPv4 with non-numeric should fail validation`` () =
        let result = Validation.validateIpAddress "192.168.abc.1"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Empty IP address should fail validation`` () =
        let result = Validation.validateIpAddress ""
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Whitespace IP address should fail validation`` () =
        let result = Validation.validateIpAddress "   "
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Valid IPv6 loopback should pass validation`` () =
        let result = Validation.validateIpAddress "::1"
        Assert.True(Result.isOk result)

module HostnameValidationTests =
    
    [<Fact>]
    let ``Valid simple hostname should pass validation`` () =
        let result = Validation.validateHostname "localhost"
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Valid hostname with subdomain should pass validation`` () =
        let result = Validation.validateHostname "api.example.com"
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Valid hostname with hyphen should pass validation`` () =
        let result = Validation.validateHostname "my-server.local"
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Valid hostname with numbers should pass validation`` () =
        let result = Validation.validateHostname "server1.test.com"
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Invalid hostname starting with hyphen should fail validation`` () =
        let result = Validation.validateHostname "-invalid.com"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Invalid hostname ending with hyphen should fail validation`` () =
        let result = Validation.validateHostname "invalid-.com"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Invalid hostname with underscore should fail validation`` () =
        let result = Validation.validateHostname "invalid_host.com"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Invalid hostname with space should fail validation`` () =
        let result = Validation.validateHostname "invalid host.com"
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Empty hostname should fail validation`` () =
        let result = Validation.validateHostname ""
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Whitespace hostname should fail validation`` () =
        let result = Validation.validateHostname "   "
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Hostname too long (over 253 chars) should fail validation`` () =
        let longHostname = String.replicate 260 "a"
        let result = Validation.validateHostname longHostname
        Assert.True(Result.isError result)

module HostRecordValidationTests =
    
    [<Fact>]
    let ``Valid host record should pass validation`` () =
        let record = {
            Id = Guid.NewGuid()
            IpAddress = "127.0.0.1"
            Hostname = "localhost"
            Comment = Some "Test entry"
            IsEnabled = true
        }
        let result = Validation.validateHostRecord record
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Host record with invalid IP should fail validation`` () =
        let record = {
            Id = Guid.NewGuid()
            IpAddress = "invalid"
            Hostname = "localhost"
            Comment = None
            IsEnabled = true
        }
        let result = Validation.validateHostRecord record
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Host record with invalid hostname should fail validation`` () =
        let record = {
            Id = Guid.NewGuid()
            IpAddress = "127.0.0.1"
            Hostname = "-invalid"
            Comment = None
            IsEnabled = true
        }
        let result = Validation.validateHostRecord record
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Host record with both invalid IP and hostname should return multiple errors`` () =
        let record = {
            Id = Guid.NewGuid()
            IpAddress = "invalid"
            Hostname = "-invalid"
            Comment = None
            IsEnabled = true
        }
        match Validation.validateHostRecord record with
        | Error errors -> Assert.Equal(2, errors.Length)
        | Ok _ -> Assert.Fail("Should have failed validation")

module DuplicateCheckTests =
    
    [<Fact>]
    let ``Non-duplicate record should pass duplicate check`` () =
        let existing = [
            { Id = Guid.NewGuid(); IpAddress = "127.0.0.1"; Hostname = "localhost"; Comment = None; IsEnabled = true }
        ]
        let newRecord = { Id = Guid.NewGuid(); IpAddress = "192.168.1.1"; Hostname = "server"; Comment = None; IsEnabled = true }
        
        let result = Validation.checkDuplicate existing newRecord
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Duplicate record should fail duplicate check`` () =
        let existingId = Guid.NewGuid()
        let existing = [
            { Id = existingId; IpAddress = "127.0.0.1"; Hostname = "localhost"; Comment = None; IsEnabled = true }
        ]
        let newRecord = { Id = Guid.NewGuid(); IpAddress = "127.0.0.1"; Hostname = "localhost"; Comment = None; IsEnabled = true }
        
        let result = Validation.checkDuplicate existing newRecord
        Assert.True(Result.isError result)
    
    [<Fact>]
    let ``Same ID record should not be flagged as duplicate (editing)`` () =
        let recordId = Guid.NewGuid()
        let existing = [
            { Id = recordId; IpAddress = "127.0.0.1"; Hostname = "localhost"; Comment = None; IsEnabled = true }
        ]
        let editedRecord = { Id = recordId; IpAddress = "127.0.0.1"; Hostname = "localhost"; Comment = Some "Updated"; IsEnabled = true }
        
        let result = Validation.checkDuplicate existing editedRecord
        Assert.True(Result.isOk result)
    
    [<Fact>]
    let ``Same hostname but different IP should not be duplicate`` () =
        let existing = [
            { Id = Guid.NewGuid(); IpAddress = "127.0.0.1"; Hostname = "localhost"; Comment = None; IsEnabled = true }
        ]
        let newRecord = { Id = Guid.NewGuid(); IpAddress = "192.168.1.1"; Hostname = "localhost"; Comment = None; IsEnabled = true }
        
        let result = Validation.checkDuplicate existing newRecord
        Assert.True(Result.isOk result)
