open System.Net
open System.IO
// open Microsoft.Extensions.Logging

open SimpleRssServer.Request
open SimpleRssServer.Logging


let startServer cacheDir (prefixes: string list) =
    let listener = new HttpListener()
    prefixes |> List.iter listener.Prefixes.Add
    listener.Start()
    let addresses = prefixes |> String.concat ", "
    logger.LogInformation($"Listening at {addresses}...")

    let httpClient = new Http.HttpClient()

    let rec loop () =
        async {
            let! context = listener.GetContextAsync() |> Async.AwaitTask
            do! handleRequest httpClient cacheDir context
            return! loop ()
        }

    loop ()

[<EntryPoint>]
let main argv =
    let cacheDir = "rss-cache"

    if not (Directory.Exists(cacheDir)) then
        Directory.CreateDirectory(cacheDir) |> ignore

    let prefixes =
        if argv.Length > 0 then
            argv |> Array.toList
        else
            [ "http://+:5000/" ]

    // let factory = LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore)
    // let logger = factory.CreateLogger("SimpleRssReader")

    startServer cacheDir prefixes |> Async.RunSynchronously
    0
