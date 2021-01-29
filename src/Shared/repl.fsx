

(*


        CreateGame
            |
        JoinGame (Add new Player)
            |
            |
            |
   ----- GameRound --------------------------
   |                                        |
   | /->StartRound                          |<-- JoinGame (Add new Player)
   | |      |                               |
   | |  PlayCard <--| once for every player |<-- LeaveGame (By Admin or Player itself)
   | |      |_______/                       |
   | |      |                               |<-- EndGame
   | \--OpenAllCards                        |
   |----------------------------------------|


    States and possible Commands and State Transitions:

    - Start
        - CreateGame
            -> into State: InGame
    - InGame
        - EndGame
            -> Into State: Start
        - JoinGame (add player)
        - LeaveGame (remove player)
        - StartRound
            -> Into State: InRound

            InGame States:

            - InRound
                - EndGame
                    -> Into State: Start
                - JoinGame (add player)
                - LeaveGame (remove player)
                - FinishRound (show all cards)
                    -> Into State: Result
            - Result
                - StartRound
                    - Into State: InRound
                - EndGame
                    -> Into State: Start
                - JoinGame (add player)
                - LeaveGame (remove player)
                    

*)


open System



//module Helper =

//    open System.Security.Cryptography

//    let generateRandomCharacters length =
//        let rng = new RNGCryptoServiceProvider()
        


type Card = private Card of int

module Card =

    let create value =
        Card value
    


type Player = private User of id:Guid * name:string

module Player =
    
    let create name =
        User (Guid.NewGuid(), name)


type Game = private Game of id:string * admin:Player

module Game =

    let create admin =
        
        Game (Guid.NewGuid().ToString("N"), admin)
        


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
    | PlayCard      of player:Player * card:Card





let update msg state =
    match state, msg with
    // ############# START ####################
    | Start, CreateGame admin ->
        let newGame = Game.create admin
        let inGameState = {
            Game = newGame
            State = Beginning
            Players = [ admin ]
            PlayedCards = []
        }
        InGame inGameState |> Ok
    | Start, _ ->
        $"Invalid command '{msg}' in this state of the game 'start'" |> Error

    // ############# INGAME ####################
    | InGame state, CreateGame _ ->
        $"Invalid command '{msg}' in this state of the game 'start'" |> Error

    | InGame state, EndGame ->
        Start |> Ok

    | InGame state, JoinGame player ->
        let newState = {
            state with
                Players = player::state.Players
        }
        InGame newState |> Ok

    | InGame state, LeaveGame player ->
        let newState = {
            state with
                Players = state.Players |> List.filter (fun p -> p <> player)
        }
        InGame newState |> Ok

    | InGame state, StartRound ->
        InGame { state with State = InRound } |> Ok

    | InGame state, FinishRound ->
        InGame { state with State = DisplayResult } |> Ok

    | InGame state , PlayCard (player, card) when state.State = InRound ->

        let playedCard = {
            Player = player
            Card = card
        }

        let alreadyPlayed =
            state.PlayedCards |> List.exists (fun pc -> pc.Player = player)

        let playedCards = 
            if alreadyPlayed then
                state.PlayedCards |> List.map (fun c -> if c.Player = player then { c with Card = card} else c)
            else
                playedCard::state.PlayedCards

        // when all player set a card, then change to the displayResult State
        let inGameState =
            if playedCards.Length = state.Players.Length then
                DisplayResult
            else
                InRound

        let newState = {
            state with
                State = inGameState
                PlayedCards = playedCards
        }
        InGame newState |> Ok

    | InGame state , PlayCard _ ->
        "You can only play a card, when you are playing a round" |> Error








    
    

        
