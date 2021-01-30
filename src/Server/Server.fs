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


let createPokerApiFromContext (httpContext: HttpContext) : IPokerApi = 
    let todoStore = httpContext.GetService<GameEngine>()
    Api.pokerApi todoStore


let webApp : HttpHandler = 
    Remoting.createApi()
    |> Remoting.fromContext createPokerApiFromContext
    |> Remoting.buildHttpHandler


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
    }

run app
