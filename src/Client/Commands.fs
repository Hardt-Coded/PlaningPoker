[<RequireQualifiedAccess>]
module Commands

open Fable.Remoting.Client
open Shared
open Shared.Api
open Shared.Domain
open Models
open Elmish
open SignalRHelper


let baseUrl = "http://localhost:7071"

let pokerApi =
    Remoting.createApi()
    |> Remoting.withBaseUrl baseUrl
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IPokerApi>


[<RequireQualifiedAccess>]
module SignalR =

    open Fable.Core
    open Fable.SimpleHttp
    open Fable.SimpleJson

    type SignalRConnectionInfo = {
        url:string
        accessToken:string
    }        

    let private getConnectionInfo (gameId:string) =
        async {
            let url = $"{baseUrl}/api/negotiate?username={gameId}&hubname={Shared.SignalR.hubName}"

            let! respo = 
                Http.request url
                |> Http.method HttpMethod.GET
                |> Http.header (Header.Header ("x-ms-signalr-userid", gameId))
                |> Http.send
            
            match respo.statusCode with
            | 200 -> 
                Fable.Core.JS.console.log ("signalr negotiated!")
                let content = Json.parseAs<SignalRConnectionInfo> respo.responseText         
                return content
            | _ -> 
                return failwith ("error getting signalr service info")
        }


    open Fable.Core.JsInterop

    let private openSignalRConnection (info:SignalRConnectionInfo) onNewState =
        async {
            
            Fable.Core.JS.console.log ("signalr init connection!")

            let connection =
                SignalRHelper.signalR.CreateHubConnectionBuilder()
                    .withUrl(info.url, !!{| accesTokenFactory = (fun () -> info.accessToken) |})
                    .build()
            
            connection.on("newState",(fun (data:obj) -> onNewState data))

            Fable.Core.JS.console.log ("signalr connecting!")
            do! connection.start() |> Async.AwaitPromise
            Fable.Core.JS.console.log ("signalr connected!")

            return connection
        }



    let connectSignalRCmd gameId =
        fun dispatch ->
            async {
                try
                    let gameId = gameId |> GameId.extract
                    let! info = getConnectionInfo gameId
                    let! connection = openSignalRConnection info (fun data -> 
                        let payload = Json.parseAs<{| GameId:GameId; GameModel:GameModel |}> (data |> string)
                        let incommingGameId = payload.GameId |> GameId.extract
                        if (gameId <> incommingGameId) then
                            ()
                        else
                            dispatch <| SetCurrentGameState payload.GameModel
                    )
                    dispatch <| SignalRConnected connection
                with
                | _ as ex ->
                    dispatch (OnError ex.Message)
                    raise ex
    
            } |> Async.StartImmediate
            
        |> Cmd.ofSub


    let disconnectSignalRCmd (connection:IHubConnection option) =
        fun dispatch ->
            async {
                try
                    match connection with
                    | None ->
                        ()
                    | Some connection ->
                        connection.off("newState")
                        do! connection.stop() |> Async.AwaitPromise
                        dispatch <| SignalRDisconnected
                with
                | _ as ex ->
                    dispatch (OnError ex.Message)
                    raise ex
    
            } |> Async.StartImmediate
        
        |> Cmd.ofSub


let private sendCommandWithSucceed f succeedDispatch =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            try
                let! state = f()
            
                match state with
                | Ok state -> 
                    dispatch <| SetCurrentGameState state
                    succeedDispatch dispatch
                | Error e -> dispatch <| OnError e
            with
            | ex ->
                dispatch <| OnError ex.Message

            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub


let private sendCommand f =
    sendCommandWithSucceed f (ignore)


let loadState gameId =
    sendCommand (fun () -> pokerApi.getState gameId)


let createGame currentPlayer =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            try
                let! state = pokerApi.createGame currentPlayer
                match state with
                | Ok state -> 
                    match state with
                    | GameModel.GotGameId gameId ->
                        dispatch <| GameCreated (currentPlayer, gameId, state)
                    | _ ->
                        dispatch <| OnError "Game Server doesn't switch to the right Game State. Please try again!"
                | Error e -> dispatch <| OnError e
            with
            | ex ->
                dispatch <| OnError ex.Message

            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub


let resetWhenGameNotExists gameId =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            try
                let! state = pokerApi.getState gameId
                match state with
                | Ok _ ->
                    ()
                | Error e ->
                    Browser.Dom.console.log(e)
                    dispatch <| Reset
                    dispatch <| Navigate []
            with
            | ex ->
                dispatch <| OnError ex.Message
            
            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub


let joinGameFromCookiesOrCheckGameExisits () =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            match Cookies.getCurrentPlayer(), Cookies.getGameId() with
            | Some currentPlayer, Some gameId ->

                try
                    let! state = pokerApi.joinGame gameId currentPlayer
                    match state with
                    | Ok state -> 
                        match state with
                        | GameModel.GotGameId gameId ->
                            dispatch <| GameJoined (currentPlayer, gameId, state)
                        | _ ->
                            dispatch <| OnError "Game Server doesn't switch to the right Game State. Please try again!"
                    | Error e ->
                        Browser.Dom.console.log(e)
                        dispatch <| Reset
                with
                | ex ->
                    dispatch <| OnError ex.Message

            | _ ->
                ()
            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub


let joinGame gameId currentPlayer =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            try
                let! state = pokerApi.joinGame gameId currentPlayer
                match state with
                | Ok state -> 
                    match state with
                    | GameModel.GotGameId gameId ->
                        dispatch <| GameJoined (currentPlayer, gameId, state)
                    | _ ->
                        dispatch <| OnError "Game Server doesn't switch to the right Game State. Please try again!"
                | Error e -> dispatch <| OnError e
            with
            | ex ->
                dispatch <| OnError ex.Message

            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub

// "Game doesn't exists"
let endGame gameId currentPlayer = 
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            try
                let! state = pokerApi.endGame gameId currentPlayer
            
                match state with
                | Ok state -> 
                    dispatch <| SetCurrentGameState state
                | Error "Game doesn't exists" -> // exception, if the game not exisit any more, leave display no error and end the game
                    dispatch <| SetCurrentGameState (GameEnded gameId)
                | Error e -> dispatch <| OnError e
            with
            | ex ->
                dispatch <| OnError ex.Message

            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub
    

let leaveGame gameId currentPlayer playerToLeave = 
    sendCommand (fun () -> pokerApi.leaveGame gameId currentPlayer playerToLeave)

let startRound gameId currentPlayer = 
    sendCommand (fun () -> pokerApi.startRound gameId currentPlayer)

let finishRound gameId currentPlayer = 
    sendCommand (fun () -> pokerApi.finishRound gameId currentPlayer)

let playCard gameId currentPlayer card = 
    sendCommand (fun () -> pokerApi.playCard gameId currentPlayer card)


let setCookies gameId player =
    fun _ ->
        Cookies.setCurrentPlayer player
        Cookies.setGameId gameId
    |> Cmd.ofSub

let removeCookies () =
    fun _ ->
        Cookies.removeAllCookies ()
    |> Cmd.ofSub

    




[<RequireQualifiedAccess>]
module WebSocket =

    open Browser.Types
    open Fable.SimpleJson
    

    type ChannelMessage = { Topic: string; Payload: string }
    

    let connectWebSocketCmd (gameId:GameId) =
        fun dispatch ->
            ()


        |> Cmd.ofSub


    //let disconnectWebsocket (ws:WebSocket) =
    //    fun dispatch ->
    //        ()
    //    |> Cmd.ofSub