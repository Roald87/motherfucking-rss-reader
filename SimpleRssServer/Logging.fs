module SimpleRssServer.Logging

open Microsoft.Extensions.Logging

let private loggerFactory =
    LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore)

let logger: ILogger = loggerFactory.CreateLogger("SimpleRssReader")
