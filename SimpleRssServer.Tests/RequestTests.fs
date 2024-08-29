module SimpleRssServer.Tests.RequestTests

open Xunit
open SimpleRssServer.Helper
open SimpleRssServer.Request
open System
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open System.Net

[<Fact>]
let ``Test getRequestInfo`` () =
    let result = getRssUrls "?rss=https://abs.com/test"

    Assert.Equal(Some [ "https://abs.com/test" ], result)

[<Fact>]
let ``Test getRequestInfo with two URLs`` () =
    let result = getRssUrls "?rss=https://abs.com/test1&rss=https://abs.com/test2"

    Assert.Equal(Some [ "https://abs.com/test1"; "https://abs.com/test2" ], result)

[<Fact>]
let ``Test getRequestInfo with empty string`` () =
    let result = getRssUrls ""

    Assert.Equal(None, result)

[<Fact>]
let ``Test convertUrlToFilename`` () =
    Assert.Equal("https_abc_com_test", convertUrlToValidFilename "https://abc.com/test")
    Assert.Equal("https_abc_com_test_rss_blabla", convertUrlToValidFilename "https://abc.com/test?rss=blabla")

type MockHttpMessageHandler(response: HttpResponseMessage) =
    inherit HttpMessageHandler()

    override _.SendAsync(request: HttpRequestMessage, cancellationToken: CancellationToken) = Task.FromResult(response)

[<Fact>]
let ``Test getAsync with successful response`` () =
    let expectedContent = "Hello, world!"
    let responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    responseMessage.Content <- new StringContent(expectedContent)

    let handler = new MockHttpMessageHandler(responseMessage)
    let client = new HttpClient(handler)

    let result =
        getAsync client "http://example.com" (Some DateTime.Now)
        |> Async.RunSynchronously

    match result with
    | Success result -> Assert.Equal(expectedContent, result)
    | Failure error -> Assert.True(false, error)

[<Fact>]
let ``Test getAsync with unsuccessful response on real page`` () =
    let client = new HttpClient()

    let response =
        getAsync client "https://thisurldoesntexistforsureordoesit.com" (Some DateTime.Now)
        |> Async.RunSynchronously

    match response with
    | Success _ -> Assert.False(true, "Expected Failure but got Success")
    | Failure errorMsg -> Assert.Contains("Exception", errorMsg)
