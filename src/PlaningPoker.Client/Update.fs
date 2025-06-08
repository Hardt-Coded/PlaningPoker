module Update

open Shared.Domain
open PlaningPoker.Shared
open Models
open Elmish
open FsToolkit.ErrorHandling
open Feliz.Router
open Shared.Domain.GameModel



let init isDarkMode =
    let currentUrl = Router.currentUrl ()
    let id =
        match currentUrl with
        | [] -> ""
        | _ -> currentUrl.[0]

    let state = {
        Error = ""
        Message = "",""
        Theme = if isDarkMode then Dark else Light
        IsLoading = false
        View =
            if id = "" then
                CreateGameView {| Name = "" |}
            else
                JoinGameView {| Name = ""; Id = id |}
    }

    let cmdResult =
        result {
            let! gameIdFromCookies = Cookies.getGameId() |> Result.ofOption ""
            let! gameIdFromUrlId = GameId.create id
            if (gameIdFromCookies<>gameIdFromUrlId) then
                return [ Commands.resetWhenGameNotExists gameIdFromUrlId ]
            else
                return [ Commands.joinGameFromCookiesOrCheckGameExists () ]
        }
    match cmdResult with
    | Ok cmd ->
        state, cmd
    | Error _ ->
        state, Cmd.none



let update (msg:Msg) state =
    Fable.Core.JS.console.log($"Update: %A{msg} with state: %A{state}")
    match state.View, msg with
    | _, Reset ->
        {
            Error = ""
            Message = "",""
            Theme = state.Theme
            IsLoading = false
            View = CreateGameView {| Name = "" |}
        }, Cmd.batch [  [ Commands.removeCookies () ]; Cmd.ofMsg (Navigate [ "" ]) ]

    | CreateGameView viewState, CreateGame ->
        let player = Player.create viewState.Name
        match player with
        | Error e ->
            state, Cmd.ofMsg <| OnError e
        | Ok player ->
            state, [ Commands.sendMsgToServer GameId.none (Some player) (GameMsg.CreateGame player) ]
    | CreateGameView _, SetCurrentGameState (newGameState, player) ->
       match newGameState, player with
       | GotGameId (gameId, _), Some player ->
            state, Cmd.ofMsg <| GameCreated (player, gameId, newGameState)
       | _ ->
            let err = "GameId was not created. Please try again!"
            state, Cmd.ofMsg <| OnError err

    | JoinGameView _, SetCurrentGameState (newGameState, player) ->
       match newGameState, player with
       | GotGameId (gameId, _), Some player ->
            state, Cmd.ofMsg <| GameJoined (player, gameId, newGameState)
       | _ ->
            let err = "GameId was not created. Please try again!"
            state, Cmd.ofMsg <| OnError err

    | CreateGameView _, GameCreated (player,gameId,gameState) ->
        let view = InGameView {
            CurrentPlayer = player
            SignalRConnection = None
            GameId = gameId
            CurrentGameState = gameState
        }
        let cmds =
            Cmd.batch [
                Cmd.ofMsg <| ConnectToSignalR gameId
                Cmd.ofMsg <| SetCookies
                Cmd.ofMsg <| Navigate [ GameId.extract gameId ]
                Cmd.ofMsg <| OnMessage ("Share the Link", "In order to add new players, please share the link from your browser address with the others.")
            ]
        { state with View = view }, cmds

    | JoinGameView viewState, JoinGame ->
        let cmdResult =
            result {
                let! player = Player.create viewState.Name
                let! gameId = GameId.create viewState.Id
                return [ Commands.sendMsgToServer gameId (Some player) (GameMsg.JoinGame player) ]
            }
        match cmdResult with
        | Error e ->
            state, Cmd.ofMsg <| OnError e
        | Ok cmd ->
            state, cmd

    | JoinGameView _, GameJoined (player, gameId, gameState) ->
        let view = InGameView {
            CurrentPlayer = player
            SignalRConnection = None
            GameId = gameId
            CurrentGameState = gameState
        }
        let cmds =
            Cmd.batch [
                Cmd.ofMsg <| ConnectToSignalR gameId
                Cmd.ofMsg <| SetCookies
                Cmd.ofMsg <| Navigate [ GameId.extract gameId ]
            ]
        { state with View = view }, cmds

    // in case of cookie joining!
    | CreateGameView _, GameJoined (player, gameId, gameState) ->
        let view = InGameView {
            CurrentPlayer = player
            SignalRConnection = None
            GameId = gameId
            CurrentGameState = gameState
        }
        let cmds =
            Cmd.batch [
                Cmd.ofMsg <| ConnectToSignalR gameId
                Cmd.ofMsg <| SetCookies
                Cmd.ofMsg <| Navigate [ GameId.extract gameId ]
            ]
        { state with View = view }, cmds


    | CreateGameView viewState, ChangeName name ->
        let newViewState = {| viewState with Name = name |}
        { state with View = CreateGameView newViewState }, Cmd.none
    | JoinGameView viewState, ChangeName name ->
        let newViewState = {| viewState with Name = name |}
        { state with View = JoinGameView newViewState }, Cmd.none
    | JoinGameView viewState, ChangeId id ->
        let newViewState = {| viewState with Id = id |}
        { state with View = JoinGameView newViewState }, Cmd.none

    | InGameView viewState, GameMsg StartRound ->
        state, [ Commands.sendMsgToServer viewState.GameId (Some viewState.CurrentPlayer) GameMsg.StartRound ]

    | InGameView viewState, GameMsg FinishRound ->
        state, [ Commands.sendMsgToServer viewState.GameId (Some viewState.CurrentPlayer) GameMsg.FinishRound ]

    | InGameView viewState, GameMsg (LeaveGame playerToLeave) ->
        let cmd =
            let cmds = [
                [ Commands.sendMsgToServer viewState.GameId (Some viewState.CurrentPlayer) (GameMsg.LeaveGame playerToLeave) ]
                // Disconnect from the WebSocket, so you don't get any refreshed.
                // also reset the state
                if (viewState.CurrentPlayer = playerToLeave) then
                    Cmd.ofMsg DisconnectSignalR
                    Cmd.ofMsg Reset
            ]
            Cmd.batch cmds

        state, cmd

    | InGameView viewState, GameMsg EndGame ->
        state, [ Commands.sendMsgToServer viewState.GameId (Some viewState.CurrentPlayer) GameMsg.EndGame ]

    | InGameView viewState, GameMsg (PlayCard card) ->
        state, [ Commands.sendMsgToServer viewState.GameId (Some viewState.CurrentPlayer) (GameMsg.PlayCard card) ]

    | InGameView viewState, GameMsg _ ->
        state, Cmd.none

    | InGameView viewState, LoadState ->
        state, [Commands.loadState viewState.GameId ]
    | InGameView viewState, SetCurrentGameState (gameModel, _) ->
        match gameModel with
        | GameEnded gameId -> // game was ended by admin
            let cmds =
                [
                    Cmd.ofMsg DisconnectSignalR
                    Cmd.ofMsg Reset
                ] |> Cmd.batch
            state, cmds
        | InGame inGameModel ->
            let didYouExistAnyMore = inGameModel.Players |> List.exists (fun p -> p = viewState.CurrentPlayer)
            if didYouExistAnyMore then
                let newViewState = { viewState with CurrentGameState = gameModel }
                { state with View = InGameView newViewState; Error = "" }, Cmd.none
            else
                // reset game and disconnect from signalR
                let cmd =
                    let cmds = [
                            Cmd.ofMsg DisconnectSignalR
                            Cmd.ofMsg Reset
                    ]
                    Cmd.batch cmds
                state, cmd

        | _ ->
            let newViewState = { viewState with CurrentGameState = gameModel }
            { state with View = InGameView newViewState; Error = "" }, Cmd.none

    // if the Gamestate comes over the web socket and if you not anymore in the game. Nobody cares.
    | InGameView viewState, ConnectToSignalR gameId ->
        state, [ Commands.SignalR.connectSignalRCmd gameId ]
    | InGameView viewState, SignalRConnected connection ->
        { state with View = InGameView { viewState with SignalRConnection = Some connection }}, []
    | InGameView viewState, DisconnectSignalR ->
        state, [ Commands.SignalR.disconnectSignalRCmd viewState.SignalRConnection ]
    | InGameView viewState, SignalRDisconnected ->
        { state with View = InGameView { viewState with SignalRConnection = None }}, []
    | _, SignalRDisconnected ->
        state, Cmd.none
    | _, OnError error ->
        { state with Error = error }, Cmd.none
    | _, OnMessage (title,error) ->
        { state with Message = title,error }, Cmd.none
    | _, ClearError ->
        { state with Error = "" }, Cmd.none
    | _, ClearMessage ->
        { state with Message = ("","") }, Cmd.none
    | _, ToggleTheme ->
        let newTheme =
            match state.Theme with
            | Dark -> Light
            | Light -> Dark
        { state with Theme = newTheme }, Cmd.none
    | _, IsLoading b ->
        { state with IsLoading = b }, Cmd.none
    | _, Navigate segments ->
        match segments with
        | [ id ] ->
            state, [ (fun _ -> Router.navigate (segments,[])) ]
        | _ ->
            state, Cmd.none
    | InGameView viewState, SetCookies ->
        state, [ Commands.setCookies viewState.GameId viewState.CurrentPlayer ]
    | CreateGameView cv, UrlChanged currentUrl ->
        match currentUrl with
        | [ id ] ->
            {state with View = JoinGameView {| Name= cv.Name; Id = id |}}, Cmd.none
        | _ ->
            state, Cmd.none

    | JoinGameView jv, UrlChanged currentUrl ->
        match currentUrl with
        | [] ->
            {state with View = CreateGameView {| Name= jv.Name |}}, Cmd.none
        | _ ->
            state, Cmd.none
    | InGameView _, UrlChanged _ ->
        state, Cmd.none
    | _ ->
        let msgStr = $"%A{msg}"
        let stateStr = $"%A{state}"
        Browser.Dom.console.log($"message: {msgStr}")
        Browser.Dom.console.log($"state: {stateStr}")
        let err = $"Invalid Message '{msg}' with current State. Show Console for more details!"
        { state with Error = err }, Cmd.none

