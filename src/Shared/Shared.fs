namespace Shared


module Domain =

    open System

    type CardValue =
        | Zero
        | Halve
        | One
        | Two
        | Three
        | Five
        | Eight
        | Thirtheen
        | Twenty
        | Fourty
        | Hundred
        | Coffee
        | Stop
        | IDontKnow


    type Card = private Card of CardValue

    module Card =

        let create value =
            Card value
    


    type Player = private Player of id:Guid * name:string

    module Player =
    
        let create name =
            Player (Guid.NewGuid(), name)


    type GameId = private GameId of string

    module GameId =

        let create id = GameId id

        let extract (GameId id) = id




    type Game = private Game of id:GameId * admin:Player

    module Game =

        let create admin =
            let gameId = GameId.create <| Guid.NewGuid().ToString("N")
            Game (gameId, admin)


        let extract (Game (id, admin)) = id, admin


        let (|GetGameAdmin|_|) game =
            let (_, admin) = extract game
            Some admin

        let (|GetGameId|_|) game =
            let (id, _) = extract game
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
        | Start
        | InGame of InGameModel


    type Msg =
        | CreateGame    of admin:Player
        | EndGame       
        | JoinGame      of player:Player
        | LeaveGame     of player:Player
        | StartRound    
        | FinishRound   
        | PlayCard      of card:Card



module Route =
    let builder = sprintf "/api/%s/%s"


module Api =

    open Domain


    type IPokerApi =
        { 
            getState    : GameId -> Result<GameModel,string> Async
            createGame  : Player -> Result<GameModel,string> Async 
            endGame     : GameId -> Player -> Result<GameModel,string> Async 
            joinGame    : GameId -> Player -> Result<GameModel,string> Async 
            leaveGame   : GameId -> Player -> Player -> Result<GameModel,string> Async 
            startRound  : GameId -> Player -> Result<GameModel,string> Async 
            finishRound : GameId -> Player -> Result<GameModel,string> Async 
            playCard    : GameId -> Player -> Card -> Result<GameModel,string> Async 
        }
    