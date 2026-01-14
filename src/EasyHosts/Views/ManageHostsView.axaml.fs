namespace EasyHosts.Views

open System
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open EasyHosts.ViewModels

type ManageHostsView() as this =
    inherit UserControl()
    
    do this.InitializeComponent()
    
    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        
        // Wire up button events
        let addButton = this.FindControl<Button>("AddButton")
        let editButton = this.FindControl<Button>("EditButton")
        let removeButton = this.FindControl<Button>("RemoveButton")
        let toggleButton = this.FindControl<Button>("ToggleButton")
        let refreshButton = this.FindControl<Button>("RefreshButton")
        let clearErrorButton = this.FindControl<Button>("ClearErrorButton")
        
        addButton.Click.Add(this.OnAddClick)
        editButton.Click.Add(this.OnEditClick)
        removeButton.Click.Add(this.OnRemoveClick)
        toggleButton.Click.Add(this.OnToggleClick)
        refreshButton.Click.Add(this.OnRefreshClick)
        clearErrorButton.Click.Add(this.OnClearErrorClick)
    
    member private this.GetViewModel() =
        match this.DataContext with
        | :? ManageHostsViewModel as vm -> Some vm
        | _ -> None
    
    member private this.OnAddClick(e: RoutedEventArgs) =
        task {
            let dialog = HostRecordDialog()
            dialog.SetRecord(HostRecordViewModel.CreateNew())
            
            match TopLevel.GetTopLevel(this) with
            | :? Window as window ->
                let! result = dialog.ShowDialog<bool>(window)
                let! result = dialog.ShowDialog<bool>(window)
            
                if result then
                    let newRecord = dialog.GetRecord()
                    match this.GetViewModel() with
                    | Some vm -> vm.AddRecord(newRecord) |> ignore
                    | None -> ()
            | _ -> ()
        } |> ignore
    
    member private this.OnEditClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm ->
            match vm.SelectedRecord with
            | Some selected ->
                task {
                    let dialog = HostRecordDialog()
                    // Create a copy for editing
                    let editCopy = HostRecordViewModel({
                        Id = selected.Id
                        IpAddress = selected.IpAddress
                        Hostname = selected.Hostname
                        Comment = if String.IsNullOrEmpty(selected.Comment) then None else Some selected.Comment
                        IsEnabled = selected.IsEnabled
                    })
                    dialog.SetRecord(editCopy)
                    
                    match TopLevel.GetTopLevel(this) with
                    | :? Window as window ->
                        let! result = dialog.ShowDialog<bool>(window)
                        
                        if result then
                            let editedRecord = dialog.GetRecord()
                            vm.UpdateRecord(editedRecord) |> ignore
                    | _ -> ()
                } |> ignore
            | None -> ()
        | None -> ()
    
    member private this.OnRemoveClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm ->
            match vm.SelectedRecord with
            | Some selected -> vm.RemoveRecord(selected) |> ignore
            | None -> ()
        | None -> ()
    
    member private this.OnToggleClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm ->
            match vm.SelectedRecord with
            | Some selected -> vm.ToggleRecord(selected) |> ignore
            | None -> ()
        | None -> ()
    
    member private this.OnRefreshClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.LoadRecords()
        | None -> ()
    
    member private this.OnClearErrorClick(e: RoutedEventArgs) =
        match this.GetViewModel() with
        | Some vm -> vm.ClearError()
        | None -> ()
