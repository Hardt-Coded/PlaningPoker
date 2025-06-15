namespace PlaningPoker2.Server

open DataAccess
open Microsoft.Extensions.Logging
open Shared.Domain

type GameEngine(
        logger: ILogger,
        repo: GameRepo
    ) =
        let processGameMsg gameId player msg =
            task {
                let! currentGameState = repo.getGameState gameId
                match currentGameState, msg with
                | None, CreateGame admin ->
                    let stateResult = Domain.processMessage (Some admin) msg Init

                    match stateResult with
                    | Ok (GameModel.GotGameId (gameId, gameState)) ->
                        do! repo.addGameState (gameId, gameState)
                        return stateResult
                    | Ok _ ->
                        logger.LogError "CreateGame should return GameId!"
                        return Error "CreateGame should return GameId!"
                    | Error e ->
                        logger.LogError $"Error: Create NewGame - {e}"
                        return Error e

                | None, _ ->
                    return Error "No GameId found!"

                | Some (currentGame, currentGameState), _ ->
                    let stateResult = Domain.processMessage player msg currentGameState
                    match stateResult with
                    | Ok (GameEnded _) ->
                        do! repo.deleteGameState gameId
                        return stateResult

                    | Ok newState ->
                        do! repo.updateGameState (currentGame, newState)
                        return stateResult

                    | Error e ->
                        logger.LogError $"Error: %A{currentGame} - {e}"
                        return Error e
            }


        member _.GetGameState gameId  =
            task {
                match! repo.getGameState gameId with
                | None ->
                    logger.LogWarning $"No GameState found for GameId: {gameId}"
                    return Error "No GameState found!"
                | Some (_, gameState) ->
                    return Ok gameState
            }


        member _.ProcessGameMsg gameId player msg =
            processGameMsg gameId player msg


