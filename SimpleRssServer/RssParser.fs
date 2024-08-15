module SimpleRssServer.RssParser

open System
open System.IO

open CodeHollow.FeedReader

type Article =
    { PostDate: DateTime option
      Title: string
      Url: string
      BaseUrl: string
      Text: string }

let stripHtml (input: string) : string =
    let regex = Text.RegularExpressions.Regex("<.*?>")
    let noHtml = regex.Replace(input, "")
    let removeMutliSpaces = Text.RegularExpressions.Regex("\s+")

    noHtml.Replace("\n", " ").Replace("\r", "").Trim()
    |> fun s -> removeMutliSpaces.Replace(s, " ")

let ARTICLE_DESCRIPTION_LENGTH = 255

let parseRss (feedContent: string) : Article list =
    let feed = FeedReader.ReadFromString(feedContent)

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


let parseRssFromFile (fileName) = parseRss (File.ReadAllText fileName)
