module SimpleRssServer.RssParser

open System
open System.IO

open CodeHollow.FeedReader
open SimpleRssServer.Helper

type Article =
    { PostDate: DateTime option
      Title: string
      Url: string
      BaseUrl: string
      Text: string }

let stripHtml (input: string) : string =
    if String.IsNullOrWhiteSpace(input) then
        ""
    else
        let regex = Text.RegularExpressions.Regex("<.*?>")
        let noHtml = regex.Replace(input, "")
        let removeMutliSpaces = Text.RegularExpressions.Regex("\s+")

        noHtml.Replace("\n", " ").Replace("\r", "").Trim()
        |> fun s -> removeMutliSpaces.Replace(s, " ")

let ARTICLE_DESCRIPTION_LENGTH = 255

let createErrorFeed errorMessage =
    // Create a new FeedItem with the current date and the title "Error"
    let feedItem = FeedItem()
    feedItem.Title <- "Error"
    feedItem.PublishingDate <- Nullable(DateTime.Now)
    feedItem.Description <- errorMessage

    // Create a new Feed and add the FeedItem to it
    let customFeed = Feed()
    customFeed.Items.Add(feedItem)

    // Return the custom feed
    customFeed

let parseRss (feedContent: Result<string, string>) : Article list =
    let feed =
        match feedContent with
        | Success content -> FeedReader.ReadFromString(content)
        | Failure error -> createErrorFeed error

    feed.Items
    |> Seq.map (fun entry ->
        let postDate =
            if entry.PublishingDate.HasValue then
                Some(entry.PublishingDate.Value)
            else
                None

        let title = entry.Title
        let link = entry.Link

        let baseUrl =
            let uri = Uri(link)
            uri.Host.Replace("www.", "")

        let text =
            let content = stripHtml entry.Description

            if content.Length > ARTICLE_DESCRIPTION_LENGTH then
                content.Substring(0, ARTICLE_DESCRIPTION_LENGTH) + "..."
            else
                content

        { PostDate = postDate
          Title = title
          Url = link
          BaseUrl = baseUrl
          Text = text })
    |> Seq.toList


let parseRssFromFile fileName =
    try
        let content = File.ReadAllText(fileName) |> Success
        parseRss content
    with ex ->
        [ { PostDate = Some DateTime.Now
            Title = "Error"
            Url = fileName
            BaseUrl = fileName
            Text = $"{ex.GetType().Name} {ex.Message}" } ]
