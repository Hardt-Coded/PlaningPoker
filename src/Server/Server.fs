module Server

open Giraffe
open Saturn

open Shared
open Microsoft.Extensions.DependencyInjection
open Domain
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Microsoft.AspNetCore.Http
open Shared.Api
open FSharp.Control.Tasks
open Saturn.Channels
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open System
open Microsoft.ApplicationInsights
open Microsoft.Extensions.Logging.ApplicationInsights
open Saturn


//let [<Literal>]WebSocketChannelPath = "/socket/poker"

//let createPokerApiFromContext (httpContext: HttpContext) : IPokerApi = 
//    let todoStore = httpContext.GetService<GameEngine>()
//    let iSocketHub = httpContext.GetService<ISocketHub>()
//    Api.pokerApi todoStore iSocketHub WebSocketChannelPath


//let webApp : HttpHandler = 
//    Remoting.createApi()
//    |> Remoting.withRouteBuilder Route.builder
//    |> Remoting.fromContext createPokerApiFromContext
//    |> Remoting.buildHttpHandler


//let channel =
//    channel {
//        join (fun ctx clientInfo ->
//            task {
//                printfn "Someone has connected!"
//                return Channels.Ok
//            }
//        )
//    }

//let configAi (logging:ILoggingBuilder) =
//    logging.ClearProviders()  |> ignore
//    logging.AddApplicationInsights() |> ignore
//    logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace) |> ignore
//    logging.AddConsole() |> ignore

//let configureServices (services: IServiceCollection) =
//    services.AddApplicationInsightsTelemetry() |> ignore
    
//    let gameEngineFactory (sp:IServiceProvider) =
//        let logger = sp.GetService<ILogger<GameEngine>>()
        
//        let log (str:string) = 
//            logger.LogInformation(str)

//        let configuration = sp.GetService<IConfiguration>()

//        let connectionStr = configuration.GetValue("TableStorageConnectionString")

//        let dbRepo = DataAccess.initGameRepository connectionStr

//        GameEngine(log, dbRepo.getGameState, dbRepo.addGameState, dbRepo.updateGameState, dbRepo.deleteGameState)

//    services.AddSingleton<GameEngine>(gameEngineFactory)



open Saturn
open Saturn.AzureFunctions
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.SignalRService
open Microsoft.Azure.WebJobs.Extensions.Http

let private webApp pokerApi : HttpHandler = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.functionBuilder
    |> Remoting.fromValue pokerApi
    |> Remoting.buildHttpHandler

let private func pokerApi log = azureFunction {
    host_prefix "/api"
    use_router (webApp pokerApi)
    logger log
}


let private gameEngineFactory (logger:ILogger) =
    let log (str:string) = 
        logger.LogInformation(str)
        
    // use the same storage as the function itself
    let connectionStr = 
        System.Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process)

    let dbRepo = DataAccess.initGameRepository connectionStr

    GameEngine(log, dbRepo.getGameState, dbRepo.addGameState, dbRepo.updateGameState, dbRepo.deleteGameState)



[<FunctionName("negotiate")>]
let negotiate(
    [<HttpTrigger(AuthorizationLevel.Anonymous)>]req: HttpRequest,
    [<SignalRConnectionInfo(HubName = "poker", UserId="{headers.x-ms-signalr-userid}")>]connectionInfo: SignalRConnectionInfo) : SignalRConnectionInfo =
    connectionInfo


[<FunctionName("PokerEndpoint")>]
let pokerEndpoint (
    [<HttpTrigger(AuthorizationLevel.Anonymous, Route = "{*any}")>]req: HttpRequest, 
    [<SignalR(HubName = Shared.SignalR.hubName)>] signalRMessages:IAsyncCollector<SignalRMessage>,
    log: ILogger) =
    let gameEngine = gameEngineFactory log
    let pokerApi = Api.pokerApi gameEngine signalRMessages
    func pokerApi log req



