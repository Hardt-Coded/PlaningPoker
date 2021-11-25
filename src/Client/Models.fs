module Models

    open Shared.Domain

    type Theme = 
        | Dark
        | Light


    open Browser.Types

    

    type InGameArgs = {
        CurrentPlayer: Player; 
        //WebSocket: WebSocket option; 
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
        Message: (string * string)
        IsLoading:bool
        View: View
    }

     
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
        | ConnectToSignalR of GameId
        | SignalRConnected
        | DisconnectWebSocket
        | WebSocketDisconnected
        | OnError of string
        | OnMessage of title:string * message:string
        | ClearError
        | ClearMessage

        | ToggleTheme
        | IsLoading of bool
        | Navigate of string list
        | SetCookies
        | UrlChanged of string list

        | Reset