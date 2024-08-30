module SimpleRssServer.Tests.CodeHollowFeedReaderTests

open System

open CodeHollow.FeedReader
open Xunit

[<Fact>]
let ``test CodeHollow.FeedReader with roaldinch`` () =
    let feed = FeedReader.ReadFromFile("data/roaldinch.xml")

    Assert.Equal(10, feed.Items.Count)
    Assert.Equal("Groepsreserveringen", feed.Items[0].Title)
    Assert.Equal(DateTime(2024, 8, 6, 0, 0, 0) |> Nullable, feed.Items[0].PublishingDate)
    Assert.Equal("https://roaldin.ch/groepsreserveringen", feed.Items[0].Link)

    Assert.Equal(
        "Regelmatig zie ik hier treincoupés die zijn gereserveerd voor een groep. Vaak zijn dit schoolklassen op een uitje, maar soms ook andere groepen. Zo had ik laatst een wandeling met collega’s, waarvoor een gedeelte van de coupé was gereserveerd. In Nederland",
        feed.Items.[0].Description.Substring(0, 256)
    )

[<Fact>]
let ``test CodeHollow.FeedReader with zoesklot`` () =
    let feed = FeedReader.ReadFromFile("data/zoesklot.xml")

    Assert.Equal(5, feed.Items.Count)
    Assert.Equal("Duitse shag", feed.Items[0].Title)
    Assert.Equal("https://www.zoesklot.nl/duitse-shag/", feed.Items[0].Link)
    Assert.Equal(DateTime(2024, 8, 6, 13, 26, 32) |> Nullable, feed.Items[0].PublishingDate)

    Assert.Equal(
        "Bij de kassa van de Jumbo ziet M. mij en ik kan niet meer vluchten naar een andere kassa. M., een magere vijftiger met donker achterovergekamd golvend haar; enkele tattoos, oorbel en kunstgebitje ken ik van de volkstuin. Als we oogcontact hebben dan word ik meegezogen in zijn persoonlijke wereld. “Het was zo druk man"
            .Substring(0, 255),
        feed.Items[0].Description.Substring(0, 255)
    )

[<Fact>]
let ``test CodeHollow.FeedReader with spectrum`` () =
    // https://spectrum.ieee.org/feeds/feed.rss
    let feed = FeedReader.ReadFromFile("data/spectrum.rss")

    Assert.Equal(30, feed.Items.Count)
    Assert.Equal("Quantum Cryptography Has Everyone Scrambling", feed.Items[0].Title)

[<Fact>]
let ``test CodeHollow.FeedReader with quanta`` () =
    // https://www.quantamagazine.org/feed/
    let feed = FeedReader.ReadFromFile("data/quanta.xml")

    Assert.Equal(5, feed.Items.Count)
    Assert.Equal("Physicists Pinpoint the Quantum Origin of the Greenhouse Effect", feed.Items[0].Title)
