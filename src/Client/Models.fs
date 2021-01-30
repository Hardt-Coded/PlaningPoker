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
            Name: string
            Error: string
        }
    
    type Msg =
        | GameMsg of Shared.Domain.Msg
        | ChangeName of string
        | SetGameId of GameId
        | LoadState
        | SetCurrentGameState of GameModel
        | OnError of string

        | ToggleTheme

