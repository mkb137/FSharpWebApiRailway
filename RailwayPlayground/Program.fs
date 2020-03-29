open Microsoft.Extensions.Configuration
open Serilog
open BlogDemos

let configureLogging() =
    let configuration =
        ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build()
    Log.Logger <- LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger()

[<EntryPoint>]
let main argv =
    printfn "main"
    try
        configureLogging()
        run()
    with e -> printfn "%A" e
    0 // return an integer exit code
