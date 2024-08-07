module SimpleRssServer.Tests.RequestTests

open Xunit
open SimpleRssServer.Request

[<Fact>]
let ``Test getRequestInfo`` () =
    let result = getRssUrls "?rss=https://abs.com/test"

    Assert.Equal(Some ["https://abs.com/test"], result)

[<Fact>]
let ``Test getRequestInfo with two URLs`` () =
    let result = getRssUrls "?rss=https://abs.com/test1&rss=https://abs.com/test2"

    Assert.Equal(Some ["https://abs.com/test1"; "https://abs.com/test2"], result)
