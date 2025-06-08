module SignalRHelper

open System.Collections
open System.Collections.Generic
open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open System

type IHub =
    abstract disconnected:(unit->unit)->unit

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
    abstract headers: IDictionary<string, string>

type IConnectionBuilder =
    //[<Emit("$0.withUrl($1,{ accessTokenFactory: function() { return $2; } })")>]
    abstract withUrl: string * IHttpConnectionOptions -> IConnectionBuilder
    abstract withAutomaticReconnect: unit-> IConnectionBuilder
    abstract build: unit-> IHubConnection

type ISignalR =
    [<Emit("new $0.HubConnectionBuilder()")>]
    member this.CreateHubConnectionBuilder(): IConnectionBuilder = jsNative

let signalR:ISignalR = importAll "@microsoft/signalr"

