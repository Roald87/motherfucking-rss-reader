module SimpleRssServer.Request

open System.Net
open System.Text
open System
open System.Web
open System.Net.Http
open RssParser
open System.IO

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

let fetchWithCache client (cacheLocation: string) (url: string) =
    async {
        let cacheFilename = convertUrlToFilename url
        let cachePath = Path.Combine(cacheLocation, cacheFilename)

        if not <| File.Exists(cachePath) then
            printfn $"Did not find cached file %s{cachePath}. Fetching %s{url}"
            let! page = getAsync client url
            File.WriteAllText(cachePath, page)
            return page
        else
            printfn $"Found cached file %s{cachePath}"
            return File.ReadAllText(cachePath)
    }

let fetchAllRssFeeds client (cacheLocation: string) (urls: string list) =
    urls
    |> List.map (fetchWithCache client cacheLocation)
    |> Async.Parallel
    |> Async.RunSynchronously


let rssHtmlItem article =
    let date =
        if article.PostDate.IsSome then
            $"@ %s{article.PostDate.Value.ToLongDateString()}"
        else
            ""

    $"""
    <div class="feed-item">
        <h2><a href="%s{article.Url}" target="_blank">%s{article.Title}</a></h2>
            <div class="source-date">%s{article.BaseUrl} %s{date}</div>
            <p>%s{article.Text}</p>
        </div>
    """

let header =
    """
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8", name="viewport" content="width=device-width, initial-scale=1">
        <title>Motherfucking RSS Reader</title>
        <link rel="stylesheet" type="text/css" href="/styles.css">
    </head>
    """

let landingPage =
    header
    + """
    <body>
        <div class="header">
            <h1>Motherfucking RSS Reader</h1>
            <a id="config-link" href="config.html">Configure</a>
        </div>
        <p><Please provide one or more RSS feed URLs as query parameters, e.g., ?rss=https://example.com/rss&rss=https://another.com/rss/</p>
    </body>
    </hmtl>
    """

let homepage rssItems =
    let body =
        """
    <body>
        <div class="header">
            <h1>Motherfucking RSS Reader</h1>
            <a id="config-link" href="config.html">Configure</a>
        </div>
    """

    let rssFeeds =
        rssItems
        |> Seq.collect parseRss
        |> Seq.sortByDescending (fun a -> a.PostDate)
        |> Seq.map rssHtmlItem
        |> String.concat ""

    let footer =
        """
    </body>
    </html>
    """

    header + body + rssFeeds + footer

// https://stackoverflow.com/a/3722671/6329629
let (|Prefix|_|) (p: string) (s: string) =
    if s.StartsWith(p) then
        Some(s.Substring(p.Length))
    else
        None

let assembleRssFeeds client cacheLocation query =
    let rssFeeds = getRssUrls query

    let items =
        match rssFeeds with
        | Some urls -> fetchAllRssFeeds client cacheLocation urls
        | None -> [||]

    homepage items

let handleRequest client (cacheLocation: string) (context: HttpListenerContext) =
    async {
        printfn $"Received request %A{context.Request.Url}"

        context.Response.ContentType <- "text/html"

        let responseString =
            match context.Request.RawUrl with
            | "/styles.css" as x ->
                context.Response.ContentType <- "text/css"
                File.ReadAllText("./" + x.Substring(1, x.Length - 1))
            | "/config.html" as x -> File.ReadAllText(x.Substring(1, x.Length - 1))
            | Prefix "/?rss=" _ -> assembleRssFeeds client cacheLocation context.Request.Url.Query
            | _ -> landingPage

        let buffer = Encoding.UTF8.GetBytes(responseString)
        context.Response.ContentLength64 <- int64 buffer.Length


        do!
            context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length)
            |> Async.AwaitTask

        context.Response.OutputStream.Close()
    }
