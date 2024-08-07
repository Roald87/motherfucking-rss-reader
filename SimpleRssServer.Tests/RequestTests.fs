module SimpleRssServer.Tests.RequestTests

open Xunit
open SimpleRssServer.Request

[<Fact>]
let ``Test getRequestInfo`` () =
    let result = getRssUrls "?rss=https://abs.com/test"

    Assert.Equal(Some ["https://abs.com/test"], result)
