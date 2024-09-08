module SimpleRssServer.Tests.RequestTests

open System
open System.IO
open System.Net
open System.Net.Http
open System.Threading.Tasks

open Xunit

open SimpleRssServer.Helper
open SimpleRssServer.Request
open SimpleRssServer.RssParser

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

type MockHttpResponseHandler(response: HttpResponseMessage) =
    inherit HttpMessageHandler()
    override _.SendAsync(request, cancellationToken) = Task.FromResult(response)

[<Fact>]
let ``Test getAsync with successful response`` () =
    let expectedContent = "Hello, world!"
    let responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    responseMessage.Content <- new StringContent(expectedContent)

    let handler = new MockHttpResponseHandler(responseMessage)
    let client = new HttpClient(handler)

    let result =
        getAsync client "http://example.com" (Some DateTimeOffset.Now)
        |> Async.RunSynchronously

    match result with
    | Success result -> Assert.Equal(expectedContent, result)
    | Failure error -> Assert.True(false, error)

[<Fact>]
let ``Test getAsync with unsuccessful response on real page`` () =
    let client = new HttpClient()

    let response =
        getAsync client "https://thisurldoesntexistforsureordoesit.com" (Some DateTimeOffset.Now)
        |> Async.RunSynchronously

    match response with
    | Success _ -> Assert.False(true, "Expected Failure but got Success")
    | Failure errorMsg -> Assert.Contains("Exception", errorMsg)

type MockHttpMessageHandler(sendAsyncImpl: HttpRequestMessage -> Task<HttpResponseMessage>) =
    inherit HttpMessageHandler()
    override _.SendAsync(request, cancellationToken) = sendAsyncImpl request

let mockHttpClient (handler: HttpMessageHandler) = new HttpClient(handler)

let createDynamicResponse (lastModifiedDate: DateTimeOffset) =
    new MockHttpMessageHandler(fun request ->
        let ifModifiedSince = request.Headers.IfModifiedSince

        if ifModifiedSince.HasValue && ifModifiedSince.Value >= lastModifiedDate then
            new HttpResponseMessage(HttpStatusCode.NotModified) |> Task.FromResult
        else
            let response = new HttpResponseMessage(HttpStatusCode.OK)
            response.Content <- new StringContent("Content has changed since the last modification date")
            response.Content.Headers.LastModified <- Nullable(lastModifiedDate)
            response |> Task.FromResult)

[<Fact>]
let ``GetAsync returns NotModified or OK based on IfModifiedSince header`` () =
    // Arrange
    let url = "http://example.com"
    let lastModifiedDate = DateTimeOffset(DateTime(2023, 1, 1))
    let client = mockHttpClient (createDynamicResponse lastModifiedDate)

    // Case 1: When If-Modified-Since is equal to lastModifiedDate
    let result1 = getAsync client url (Some lastModifiedDate) |> Async.RunSynchronously

    match result1 with
    | Success content -> Assert.Equal("No changes", content)
    | Failure error -> failwithf "Expected success, but got failure: %s" error

    // Case 2: When If-Modified-Since is before lastModifiedDate
    let earlierDate = lastModifiedDate.AddDays(-1.0)
    let result2 = getAsync client url (Some earlierDate) |> Async.RunSynchronously

    match result2 with
    | Success content -> Assert.Equal("Content has changed since the last modification date", content)
    | Failure error -> failwithf "Expected success, but got failure: %s" error

    // Case 3: When If-Modified-Since is not provided
    let result3 = getAsync client url None |> Async.RunSynchronously

    match result3 with
    | Success content -> Assert.Equal("Content has changed since the last modification date", content)
    | Failure error -> failwithf "Expected success, but got failure: %s" error

[<Fact>]
let ``Test fetchWithCache with no cache`` () =
    let url = "http://example.com/test"
    let expectedContent = "Mock response content"
    let responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    responseMessage.Content <- new StringContent(expectedContent)

    let handler = new MockHttpResponseHandler(responseMessage)
    let client = new HttpClient(handler)

    let filename = convertUrlToValidFilename url
    let currentDir = Directory.GetCurrentDirectory()
    let filePath = Path.Combine(currentDir, filename)

    // Ensure the file does not exist before the test
    if File.Exists(filePath) then
        File.Delete(filePath)

    let result = fetchWithCache client currentDir url |> Async.RunSynchronously

    match result with
    | Success _ ->
        Assert.True(File.Exists(filePath), "Expected file to be created")
        let fileContent = File.ReadAllText(filePath)
        Assert.Equal(expectedContent, fileContent)
    | Failure error -> Assert.True(false, error)

    // Clean up
    if File.Exists(filePath) then
        File.Delete(filePath)

