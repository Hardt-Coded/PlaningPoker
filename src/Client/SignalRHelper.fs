module SignalRHelper

open System
open Fable.Core
open Fable.Core.JS

type SignalRResult = {
    Result: obj
}

type IHubConnection =
    
    
    [<Emit("$0.on($1,$2)")>]
    abstract On:string * (obj -> unit) -> unit
    
    
    [<Emit("$0.start()")>]
    abstract Start:unit -> Promise<unit>


type IConnectionBuilder =
    [<Emit("new signalr.HubConnectionBuilder()")>]
    abstract Create: unit->IConnectionBuilder
    [<Emit("$0.withUrl($1,{ accessTokenFactory: function() { return $2; } })")>]
    abstract WithUrl: string * string -> IConnectionBuilder
    [<Emit("$0.build()")>]
    abstract Build: unit-> IHubConnection



//type ISignalR =
//    member this.connectionBuilder:IConnectionBuilder = jsNative


//let signalR:ISignalR = jsNative
[<Import("*", from="@microsoft/signalr")>]
let connectionBuilder:IConnectionBuilder = jsNative


