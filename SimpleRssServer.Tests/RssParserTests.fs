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
            Url = "https://roaldin.ch/groepsreserveringen"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 8, 5, 2, 0, 0)
            Title = "1 augustus brunch"
            Url = "https://roaldin.ch/1-augustus-brunch"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 7, 30, 2, 0, 0)
            Title = "Geschakeld stopcontact"
            Url = "https://roaldin.ch/geschakeld-stopcontact"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 7, 25, 2, 0, 0)
            Title = "Voetbalfans"
            Url = "https://roaldin.ch/voetbalfans"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 7, 20, 2, 0, 0)
            Title = "Canapé"
            Url = "https://roaldin.ch/canape"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 7, 14, 2, 0, 0)
            Title = "Guillemets"
            Url = "https://roaldin.ch/guillemets"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 7, 1, 2, 0, 0)
            Title = "Bevoegde kinderen"
            Url = "https://roaldin.ch/bevoegde-kinderen"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 6, 27, 2, 0, 0)
            Title = "Verkeerd om"
            Url = "https://roaldin.ch/verkeerd-om"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 6, 22, 2, 0, 0)
            Title = "Vrouwenemancipatie"
            Url = "https://roaldin.ch/vrouwenemancipatie"
            BaseUrl = "roaldin.ch" }
          { PostDate = DateTime(2024, 6, 16, 2, 0, 0)
            Title = "Promoveren"
            Url = "https://roaldin.ch/promoveren"
            BaseUrl = "roaldin.ch" } ]

    Assert.Equal<Article list>(expected, result)
