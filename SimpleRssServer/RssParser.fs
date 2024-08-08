module SimpleRssServer.RssParser

open System
open System.Xml.Linq

type Article =
    { PostDate: DateTime
      Title: string
      Url: string }

let parseRss (fileName: string) : Article list =
    let doc = XDocument.Load(fileName)
    let ns = XNamespace.Get("http://www.w3.org/2005/Atom")
    let entries = doc.Descendants(ns + "entry")

    entries
    |> Seq.map (fun entry ->
        let published = entry.Element(ns + "published").Value
        let postDate = DateTime.Parse(published)
        let title = entry.Element(ns + "title").Value
        let link = entry.Element(ns + "link").Attribute(XName.Get("href")).Value

        { PostDate = postDate
          Title = title
          Url = link })
    |> Seq.toList
