module SimpleRssServer.RssParser

open System
open System.Xml.Linq

type Article =
    { PostDate: DateTime
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

let parseRss (fileName: string) : Article list =
    let doc = XDocument.Load(fileName)
    let ns = XNamespace.Get("http://www.w3.org/2005/Atom")
    let entries = 
        let atomEntries = doc.Descendants(ns + "entry")
        if Seq.isEmpty(atomEntries) then
            doc.Descendants(XName.Get("item"))
        else
            atomEntries

    entries
    |> Seq.map (fun entry ->
        let published = entry.Element(ns + "published").Value
        let postDate = DateTime.Parse(published)
        let title = entry.Element(ns + "title").Value
        let link = entry.Element(ns + "link").Attribute(XName.Get("href")).Value

        let baseUrl =
            let uri = Uri(link)
            uri.Host

        let text =
            let content = stripHtml (entry.Element(ns + "content").Value)

            if content.Length > 256 then
                content.Substring(0, 256) + "..."
            else
                content

        { PostDate = postDate
          Title = title
          Url = link
          BaseUrl = baseUrl
          Text = text })
    |> Seq.toList
