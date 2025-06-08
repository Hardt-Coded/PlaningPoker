namespace Shared

open PlaningPoker2.Shared.API


module Domain =

    open System

    type CardValue =
        | One
        | Two
        | Three
        | Five
        | Eight
        | Thirteen
        | Twenty
        | Forty
        | Coffee
        | Stop
        | Wtf
        | TrustMeBro
        | Observer



    type Card = private Card of CardValue
    module Card =
        let create value =
            Card value

        let extract (Card value) = value



    type Player = private Player of id:Guid * name:string
    module Player =

        let create name =
            if name = "" then
                "The Name is empty!" |> Error
            else
                Player (Guid.NewGuid(), name) |> Ok

        let build (id:string) name =
            match name, id with
            | "", _ -> "The Name is empty!" |> Error
            | _, InvalidId -> "Invalid id Format" |> Error
            | _, ValidId guid -> Player (guid, name) |> Ok

        let extract (Player (id,name)) = id,name

        let (|ExtractPlayerInfo|) (Player (id,name)) = id,name


    type GameId = private GameId of string
    module GameId =

        let create id =
            if id = "" then
                "GameId is empty!" |> Error
            else
                GameId id |> Ok

        let extract (GameId id) = id

        let internal build id = GameId id

        let (|GetGameId|) (GameId gameId) = gameId

        let none = GameId ""




    type Game = private Game of id:GameId * admin:Player
    module Game =

        let create admin =
            let gameId = GameId.build <| Guid.NewGuid().ToString("N")
            Game (gameId, admin)


        let extract (Game (id, admin)) = id, admin


        let internal (|GetGameAdmin|_|) game =
            let _, admin = extract game
            Some admin

        let internal (|GetGameId|_|) game =
            let id, _ = extract game
            Some id




    type InGameState =
        | Beginning
        | InRound
        | DisplayResult



    type PlayedCard = {
        Player: Player
        Card: Card
    }


    type InGameModel = {
        Game: Game
        State: InGameState
        Players: Player list
        PlayedCards: PlayedCard list
    }


    type GameModel =
        | Init
        | InGame of InGameModel
        | GameEnded of GameId


    module GameModel =

        let (|GotGameAdmin|_|) gameModel =
            match gameModel with
            | InGame { Game = Game.GetGameAdmin admin } ->
                Some admin
            | _ ->
                None


        let (|GotGameId|_|) gameModel =
            match gameModel with
            | InGame { Game = Game.GetGameId gameId } ->
                Some (gameId, gameModel)
            | GameEnded gameId ->
                Some (gameId, gameModel)
            | _ ->
                None


        let (|GotPlayers|_|) gameModel =
            match gameModel with
            | InGame { Players = players } -> Some players
            | _ -> None


        let countPlayedCards cardValue gameModel  =
            gameModel.PlayedCards
            |> List.filter (fun pc -> (pc.Card |> Card.extract) = cardValue)
            |> List.length



    type GameMsg =
        | CreateGame    of admin:Player
        | EndGame
        | JoinGame      of player:Player
        | LeaveGame     of player:Player
        | StartRound
        | FinishRound
        | PlayCard      of card:Card



module Api =

    open Domain

    type IPokerApi = {
            getGameState    : GameId -> Result<GameModel,string> Async
            sendCommand     : GameId -> Player option -> GameMsg -> Result<GameModel,string> Async
    } with static member EndpointBuilder s m = $"/api/%s{s}/%s{m}"


    type MessageFormat = {
        GameId: GameId
        GameModel: GameModel
    }


module SignalR =

    let [<Literal>] hubName = "poker"


