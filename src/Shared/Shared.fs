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

        let extract (Card value) = value
    


    type Player = private Player of id:Guid * name:string

    module Player =
    
        let create name =
            Player (Guid.NewGuid(), name)

        let build (id:string) name =
            Player (Guid id, name)

        let extract (Player (id,name)) = id,name

        let (|ExtractPlayerInfo|) (Player (id,name)) = id,name


    type GameId = private GameId of string

    module GameId =

        let create id = GameId id

        let extract (GameId id) = id

        let (|GetGameId|) (GameId gameId) = gameId




    type Game = private Game of id:GameId * admin:Player

    module Game =

        let create admin =
            let gameId = GameId.create <| Guid.NewGuid().ToString("N")
            Game (gameId, admin)


        let extract (Game (id, admin)) = id, admin


        let internal (|GetGameAdmin|_|) game =
            let (_, admin) = extract game
            Some admin

        let internal (|GetGameId|_|) game =
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
        | InGame of InGameModel
        | GameEnded of GameId


    module GameModel =

        let (|GotGameAdmin|_|) gameModel = 
            match gameModel with
            | (InGame { Game = Game.GetGameAdmin admin }) ->
                Some admin
            | _ ->
                None


        let (|GotGameId|_|) gameModel = 
            match gameModel with
            | (InGame { Game = Game.GetGameId gameId }) ->
                Some gameId
            | GameEnded gameId ->
                Some gameId
            | _ ->
                None


        let (|GotPlayers|_|) gameModel =
            match gameModel with
            | (InGame { Players = players }) -> Some players
            | _ -> None


        let countPlayeredCards cardValue gameModel  =
            //match gameModel with
            //| InGame { PlayedCards = playedCard } ->
            gameModel.PlayedCards
            |> List.filter (fun pc -> (pc.Card |> Card.extract) = cardValue)
            |> List.length
            //| _ ->
            //    0





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

    
    