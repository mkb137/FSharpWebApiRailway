namespace RailwayWebApi

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Serilog

module Program =
    let exitCode = 0




    [<EntryPoint>]
    let main args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
            webBuilder.UseStartup<Startup>()
                .UseSerilog(fun ctx cfg -> cfg.ReadFrom.Configuration(ctx.Configuration) |> ignore) |> ignore).Build()
            .Run()

        exitCode
