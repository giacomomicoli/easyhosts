namespace EasyHosts.ViewModels

open System.ComponentModel
open System.Runtime.CompilerServices

/// Base class for all ViewModels providing INotifyPropertyChanged implementation
[<AbstractClass>]
type ViewModelBase() =
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()
    
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = propertyChanged.Publish
    
    /// Raises PropertyChanged event
    member this.OnPropertyChanged([<CallerMemberName>] ?propertyName: string) =
        let name = defaultArg propertyName ""
        propertyChanged.Trigger(this, PropertyChangedEventArgs(name))
    
    /// Sets a property value and raises PropertyChanged if the value changed
    member this.SetProperty<'T>(field: byref<'T>, value: 'T, [<CallerMemberName>] ?propertyName: string) =
        if not (obj.Equals(field, value)) then
            field <- value
            this.OnPropertyChanged(?propertyName = propertyName)
            true
        else
            false
