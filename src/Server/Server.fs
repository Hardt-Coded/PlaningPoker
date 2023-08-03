module Server

open Giraffe
open Shared
open Domain
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System
open Microsoft.ApplicationInsights
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

    GameEngineFunction(log, dbRepo.getGameState, dbRepo.addGameState, dbRepo.updateGameState, dbRepo.deleteGameState)



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