[<Fact>]
let ``Test fetchWithCache with existing cache less than 1 hour old`` () =
    let url = "http://example.com/test"
    let expectedContent = "Cached response content"

    // Create a mock handler that throws an exception if called
    let handler =
        new MockHttpMessageHandler(fun _ -> failwith "HTTP request should not be made")

    let client = new HttpClient(handler)

    let filename = convertUrlToValidFilename url
    let currentDir = Directory.GetCurrentDirectory()
    let filePath = Path.Combine(currentDir, filename)

    // Write the expected content to the file and set its last write time to less than 1 hour ago
    File.WriteAllText(filePath, expectedContent)
    File.SetLastWriteTime(filePath, DateTime.Now.AddMinutes(-30.0))

    let result = fetchWithCache client currentDir url |> Async.RunSynchronously

    match result with
    | Success content -> Assert.Equal(expectedContent, content)
    | Failure error -> Assert.True(false, error)

    // Clean up
    if File.Exists(filePath) then
        File.Delete(filePath)

[<Fact>]
let ``Test fetchWithCache with existing cache more than 1 hour old`` () =
    let url = "http://example.com/test"
    let cachedContent = "Old cached response content"
    let newContent = "New response content"
    let responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    responseMessage.Content <- new StringContent(newContent)

    let handler = new MockHttpResponseHandler(responseMessage)
    let client = new HttpClient(handler)

    let filename = convertUrlToValidFilename url
    let currentDir = Directory.GetCurrentDirectory()
    let filePath = Path.Combine(currentDir, filename)

    // Write the cached content to the file and set its last write time to more than 1 hour ago
    File.WriteAllText(filePath, cachedContent)
    File.SetLastWriteTime(filePath, DateTime.Now.AddHours(-2.0))

    let result = fetchWithCache client currentDir url |> Async.RunSynchronously

    match result with
    | Success content ->
        Assert.Equal(newContent, content)
        let fileContent = File.ReadAllText(filePath)
        Assert.Equal(newContent, fileContent)
    | Failure error -> Assert.True(false, error)

    if File.Exists(filePath) then
        File.Delete(filePath)

[<Fact>]
let ``Test fetchWithCache with existing cache more than 1 hour old and 304 response`` () =
    let url = "http://example.com/test"
    let cachedContent = "Old cached response content"
    let responseMessage = new HttpResponseMessage(HttpStatusCode.NotModified)

    let handler = new MockHttpResponseHandler(responseMessage)
    let client = new HttpClient(handler)

    let filename = convertUrlToValidFilename url
    let currentDir = Directory.GetCurrentDirectory()
    let filePath = Path.Combine(currentDir, filename)

    // Write the cached content to the file and set its last write time to more than 1 hour ago
    File.WriteAllText(filePath, cachedContent)
    let oldWriteTime = DateTime.Now.AddHours(-2.0)
    File.SetLastWriteTime(filePath, oldWriteTime)

    let result = fetchWithCache client currentDir url |> Async.RunSynchronously

    match result with
    | Success content ->
        Assert.Equal(cachedContent, content)
        let newWriteTime = File.GetLastWriteTime(filePath)
        Assert.True(newWriteTime > oldWriteTime, "Expected file write time to be updated")
    | Failure error -> Assert.True(false, error)

    // Clean up
    if File.Exists(filePath) then
        File.Delete(filePath)

[<Fact>]
let ``Test Html encoding of special characters`` () =
    let expected =
        """
    <div class="feed-item">
        <h2><a href="https://rachelbythebay.com/w/2024/02/24/signext/" target="_blank">1 &lt;&lt; n vs. 1U &lt;&lt; n and a cell phone autofocus problem</a></h2>
        <div class="source-date">rachelbythebay.com on Sunday, February 25, 2024</div>
        <p>Maybe 15 years ago, &amp; I heard that a certain cell phone camera would lose the ability to autofocus for about two weeks, then it would go back to working for another two weeks, and so on. It had something to do with the time ( since the epoch), the bits in u...</p>
    </div>
    """

    let actual =
        { Title = "1 << n vs. 1U << n and a cell phone autofocus problem"
          Text =
            "Maybe 15 years ago, & I heard that a certain cell phone camera would lose the ability to autofocus for about two weeks, then it would go back to working for another two weeks, and so on. It had something to do with the time ( since the epoch), the bits in u..."
          PostDate = Some(DateTime(2024, 02, 25))
          Url = "https://rachelbythebay.com/w/2024/02/24/signext/"
          BaseUrl = "rachelbythebay.com" }
        |> convertArticleToHtml

    Assert.Equal(expected, actual)
