[<RequireQualifiedAccess>]
module Commands

open Fable.Remoting.Client
open Shared
open Shared.Api
open Shared.Domain
open Models
open Elmish

let pokerApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IPokerApi>


let private sendCommandWithSucceed f succeedDispatch =
    fun dispatch ->
        async {
            dispatch <| IsLoading true

            let! state = f()
            match state with
            | Ok state -> 
                dispatch <| SetCurrentGameState state
                succeedDispatch dispatch
            | Error e -> dispatch <| OnError e

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
            let! state = pokerApi.createGame currentPlayer
            match state with
            | Ok state -> 
                dispatch <| SetCurrentGameState state
                match state with
                | InGame { Game = Game.GetGameId gameId } ->
                    dispatch <| SetCurrentPlayer currentPlayer
                    dispatch <| ConnectToWebSocket gameId
                    dispatch <| SetGameId gameId
                    dispatch <| SetCookies
                | _ ->
                    dispatch <| OnError "Game Server doesn't switch to the right Game State. Please try again!"
            | Error e -> dispatch <| OnError e

            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub


let joinGameFromCookies () =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            match Cookies.getCurrentPlayer(), Cookies.getGameId() with
            | Some currentPlayer, Some gameId ->
                let! state = pokerApi.joinGame gameId currentPlayer
                match state with
                | Ok state -> 
                    dispatch <| SetCurrentGameState state
                    match state with
                    | InGame { Game = Game.GetGameId gameId } ->
                        dispatch <| SetCurrentPlayer currentPlayer
                        dispatch <| ConnectToWebSocket gameId
                        dispatch <| SetGameId gameId
                        dispatch <| SetCookies
                        dispatch <| Navigate [ GameId.extract gameId ]
                    | _ ->
                        dispatch <| OnError "Game Server doesn't switch to the right Game State. Please try again!"
                | Error e ->
                    Browser.Dom.console.log(e)
            | _ ->
                ()

            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub


let joinGame gameId currentPlayer =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            let! state = pokerApi.joinGame gameId currentPlayer
            match state with
            | Ok state -> 
                dispatch <| SetCurrentGameState state
                match state with
                | InGame { Game = Game.GetGameId gameId } ->
                    dispatch <| SetCurrentPlayer currentPlayer
                    dispatch <| ConnectToWebSocket gameId
                    dispatch <| SetGameId gameId
                    dispatch <| SetCookies
                    dispatch <| Navigate [ GameId.extract gameId ]
                | _ ->
                    dispatch <| OnError "Game Server doesn't switch to the right Game State. Please try again!"
            | Error e -> dispatch <| OnError e

            dispatch <| IsLoading false
        } |> Async.StartImmediate
    |> Cmd.ofSub


let endGame gameId currentPlayer = 
    sendCommand (fun () -> pokerApi.endGame gameId currentPlayer)

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

    




[<RequireQualifiedAccess>]
module WebSocket =

    open Browser.Types
    open Browser.WebSocket
    open Fable.SimpleJson
    

    type ChannelMessage = { Topic: string; Payload: string }
    

    let connectWebSocketCmd (gameId:GameId) =
        fun dispatch ->
            let onWebSocketMessage (msg:MessageEvent) =
                let msg = msg.data |> string |> Json.parseAs<ChannelMessage>
                let id = GameId.extract gameId
                if (id = msg.Topic) then
                    msg.Payload |> Json.parseAs<GameModel> |> SetCurrentGameState |> dispatch
                else
                    ()

            let rec connect () =
                let host = Browser.Dom.window.location.host
                let url = sprintf "ws://%s/socket/poker" host
                let ws = WebSocket.Create(url)

                ws.onopen <- (fun _ -> printfn "connection opened!")
                ws.onclose <- (fun _ ->
                    printfn "connection closed!"    
                    promise {
                        do! Promise.sleep 2000
                        connect()
                    }
                )
                ws.onmessage <- onWebSocketMessage

            connect()
        |> Cmd.ofSub