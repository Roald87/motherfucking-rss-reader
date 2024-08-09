open System.Net
open System.IO

open SimpleRssServer.Request

let startServer (prefixes: string list) =
    let listener = new HttpListener()
    prefixes |> List.iter listener.Prefixes.Add
    listener.Start()
    printfn "Listening..."

    let httpClient = new Http.HttpClient()

    let rec loop () =
        async {
            let! context = listener.GetContextAsync() |> Async.AwaitTask
            do! handleRequest httpClient context
            return! loop ()
        }

    loop ()

[<EntryPoint>]
let main argv =
    let cacheDir = "rss-cache"

    if not (Directory.Exists(cacheDir)) then
        Directory.CreateDirectory(cacheDir) |> ignore

    startServer [ "http://localhost:5000/" ] |> Async.RunSynchronously
    0
