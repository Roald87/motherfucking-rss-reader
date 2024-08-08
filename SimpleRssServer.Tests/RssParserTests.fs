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
            BaseUrl = "roaldin.ch"
            Text =
              "Regelmatig zie ik hier treincoupés die zijn gereserveerd voor een groep. Vaak zijn dit schoolklassen op een uitje, maar soms ook andere groepen. Zo had ik laatst een wandeling met collega’s, waarvoor een gedeelte van de coupé was gereserveerd. In Nederland..." }
          { PostDate = DateTime(2024, 8, 5, 2, 0, 0)
            Title = "1 augustus brunch"
            Url = "https://roaldin.ch/1-augustus-brunch"
            BaseUrl = "roaldin.ch"
            Text =
              "Normaal doen wij niets speciaals met 1 augustus1, maar deze keer kwam vriend T. met een idee: een 1 augustus brunch. I. en ik, hier al bijna negen en zeven jaar woonachtig, kenden dit concept nog niet. Op 1 augustus organiseren sommige boerderijen een brun..." }
          { PostDate = DateTime(2024, 7, 30, 2, 0, 0)
            Title = "Geschakeld stopcontact"
            Url = "https://roaldin.ch/geschakeld-stopcontact"
            BaseUrl = "roaldin.ch"
            Text =
              "Net als in Nederland hebben huizen hier dezelfde stopcontacten en lichtschakelaars. Stopcontacten hebben vaak ruimte voor drie stekkers, dankzij de kleinere vormfactor van geaarde stekkers1. Eén van de drie stekkers is soms geschakeld. Dit zie je aan het s..." }
          { PostDate = DateTime(2024, 7, 25, 2, 0, 0)
            Title = "Voetbalfans"
            Url = "https://roaldin.ch/voetbalfans"
            BaseUrl = "roaldin.ch"
            Text =
              "Toen ik in 2018 het WK voetbal vanuit Zwitserland meemaakte, was ik teleurgesteld. Het voetbal leek hier niet echt te leven. Afgezien van een enkeling met een voetbalshirt en winkels die iets met het WK deden, merkte je weinig van het toernooi. Het dieptep..." }
          { PostDate = DateTime(2024, 7, 20, 2, 0, 0)
            Title = "Canapé"
            Url = "https://roaldin.ch/canape"
            BaseUrl = "roaldin.ch"
            Text =
              "Canapé1 zijn kleine hartige hapjes die tijdens een apéro2 worden geserveerd. Deze hapjes bestaan uit een basis van vers of getoast brood met daarop hartig beleg. Als je “canapé” googelt, zie je dat het geen specifiek Zwitsers fenomeen is. Wat echter wel ty..." }
          { PostDate = DateTime(2024, 7, 14, 2, 0, 0)
            Title = "Guillemets"
            Url = "https://roaldin.ch/guillemets"
            BaseUrl = "roaldin.ch"
            Text =
              "Laatst was ik bij een tentoonstelling waar de guillemets andersom werden gebruikt dan ik gewend was. Guillemets zijn deze tekens: « ». Ze worden in sommige landen, waaronder Zwitserland, als aanhalingstekens gebruikt. In mijn herinnering werd geopend met d..." }
          { PostDate = DateTime(2024, 7, 1, 2, 0, 0)
            Title = "Bevoegde kinderen"
            Url = "https://roaldin.ch/bevoegde-kinderen"
            BaseUrl = "roaldin.ch"
            Text =
              "Dit bord kwam ik tegen in onze buurt: “Betreten des Gerüstes für Kinder und Unbefugte verboten” (Betreden van de steiger verboden voor kinderen en onbevoegden). De toevoeging van de kinderen vond ik opvallend. Waarom vallen zij niet onder de onbevoegden? Z..." }
          { PostDate = DateTime(2024, 6, 27, 2, 0, 0)
            Title = "Verkeerd om"
            Url = "https://roaldin.ch/verkeerd-om"
            BaseUrl = "roaldin.ch"
            Text =
              "De eerste keer dat ik een kaartspel met Zwitsers speelde, ging er iets fout. Ik ging ervan uit dat we met de klok mee zouden spelen, maar dat bleek niet zo te zijn. De standaard richting hier is tegen de klok in. Het wordt ook wel “dr ohrfiige noch” genoemd. Dit betekent in de richting van een (rechtshandige) oorveeg. Op het werk was men verdeeld over de juiste volgorde. J. antwoordde dat het afhing of hij in Duitsland of Zwitserland speelde, S. zei dat hij in Oostenrijk de goede richting op speelde, met de klok mee, en A. uit Spanje dacht even na en kwam uit op tegen de klok in. In de rest van Europa is men ook niet eensgezind. Het hangt af van de regio, het land of het spel1. https://www.reddit.com/r/AskEurope/s/mIHUVNFwGH &#8617;"
                  .Substring(0, 256)
              + "..." }
          { PostDate = DateTime(2024, 6, 22, 2, 0, 0)
            Title = "Vrouwenemancipatie"
            Url = "https://roaldin.ch/vrouwenemancipatie"
            BaseUrl = "roaldin.ch"
            Text =
              "Hoe hier over vrouwen wordt gedacht lijkt soms een generatie achter te lopen vergeleken met Nederland. Een vriendin vertelde dat een vriendin van haar, A., naar Zwitserland was verhuisd. A. was werkloos en moest iets bij de gemeente aanvragen. Toen ze vertelde dat ze geen werk had, zei de ambtenaar niet ironisch: “Dat snap ik wel, u leeft lekker op het salaris van uw man.” Toegegeven, ze woonden ergens in een klein dorp. In een grote stad zal dit niet snel gebeuren. Ondanks de relatief hoge arbeidsparticipatie van vrouwen1, werden ze lang vooral gezien als huisvrouwen. Pas in 1971 kregen ze landelijk stemrecht en het duurde tot 1990 (!) tot ze ook in alle kantons lokaal stemrecht hadden2. Een klein voordeel hadden de vrouwen tot begin dit jaar wel: ze mochten met 64 met pensioen tegenover 65 voor mannen. Tot slot hebben ze geen dienstplicht. Al zijn er wel regelmatig discussies of dit moet veranderen. https://ourworldindata.org/female-labor-supply &#8617; https://nl.m.wikipedia.org/wiki/Vrouwenstemrecht_in_Zwitserland &#8617;"
                  .Substring(0, 256)
              + "..." }
          { PostDate = DateTime(2024, 6, 16, 2, 0, 0)
            Title = "Promoveren"
            Url = "https://roaldin.ch/promoveren"
            BaseUrl = "roaldin.ch"
            Text =
              "Recent had ik het genoegen om mijn eerste Zwitserse verdediging bij te wonen van een promovendus. Het viel me tegen hoe dit gevierd werd. Zelf ben ik in Groningen gepromoveerd en daar waren verschillende tradities en gewoonten. Zo kiest de promovendus twee paranimfen, een soort getuigen zoals bij huwelijken, die de promovendus helpen met allerlei zaken rondom de verdediging."
                  .Substring(0, 256)
              + "..." } ]

    Assert.Equal(expected.Length, result.Length)
    List.iter2 (fun (exp : Article) (act : Article) -> Assert.Equal<Article>(exp, act)) expected result
