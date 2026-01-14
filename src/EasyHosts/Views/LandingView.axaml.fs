namespace EasyHosts.Views

open Avalonia.Controls
open Avalonia.Markup.Xaml

type LandingView() as this =
    inherit UserControl()
    
    do this.InitializeComponent()
    
    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
