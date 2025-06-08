namespace PlaningPoker2.Server

open System
open Fable.Remoting.Server
open Fable.Remoting.AzureFunctions.Worker
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.Functions.Worker.SignalRService
open Microsoft.Azure.SignalR.Management
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Shared.Api



type Functions(sp:IServiceProvider, logger:ILogger<Functions>,  configuration: IConfiguration)  =
    inherit ServerlessHub(sp)

    let gameEngineFactory (logger:ILogger) =
        // use the same storage as the function itself
        let connectionStr =
            configuration.GetValue("AzureWebJobsStorage")
        let dbRepo = DataAccess.initGameRepository connectionStr
        GameEngine(logger, dbRepo)


    [<Function("negotiate")>]
    // member _.Negotiate(
    //     [<HttpTrigger(AuthorizationLevel.Anonymous)>] _req: HttpRequestData,
    //     [<SignalRConnectionInfoInput(HubName = "poker", UserId="{headers.x-ms-client-principal-id}")>]connectionInfo: string) : string =
    //     connectionInfo
    member this.Negotiate(
        [<HttpTrigger(AuthorizationLevel.Anonymous)>] req: HttpRequestData
        ) =
        let userId = if req.Headers.Contains("x-ms-client-principal-id") then req.Headers.GetValues("x-ms-client-principal-id") |> Seq.tryHead else None
        let option = NegotiationOptions()
        option.UserId <- userId |> Option.defaultValue "anonymous"
        let a = base.NegotiateAsync(option)
        task {
            let! negotiateResponse = a;
            let response = req.CreateResponse()
            // Cors headers
            response.Headers.Add("Access-Control-Allow-Origin", "*")
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS")
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, x-ms-client-principal-id")
            response.Headers.Add("Access-Control-Allow-Credentials", "true")
            req.Headers.Add("Content-Type", "application/json")
            response.WriteBytes(negotiateResponse.ToArray());
            return response;
        }




    [<Function("PokerEndpoint")>]
    member this.PokerEndpoint ([<HttpTrigger(AuthorizationLevel.Anonymous, Route = "{*any}")>] req: HttpRequestData) =
        let gameEngine = gameEngineFactory logger
        let api = Api.pokerApi gameEngine this.Clients

        Remoting.createApi()
        |> Remoting.withRouteBuilder IPokerApi.EndpointBuilder
        |> Remoting.fromValue api
        |> Remoting.withErrorHandler (Remoting.errorHandler logger)
        |> Remoting.buildRequestHandler
        |> HttpResponseData.fromRequestHandler req

