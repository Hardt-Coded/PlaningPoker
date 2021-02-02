module Models

    open Shared.Domain

    type Theme = 
        | Dark
        | Light


    open Browser.Types

    type Model =
        { 
            Theme: Theme
            CurrentGameState: GameModel
            GameId: GameId option
            CurrentPlayer: Player option
            Name: string
            Id: string
            Error: string
            IsLoading:bool
            WebSocket: WebSocket option
        }
    
    type Msg =
        | CreateGame
        | JoinGame

        | GameMsg of Shared.Domain.Msg
        | ChangeName of string
        | ChangeId of string
        | SetGameId of GameId
        | LoadState
        | SetCurrentGameState of GameModel
        | SetCurrentPlayer of Player
        | ConnectToWebSocket of GameId
        | SetWebSocketHandler of WebSocket
        | DisconnectWebSocket
        | OnError of string
        | ClearError

        | ToggleTheme
        | IsLoading of bool
        | Navigate of string list
        | SetCookies
        | ReInit of string list

        | Reset