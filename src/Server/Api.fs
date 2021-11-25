module Api

open Domain
open Shared.Domain
open Shared.Api
open Saturn.Channels
open FSharp.Control.Tasks
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.SignalRService
open Giraffe
open Newtonsoft.Json


let private forwardResultViaSignalR (signalRMessages:IAsyncCollector<SignalRMessage>) (result:Result<GameModel,string> Async) =
    async {
        let! gameState = result;
        match gameState with
        | Result.Ok gameModel ->
            match gameModel with
            | GameModel.GotGameId gameId ->
                let id = gameId |> GameId.extract
                let json = Thoth.Json.Net.Encode.Auto.toString(4,{| GameId = gameId; GameModel = gameModel |} )
                do! signalRMessages.AddAsync(SignalRMessage(UserId = id, Target="newState",Arguments=[| json |])) |> Async.AwaitTask
                ()
            | _ ->
                ()
        | _ ->
            ()

        return! result;
    }
    

let pokerApi (gameEngine:GameEngine) (signalRMessages:IAsyncCollector<SignalRMessage>) : IPokerApi = {
    getState    = fun id -> 
        async { return! gameEngine.GetState id }
    createGame  = fun admin -> 
        async { return! gameEngine.CreateGame admin |> forwardResultViaSignalR signalRMessages }
    endGame     = fun id player -> 
        async { return! gameEngine.EndGame id player |> forwardResultViaSignalR signalRMessages }
    joinGame    = fun id player -> 
        async { return! gameEngine.JoinGame id player |> forwardResultViaSignalR signalRMessages }
    leaveGame   = fun id currentPlayer playerToLeave -> 
        async { return! gameEngine.LeaveGame id currentPlayer playerToLeave |> forwardResultViaSignalR signalRMessages }
    startRound  = fun id player -> 
        async { return! gameEngine.StartRound id player |> forwardResultViaSignalR signalRMessages }
    finishRound = fun id player -> 
        async { return! gameEngine.FinishRound id player |> forwardResultViaSignalR signalRMessages }
    playCard    = fun id player cardValue -> 
        async { return! gameEngine.PlayCard id player cardValue |> forwardResultViaSignalR signalRMessages }
        
}
