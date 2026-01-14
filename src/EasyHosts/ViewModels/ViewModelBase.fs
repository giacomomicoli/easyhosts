namespace EasyHosts.ViewModels

open System.ComponentModel
open System.Runtime.CompilerServices

/// Base class for all ViewModels providing INotifyPropertyChanged implementation
[<AbstractClass>]
type ViewModelBase() =
    let mutable propertyChangedHandler: PropertyChangedEventHandler = null
    
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = 
            { new IDelegateEvent<PropertyChangedEventHandler> with
                member _.AddHandler(handler) = 
                    propertyChangedHandler <- System.Delegate.Combine(propertyChangedHandler, handler) :?> PropertyChangedEventHandler
                member _.RemoveHandler(handler) = 
                    propertyChangedHandler <- System.Delegate.Remove(propertyChangedHandler, handler) :?> PropertyChangedEventHandler
            }
    
    /// Raises PropertyChanged event
    member this.OnPropertyChanged([<CallerMemberName>] ?propertyName: string) =
        let name = defaultArg propertyName ""
        match propertyChangedHandler with
        | null -> ()
        | handler -> handler.Invoke(this, PropertyChangedEventArgs(name))
    
    /// Sets a property value and raises PropertyChanged if the value changed
    member this.SetProperty<'T>(field: byref<'T>, value: 'T, [<CallerMemberName>] ?propertyName: string) =
        if not (obj.Equals(field, value)) then
            field <- value
            this.OnPropertyChanged(?propertyName = propertyName)
            true
        else
            false
