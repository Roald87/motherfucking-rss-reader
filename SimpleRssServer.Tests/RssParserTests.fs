module SimpleRssServer.Tests.RssParser

open Xunit
open SimpleRssServer.RssParser
open System

[<Fact>]
let ``Test parseRss with roaldinch.xml`` () =
    let result = parseRss "data/roaldinch.xml"
    let expected = [
        { PostDate = DateTime(2024, 8, 6, 2, 0, 0); Title = "Groepsreserveringen" }
        { PostDate = DateTime(2024, 8, 5, 2, 0, 0); Title = "1 augustus brunch" }
        { PostDate = DateTime(2024, 7, 30, 2, 0, 0); Title = "Geschakeld stopcontact" }
        { PostDate = DateTime(2024, 7, 25, 2, 0, 0); Title = "Voetbalfans" }
        { PostDate = DateTime(2024, 7, 20, 2, 0, 0); Title = "Canapé" }
        { PostDate = DateTime(2024, 7, 14, 2, 0, 0); Title = "Guillemets" }
        { PostDate = DateTime(2024, 7, 1, 2, 0, 0); Title = "Bevoegde kinderen" }
        { PostDate = DateTime(2024, 6, 27, 2, 0, 0); Title = "Verkeerd om" }
        { PostDate = DateTime(2024, 6, 22, 2, 0, 0); Title = "Vrouwenemancipatie" }
        { PostDate = DateTime(2024, 6, 16, 2, 0, 0); Title = "Promoveren" }
    ]
    Assert.Equal<Article list>(expected, result)
