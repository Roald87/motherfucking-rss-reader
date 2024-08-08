module SimpleRssServer.Tests.RssParser

open Xunit
open SimpleRssServer.RssParser
open System

[<Fact>]
let ``Test parseRss with roaldinch.xml`` () =
    let result = parseRss "data/roaldinch.xml"

    let expected =
        [ { PostDate = DateTime(2024, 8, 6, 2, 0, 0)
            Title = "Groepsreserveringen"
            Url = "https://roaldin.ch/groepsreserveringen" }
          { PostDate = DateTime(2024, 8, 5, 2, 0, 0)
            Title = "1 augustus brunch"
            Url = "https://roaldin.ch/1-augustus-brunch" }
          { PostDate = DateTime(2024, 7, 30, 2, 0, 0)
            Title = "Geschakeld stopcontact"
            Url = "https://roaldin.ch/geschakeld-stopcontact" }
          { PostDate = DateTime(2024, 7, 25, 2, 0, 0)
            Title = "Voetbalfans"
            Url = "https://roaldin.ch/voetbalfans" }
          { PostDate = DateTime(2024, 7, 20, 2, 0, 0)
            Title = "Canapé"
            Url = "https://roaldin.ch/canape" }
          { PostDate = DateTime(2024, 7, 14, 2, 0, 0)
            Title = "Guillemets"
            Url = "https://roaldin.ch/guillemets" }
          { PostDate = DateTime(2024, 7, 1, 2, 0, 0)
            Title = "Bevoegde kinderen"
            Url = "https://roaldin.ch/bevoegde-kinderen" }
          { PostDate = DateTime(2024, 6, 27, 2, 0, 0)
            Title = "Verkeerd om"
            Url = "https://roaldin.ch/verkeerd-om" }
          { PostDate = DateTime(2024, 6, 22, 2, 0, 0)
            Title = "Vrouwenemancipatie"
            Url = "https://roaldin.ch/vrouwenemancipatie" }
          { PostDate = DateTime(2024, 6, 16, 2, 0, 0)
            Title = "Promoveren"
            Url = "https://roaldin.ch/promoveren" } ]

    Assert.Equal<Article list>(expected, result)
