module SimpleRssServer.Request

open System.Net
open System.Text
open System
open System.Web
open System.Net.Http
open RssParser
open System.IO
open Microsoft.Extensions.Logging

open SimpleRssServer.Logging
open SimpleRssServer.Helper

let convertUrlToValidFilename (url: string) : string =
    let replaceInvalidFilenameChars = RegularExpressions.Regex("[.?=:/]+")
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
        try
            let! response = client.GetAsync(url) |> Async.AwaitTask

            if response.IsSuccessStatusCode then
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return Success content
            else
                return Failure $"Failed to get {url}. Error: {response.StatusCode}."
        with ex ->
            return Failure $"Failed to get {url}. {ex.GetType().Name}: {ex.Message}"
    }

let fetchWithCache client (cacheLocation: string) (url: string) =
    async {
        let cacheFilename = convertUrlToValidFilename url
        let cachePath = Path.Combine(cacheLocation, cacheFilename)

        let fileExists = File.Exists(cachePath)

        let fileIsOld =
            if fileExists then
                let lastWriteTime = File.GetLastWriteTime(cachePath)
                (DateTime.Now - lastWriteTime).TotalHours > 1.0
            else
                false

        if not fileExists || fileIsOld then
            if fileIsOld then
                logger.LogInformation($"Cached file {cachePath} is older than 1 hour. Fetching {url}")
            else
                logger.LogInformation($"Did not find cached file {cachePath}. Fetching {url}")

            let! page = getAsync client url

            match page with
            | Success content -> File.WriteAllTextAsync(cachePath, content) |> ignore
            | Failure message -> ()

            return page
        else
            logger.LogInformation($"Found cached file {cachePath} and it is up to date")
            let! content = File.ReadAllTextAsync(cachePath) |> Async.AwaitTask
            return Success content
    }

let fetchAllRssFeeds client (cacheLocation: string) (urls: string list) =
    urls
    |> List.map (fetchWithCache client cacheLocation)
    |> Async.Parallel
    |> Async.RunSynchronously


let convertArticleToHtml article =
    let date =
        if article.PostDate.IsSome then
            $"on %s{article.PostDate.Value.ToLongDateString()}"
        else
            ""

    $"""
    <div class="feed-item">
        <h2><a href="%s{article.Url}" target="_blank">%s{article.Title}</a></h2>
            <div class="source-date">%s{article.BaseUrl} %s{date}</div>
            <p>%s{article.Text}</p>
        </div>
    """

let header = File.ReadAllText(Path.Combine("site", "header.html"))

let landingPage =
    header + File.ReadAllText(Path.Combine("site", "landing-page.html"))

let footer =
    """
    </body>
    </html>
    """

let homepage query rssItems =
    let body =
        $"""
    <body>
        <div class="header">
            <h1>Motherfucking RSS Reader</h1>
            <a id="config-link" href="config.html/%s{query}">Configure</a>
        </div>
    """

    let rssFeeds =
        rssItems
        |> Seq.collect parseRss
        |> Seq.sortByDescending (fun a -> a.PostDate)
        |> Seq.map convertArticleToHtml
        |> String.concat ""

    header + body + rssFeeds + footer

let configPage query =
    let body =
        """
    <body>
        <div class="header">
            <h1>Configure RSS Reader</h1>
        </div>
        <form id="rss-config-form" method="GET" action="/">
    """

    let rssFeeds = getRssUrls query

    let urlFields =
        match rssFeeds with
        | Some urls when query.Contains("addField=true") ->
            let newField = "<input type='text' name='rss'><br>"

            (urls
             |> List.map (fun url -> $"<input type='text' name='rss' value='%s{url}'><br>")
             |> String.concat "\n")
            + newField
        | Some urls ->
            urls
            |> List.map (fun url -> $"<input type='text' name='rss' value='%s{url}'><br>")
            |> String.concat "\n"
        | None -> ""

    let emptyField =
        $"<input type='text' name='rss'><br><input type='submit' value='Submit'>
        <button type='submit' formaction='/config.html' name=''>Add another field</button></form>"

    header + body + urlFields + emptyField + footer

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
        | Some urls ->
            let filteredUrls =
                urls |> List.filter (fun url -> not (String.IsNullOrWhiteSpace(url)))

            fetchAllRssFeeds client cacheLocation filteredUrls
        | None -> [||]

    homepage query items

let handleRequest client (cacheLocation: string) (context: HttpListenerContext) =
    async {
        logger.LogInformation($"Received request {context.Request.Url}")

        let responseString =
            match context.Request.RawUrl with
            | Prefix "/config.html" _ -> configPage context.Request.Url.Query
            | Prefix "/?rss=" _ -> assembleRssFeeds client cacheLocation context.Request.Url.Query
            | "/robots.txt" -> File.ReadAllText(Path.Combine("site", "robots.txt"))
            | "/sitemap.xml" -> File.ReadAllText(Path.Combine("site", "sitemap.xml"))
            | _ -> landingPage

        let buffer = Encoding.UTF8.GetBytes(responseString)
        context.Response.ContentLength64 <- int64 buffer.Length
        context.Response.ContentType <- "text/html"

        do!
            context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length)
            |> Async.AwaitTask

        context.Response.OutputStream.Close()
    }
