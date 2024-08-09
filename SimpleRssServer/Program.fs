﻿open System.Net
open System.Text
open FSharp.Data
open SimpleRssServer.Request

type Rss = XmlProvider<"https://roaldin.ch/feed.xml">

let fetchRssFeed (url: string) =
    async {
        let! rss = Rss.AsyncLoad(url)
        return rss.Entries |> Seq.map (fun item -> item.Title, item.Link)
    }

let fetchAllRssFeeds (urls: string list) =
    urls
    |> List.map fetchRssFeed
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Seq.concat

let handleRequest (context: HttpListenerContext) =
    async {
        let rssFeeds = getRssUrls context.Request.Url.Query

        let items =
            match rssFeeds with
            | Some urls -> fetchAllRssFeeds urls
            | None -> Seq.empty

        let itemHtml =
            items
            |> Seq.map (fun (title, link) -> sprintf "<li><a href='%s'>%s</a></li>" link.Href title.Value)
            |> String.concat ""

        let responseString = sprintf "<h1>RSS Feed</h1><ul>%s</ul>" itemHtml

        let buffer = Encoding.UTF8.GetBytes(responseString)
        context.Response.ContentLength64 <- int64 buffer.Length
        context.Response.ContentType <- "text/html"

        do!
            context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length)
            |> Async.AwaitTask

        context.Response.OutputStream.Close()
    }

let startServer (prefixes: string list) =
    let listener = new HttpListener()
    prefixes |> List.iter listener.Prefixes.Add
    listener.Start()
    printfn "Listening..."

    let rec loop () =
        async {
            let! context = listener.GetContextAsync() |> Async.AwaitTask
            do! handleRequest context
            return! loop ()
        }

    loop ()

[<EntryPoint>]
let main argv =
    let cacheDir = "rss-cache"
    if not (Directory.Exists(cacheDir)) then
        Directory.CreateDirectory(cacheDir) |> ignore

    startServer ["http://localhost:5000/"] |> Async.RunSynchronously
    0
