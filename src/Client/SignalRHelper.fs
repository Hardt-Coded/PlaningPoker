module SignalRHelper

open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open System

type IHubConnection =
    abstract on:string * (obj -> unit) -> unit
    abstract on:string * (obj * obj -> unit) -> unit
    abstract on:string * (obj * obj * obj -> unit) -> unit
    abstract on:string * (obj * obj * obj * obj -> unit) -> unit
    abstract off:string -> unit
    abstract start:unit -> Promise<unit>
    abstract stop:unit -> Promise<unit>

type IHttpConnectionOptions =
    abstract accessTokenFactory:unit->string

type IConnectionBuilder =
    //[<Emit("$0.withUrl($1,{ accessTokenFactory: function() { return $2; } })")>]
    abstract withUrl: string * IHttpConnectionOptions -> IConnectionBuilder
    abstract build: unit-> IHubConnection 

type ISignalR =
    [<Emit("new $0.HubConnectionBuilder()")>]
    member this.CreateHubConnectionBuilder(): IConnectionBuilder = jsNative

let signalR:ISignalR = importAll "@microsoft/signalr"

