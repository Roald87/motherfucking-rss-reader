module SimpleRssServer.Request

open System.Net
open System.Text
open System
open System.Web
open System.Net.Http
open RssParser

let convertUrlToFilename (url: string) : string =
    let replaceInvalidFilenameChars = Text.RegularExpressions.Regex("[.?=:/]+")
    replaceInvalidFilenameChars.Replace(url, "_")

let getRssUrls (context: string) : string list option =
    context
    |> HttpUtility.ParseQueryString
    |> fun query ->
        let rssValues = query.GetValues("rss")

        if rssValues <> null && rssValues.Length > 0 then
            Some(rssValues |> List.ofArray)
        else
            None

// Fetch the contents of a web page
let getAsync (client: HttpClient) (url: string) =
    async {
        let! response = client.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

let fetchAllRssFeeds client (urls: string list) =
    urls |> List.map (getAsync client) |> Async.Parallel |> Async.RunSynchronously


let handleRequest client (context: HttpListenerContext) =
    async {
        let rssFeeds = getRssUrls context.Request.Url.Query

        let items =
            match rssFeeds with
            | Some urls -> fetchAllRssFeeds client urls
            | None -> [||]

        let itemHtml =
            items
            |> Seq.collect parseRss
            |> Seq.sortBy (fun a -> a.PostDate)
            |> Seq.map (fun article -> $"<li><a href='%s{article.PostDate.ToString()}'>%s{article.Title}</a></li>")
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
