module Models

    open Shared.Domain

    type Theme =
        | Dark
        | Light


    open SignalRHelper


    type CardTheme =
        | Monster
        | Scifi
        | Unicorn

    type InGameArgs = {
        CurrentPlayer: Player;
        SignalRConnection: IHubConnection option;
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
        Message: string * string
        IsLoading:bool
        View: View
        CardRecentlySelected: bool
        CardTheme: CardTheme
    }


    type Msg =
        | CreateGame
        | GameCreated of Player * GameId * GameModel
        | JoinGame
        | GameJoined of Player * GameId * GameModel

        | GameMsg of GameMsg
        | ChangeName of string
        | ChangeId of string
        | LoadState
        | SetCurrentGameState of GameModel * Player option
        | ConnectToSignalR of GameId
        | SignalRConnected of IHubConnection
        | DisconnectSignalR
        | SignalRDisconnected
        | OnError of string
        | OnMessage of title:string * message:string
        | ClearError
        | ClearMessage
        | ResetCardRecentlySelectedFlag

        | ToggleTheme
        | SwitchCardTheme of CardTheme
        | IsLoading of bool
        | Navigate of string list
        | SetCookies
        | UrlChanged of string list

        | Reset