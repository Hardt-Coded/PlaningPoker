module Api

    open Domain
    open Shared.Domain
    open Shared.Api
    open Saturn.Channels
    open FSharp.Control.Tasks

    
    let sendMessage (hub:ISocketHub) channelPath topic payload =
        task {
            let message = Thoth.Json.Net.Encode.Auto.toString(0, payload)
            do! hub.SendMessageToClients channelPath topic message
        }


    let private forwardResultToWebSocket (hub:ISocketHub) channelPath (result:Result<GameModel,string> Async) =
        async {
            let! gameState = result;
            match gameState with
            | Result.Ok gameModel ->
                match gameModel with
                | GameModel.GotGameId gameId ->
                    let id = gameId |> GameId.extract
                    do! sendMessage hub channelPath id gameModel |>Async.AwaitTask
                | _ ->
                    ()
            | _ ->
                ()

            return! result;
        }
    

    let pokerApi (gameEngine:GameEngine) (socketHub:ISocketHub) (webSocketChannel:string) : IPokerApi = {
        getState    = fun id -> 
            async { return! gameEngine.GetState id }
        createGame  = fun admin -> 
            async { return! gameEngine.CreateGame admin |> forwardResultToWebSocket socketHub webSocketChannel }
        endGame     = fun id player -> 
            async { return! gameEngine.EndGame id player |> forwardResultToWebSocket socketHub webSocketChannel }
        joinGame    = fun id player -> 
            async { return! gameEngine.JoinGame id player |> forwardResultToWebSocket socketHub webSocketChannel }
        leaveGame   = fun id currentPlayer playerToLeave -> 
            async { return! gameEngine.LeaveGame id currentPlayer playerToLeave |> forwardResultToWebSocket socketHub webSocketChannel }
        startRound  = fun id player -> 
            async { return! gameEngine.StartRound id player |> forwardResultToWebSocket socketHub webSocketChannel }
        finishRound = fun id player -> 
            async { return! gameEngine.FinishRound id player |> forwardResultToWebSocket socketHub webSocketChannel }
        playCard    = fun id player cardValue -> 
            async { return! gameEngine.PlayCard id player cardValue |> forwardResultToWebSocket socketHub webSocketChannel }
        
    }
