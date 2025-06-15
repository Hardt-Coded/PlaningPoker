[<RequireQualifiedAccess>]
module Domain

    open System.Threading.Tasks
    open Shared.Domain


    type GameState = GameId * GameModel



    let rec processMessage currentPlayer msg state =
        match currentPlayer, state, msg with
        // ############# START ####################
        | _, Init, CreateGame admin ->
            createGame admin

        | Some currentPlayer, InGame state, EndGame ->
            let id, admin = state.Game |> Game.extract
            if (admin = currentPlayer) then
                GameEnded id |> Ok
            else
                $"only the admin can end a game!" |> Error

        | _, InGame state, JoinGame player ->
            state |> joinGame player

        | Some currentPlayer, InGame state, LeaveGame player ->
            let _, admin = state.Game |> Game.extract
            if currentPlayer = admin || currentPlayer = player then
                state |> leaveGame player

            else
                $"Only the player itself or the admin can remove someone from the game!" |> Error

        | Some currentPlayer, InGame state, StartRound ->
            let _, admin = state.Game |> Game.extract
            if (currentPlayer = admin) then
                // if we have a pause card or a observer card played, we take it to the next round
                let playedCards =
                    state.PlayedCards
                    |> List.filter (fun pc -> match pc.Card |> Card.extract with | Coffee | Observer -> true | _ -> false)

                InGame { state with State = InRound; PlayedCards = playedCards } |> Ok
            else
                $"Only the admin can start a round!" |> Error


        | Some currentPlayer, InGame state, FinishRound ->
            let _, admin = state.Game |> Game.extract
            if (currentPlayer = admin) then
                InGame { state with State = DisplayResult } |> Ok
            else
                $"Only the admin can finish a round before its over!" |> Error

        | Some currentPlayer, InGame state , PlayCard card when state.State = InRound ->
            state |> playCard currentPlayer card

        | Some _, _ , PlayCard _ ->
            state |> Ok

        | cp, state, msg ->
            $"The Message '{msg}' from the player '{cp}' is not allowed in the current Game state '{state}'!" |> Error


    and private createGame admin =
        let newGame = Game.create admin
        let inGameState = {
            Game = newGame
            State = Beginning
            Players = [ admin ]
            PlayedCards = []
        }
        InGame inGameState |> Ok


    and private joinGame player state =
        let alreadyJoin = state.Players |> List.exists (fun p -> player = p)

        // maybe here an error message?
        let players =
            if alreadyJoin then
                state.Players
            else
                player::state.Players

        InGame { state with Players = players } |> Ok

    and private leaveGame player state =
        // maybe error or leave it
        let newState = {
            state with
                Players = state.Players |> List.filter (fun p -> p <> player)
        }
        // When all players left the game, we end it
        let gameId, admin = Game.extract newState.Game
        match newState.Players, (admin = player) with
        | [], _ ->
            GameEnded gameId |> Ok
        | _, true ->
            // game ended, when admin left the game
            GameEnded gameId |> Ok
        | _, false ->
            InGame newState |> Ok



    and private playCard player card state =
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



