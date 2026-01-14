namespace EasyHosts.Views

open System
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open EasyHosts.Domain
open EasyHosts.ViewModels

type HostRecordDialog() as this =
    inherit Window()
    
    let mutable recordViewModel: HostRecordViewModel option = None
    
    do this.InitializeComponent()
    
    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        
        // Wire up button events
        let cancelButton = this.FindControl<Button>("CancelButton")
        let saveButton = this.FindControl<Button>("SaveButton")
        
        cancelButton.Click.Add(this.OnCancelClick)
        saveButton.Click.Add(this.OnSaveClick)
    
    /// Sets the record to edit
    member this.SetRecord(vm: HostRecordViewModel) =
        recordViewModel <- Some vm
        this.DataContext <- vm
    
    /// Gets the edited record
    member this.GetRecord() =
        match recordViewModel with
        | Some vm -> vm
        | None -> HostRecordViewModel.CreateNew()
    
    member private this.OnCancelClick(e: RoutedEventArgs) =
        this.Close(false)
    
    member private this.OnSaveClick(e: RoutedEventArgs) =
        match recordViewModel with
        | Some vm ->
            // Validate the record
            let record = vm.ToRecord()
            match Validation.validateHostRecord record with
            | Ok _ ->
                this.Close(true)
            | Error errors ->
                let validationBorder = this.FindControl<Border>("ValidationBorder")
                let validationMessage = this.FindControl<TextBlock>("ValidationMessage")
                
                let messages = errors |> List.map Validation.getErrorMessage |> String.concat "\n"
                validationMessage.Text <- messages
                validationBorder.IsVisible <- true
        | None ->
            this.Close(false)
