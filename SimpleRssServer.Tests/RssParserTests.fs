module SimpleRssServer.Tests.RssParser

open Xunit
open SimpleRssServer.RssParser
open System

[<Fact>]
let ``Test parseRss with roaldinch.xml`` () =
    let result = parseRss "data/roaldinch.xml"
    let expected = [
        { PostDate = DateTime(2024, 8, 6, 2, 0, 0) }
        { PostDate = DateTime(2024, 8, 5, 2, 0, 0) }
        { PostDate = DateTime(2024, 7, 30, 2, 0, 0) }
        { PostDate = DateTime(2024, 7, 25, 2, 0, 0) }
        { PostDate = DateTime(2024, 7, 20, 2, 0, 0) }
        { PostDate = DateTime(2024, 7, 14, 2, 0, 0) }
        { PostDate = DateTime(2024, 7, 1, 2, 0, 0) }
        { PostDate = DateTime(2024, 6, 27, 2, 0, 0) }
        { PostDate = DateTime(2024, 6, 22, 2, 0, 0) }
        { PostDate = DateTime(2024, 6, 16, 2, 0, 0) }
    ]
    Assert.Equal<Article list>(expected, result)
