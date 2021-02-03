module Models

    open Shared.Domain

    type Theme = 
        | Dark
        | Light


    open Browser.Types

    // CreateGame
    // JoinGame
    // InGame

    type InGameArgs = {
        CurrentPlayer: Player; 
        WebSocket: WebSocket option; 
        GameId: GameId; 
        CurrentGameState: GameModel
    }

    type View =
        | CreateGameView    of {| Name: string |}
        | JoinGameView      of {| Name: string; Id: string |}
        | InGameView        of InGameArgs

    type Model = {
        Theme: Theme
        Error: string
        IsLoading:bool
        View: View
    }

    //type Model =
    //    { 
    //        Theme: Theme
    //        CurrentGameState: GameModel
    //        GameId: GameId option
    //        CurrentPlayer: Player option
    //        Name: string
    //        Id: string
    //        Error: string
    //        IsLoading:bool
    //        WebSocket: WebSocket option
    //    }
    
    type Msg =
        | CreateGame
        | GameCreated of Player * GameId * GameModel
        | JoinGame
        | GameJoined of Player * GameId * GameModel

        | GameMsg of Shared.Domain.Msg
        | ChangeName of string
        | ChangeId of string
        | LoadState
        | SetCurrentGameState of GameModel
        | ConnectToWebSocket of GameId
        | SetWebSocketHandler of WebSocket
        | DisconnectWebSocket
        | WebSocketDisconnected
        | OnError of string
        | ClearError

        | ToggleTheme
        | IsLoading of bool
        | Navigate of string list
        | SetCookies
        | UrlChanged of string list

        | Reset