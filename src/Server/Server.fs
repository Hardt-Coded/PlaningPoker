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
open FSharp.Control.Tasks.V2
open Saturn.Channels


let [<Literal>]WebSocketChannelPath = "/socket/poker"

let createPokerApiFromContext (httpContext: HttpContext) : IPokerApi = 
    let todoStore = httpContext.GetService<GameEngine>()
    let iSocketHub = httpContext.GetService<ISocketHub>()
    Api.pokerApi todoStore iSocketHub WebSocketChannelPath


let webApp : HttpHandler = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromContext createPokerApiFromContext
    |> Remoting.buildHttpHandler


let channel =
    channel {
        join (fun ctx clientInfo ->
            task {
                printfn "Someone has connected!"
                return Channels.Ok
            }
        )
    }
    


let configureServices (services: IServiceCollection) =
    services.AddSingleton<GameEngine,GameEngine>()


let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
        service_config configureServices
        add_channel WebSocketChannelPath channel
    }

run app
