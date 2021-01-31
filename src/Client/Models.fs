module Models

    open Shared.Domain

    type Theme = 
        | Dark
        | Light



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
        | OnError of string
        | ClearError

        | ToggleTheme
        | IsLoading of bool
        | Navigate of string list
        | SetCookies

