module SimpleRssServer.Tests.RequestTests

open Xunit
open SimpleRssServer.Request

[<Fact>]
let ``Test getRequestInfo`` () =
    let result = getRequestInfo()
    Assert.Equal("Request Info", result)
