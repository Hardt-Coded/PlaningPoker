module Domain

    open Shared.Domain

    let rec private update currentPlayer msg state =
        match currentPlayer, state, msg with
        // ############# START ####################
        | _, Start, CreateGame admin ->
            createGame admin

        | Some currentPlayer, InGame state, EndGame ->
            let (id, admin) = state.Game |> Game.extract
            if (admin = currentPlayer) then
                GameEnded id |> Ok
            else
                $"only the admin can end a game!" |> Error

        | None, InGame state, JoinGame player ->
            state |> joinGame player
        
        | Some currentPlayer, InGame state, LeaveGame player ->
            let (id, admin) = state.Game |> Game.extract
            if currentPlayer = admin || currentPlayer = player then
                state |> leaveGame player
                
            else
                $"Only the player itself or the admin can remove someone from the game!" |> Error

        | Some currentPlayer, InGame state, StartRound ->
            let (_, admin) = state.Game |> Game.extract
            if (currentPlayer = admin) then
                InGame { state with State = InRound; PlayedCards = [] } |> Ok
            else
                $"Only the admin can start a round!" |> Error


        | Some currentPlayer, InGame state, FinishRound ->
            let (_, admin) = state.Game |> Game.extract
            if (currentPlayer = admin) then
                InGame { state with State = DisplayResult } |> Ok
            else
                $"Only the admin can finish a round before its over!" |> Error
        | Some currentPlayer, InGame state , PlayCard card when state.State = InRound ->
            state |> playCard currentPlayer card
        | Some currentPlayer, InGame state , PlayCard _ ->
            "You can only play a card, when you are playing a round" |> Error

        | cp, state, msg ->
            $"The Message '{msg}' from the player '{cp}' is not allowed in the current Game state '{state}'!" |> Error


    and createGame admin =
        let newGame = Game.create admin
        let inGameState = {
            Game = newGame
            State = Beginning
            Players = [ admin ]
            PlayedCards = []
        }
        InGame inGameState |> Ok


    and joinGame player state =
        let alreadyJoin = state.Players |> List.exists (fun p -> player = p)
    
        // maybe here an error message?
        let players =
            if alreadyJoin then 
                state.Players 
            else 
                player::state.Players
    
        InGame { state with Players = players } |> Ok

    and leaveGame player state =
        // maybe error or leave it
        let newState = {
            state with
                Players = state.Players |> List.filter (fun p -> p <> player)
        }
        // When all players left the game, we end it
        let (gameId, admin) = Game.extract newState.Game
        match newState.Players, (admin = player) with
        | [], _ ->
            GameEnded gameId |> Ok
        | _, true ->
            // game ended, when admin left the game
            GameEnded gameId |> Ok
        | _, false ->
            InGame newState |> Ok
        


    and playCard player card state =
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


    type GameEngineMsg = 
        | GameEngineMsg of game:GameId option * player:Player option * msg:Msg * AsyncReplyChannel<Result<GameModel,string>>
        | GetState of game:GameId * AsyncReplyChannel<Result<GameModel,string>>


    type GameEngine() =
    

        let getState gameId states =
            states |> List.tryFind (fun (gid,_) -> gid = gameId)


        let mailBox = 
            MailboxProcessor<GameEngineMsg>.Start(
                fun inbox -> 
                    let rec loop gameStates =
                        async {
                            let! msg = inbox.Receive()
                            match msg with 
                            | GetState (gameId, replyChannel) ->
                                let state = gameStates |> getState gameId 
                                match state with
                                | Some (_,state) ->
                                    replyChannel.Reply <| Ok state
                                | None ->
                                    replyChannel.Reply <| Error "No gamestate found!"
                                return! loop gameStates

                            | GameEngineMsg (gameId, player, msg, replyChannel) ->

                                match gameId with
                                | None ->
                                    let alreadyAdminGameId = 
                                        gameStates 
                                        |> List.tryFind (fun (_, gs) -> 
                                            match gs, player with
                                            | GameModel.GotGameAdmin admin, Some player  ->
                                                admin = player
                                            | _ ->
                                                false
                                        )
                                    match alreadyAdminGameId with
                                    | Some (gi,_) ->
                                        let gameId = GameId.extract  gi
                                        replyChannel.Reply ($"You started already started a game with the id '{gameId}'" |> Error)
                                        return! loop gameStates
                                    | None ->
                                        let initState = Start
                                        let stateResult = update player msg initState
                                        replyChannel.Reply stateResult

                                        match stateResult with
                                        | Ok newState ->
                                            match newState with
                                            | GameModel.GotGameId gameId ->
                                                let newGameStates = (gameId, newState)::gameStates
                                                return! loop newGameStates

                                            | _ ->
                                                replyChannel.Reply (Error "No GameId found!")
                                                return! loop gameStates
                                        | Error _ ->
                                            return! loop gameStates

                                | Some gameId ->
                                    let currentGameState = gameStates |> List.tryFind (fun (g,_) -> g = gameId)
                                    match currentGameState with
                                    | None ->
                                        replyChannel.Reply <| Error "Game doesn't exisits"
                                        return! loop gameStates

                                    | Some (currentGame, currentGameState) ->
                                        let stateResult = update player msg currentGameState
                                        replyChannel.Reply stateResult

                                        // in case of endgame remove state
                                        match stateResult with
                                        | Ok (GameEnded _) ->
                                            let newGameStates = 
                                                gameStates 
                                                |> List.filter (fun (g,s) ->g <> currentGame)
                                            return! loop newGameStates

                                        | Ok newState ->
                                            let newGameStates = 
                                                gameStates 
                                                |> List.map (fun (g,s) ->
                                                    if g = currentGame then
                                                        (g, newState)
                                                    else
                                                        (g,s)
                                                )
                                            return! loop newGameStates
                                        | Error e ->
                                            return! loop gameStates
                                        
                                
                                        
                            }

                    loop []
        )


        let postMsg game player msg =
            mailBox.PostAndAsyncReply(fun reply -> GameEngineMsg (game, player, msg, reply))


        member __.CreateGame admin =
            postMsg None (Some admin) (CreateGame admin)

        member __.EndGame game currentPlayer =
            postMsg (Some game) (Some currentPlayer) (EndGame)

        member __.JoinGame game currentPlayer =
            postMsg (Some game) None (JoinGame currentPlayer)

        member __.LeaveGame game currentPlayer player =
            postMsg (Some game) (Some currentPlayer) (LeaveGame player)

        member __.StartRound game currentPlayer =
            postMsg (Some game) (Some currentPlayer) StartRound

        member __.FinishRound game currentPlayer =
            postMsg (Some game) (Some currentPlayer) FinishRound

        member __.PlayCard game currentPlayer card =
            postMsg (Some game) (Some currentPlayer) (PlayCard card)

        member __.GetState game =
            mailBox.PostAndAsyncReply(fun reply -> GetState (game, reply))