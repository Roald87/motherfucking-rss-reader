open Microsoft.Extensions.Logging
open System.IO
open System.Net

open SimpleRssServer.Logging
open SimpleRssServer.Request

type Milisecond = Milisecond of int

let updateRssFeedsPeriodically client cacheDir (period: Milisecond) =
    async {
        while true do
            let urls =
                if File.Exists(requestLogPath) then
                    File.ReadAllLines(requestLogPath)
                    |> Array.map (fun line -> line.Split(' ').[1])
                    |> Array.toList
                else
                    []

            if urls.Length > 0 then
                logger.LogDebug("Fetching RSS feeds for URLs: {Urls}", String.concat ", " urls)
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

    Async.Start(updateRssFeedsPeriodically httpClient cacheDir (Milisecond(1000 * 60 * 60)))
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
