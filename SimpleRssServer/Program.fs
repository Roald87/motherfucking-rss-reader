open Microsoft.Extensions.Logging
open System.IO
open System.Net

open SimpleRssServer.Logging
open SimpleRssServer.Request

type Milisecond = Milisecond of int

let updateRssFeedsPeriodically client cacheDir (period: Milisecond) =
    async {
        while true do
            let urls = requestUrls requestLogPath

            if urls.Length > 0 then
                logger.LogDebug($"Periodically updating {urls.Length} RSS feeds.")
                fetchAllRssFeeds client cacheDir urls |> ignore

            let (Milisecond t) = period
            do! Async.Sleep(t)
    }

let startServer cacheDir (prefixes: string list) =
    let listener = new HttpListener()
    prefixes |> List.iter listener.Prefixes.Add
    listener.Start()
    let addresses = prefixes |> String.concat ", "
    logger.LogInformation("Listening at {Addresses}", addresses)

    let httpClient = new Http.HttpClient()

    let rec loop () =
        async {
            let! context = listener.GetContextAsync() |> Async.AwaitTask
            do! handleRequest httpClient cacheDir context
            return! loop ()
        }

    Async.Start(updateRssFeedsPeriodically httpClient cacheDir (Milisecond(1000 * 10)))
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

    startServer cacheDir prefixes |> Async.RunSynchronously
    0
