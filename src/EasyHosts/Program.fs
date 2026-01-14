namespace EasyHosts

open Avalonia
open System

module Program =
    
    [<CompiledName "BuildAvaloniaApp">]
    let buildAvaloniaApp () =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
    
    [<EntryPoint; STAThread>]
    let main argv =
        buildAvaloniaApp()
            .StartWithClassicDesktopLifetime(argv)
