namespace EasyHosts.Views

open Avalonia.Controls
open Avalonia.Markup.Xaml
open EasyHosts.ViewModels

type MainWindow() as this =
    inherit Window()
    
    do this.InitializeComponent()
    
    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        
        // Set up the ViewModel
        let viewModel = MainWindowViewModel()
        this.DataContext <- viewModel
        viewModel.Initialize()
