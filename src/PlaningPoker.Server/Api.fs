module Api

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.SignalR
open PlaningPoker2.Server
open Shared.Domain
open Shared.Api




let private forwardResultViaSignalR (hubClients:IHubClients) (gameState:Result<GameModel,string> ) =
    task {
        match gameState with
        | Result.Ok (GameModel.GotGameId (gameId, gameModel)) ->
            let id = gameId |> GameId.extract
            let json = Thoth.Json.Net.Encode.Auto.toString(4,{ GameId = gameId; GameModel = gameModel })
            do! hubClients.User(id).SendAsync("newState", json)
            do! hubClients.All.SendAsync("newState", json)
            do! hubClients.User(id).SendCoreAsync("newState", [| json |])
            do! hubClients.All.SendCoreAsync("newState",  [| json |])
            return gameState
        | _ ->
            return gameState

    }


let pokerApi (gameEngine:GameEngine) hubClients : IPokerApi = {
    getGameState    = fun id ->
        async { return! gameEngine.GetGameState id |> Async.AwaitTask }
    sendCommand     = fun id player msg ->
        async {
            return!
                gameEngine.ProcessGameMsg id player msg
                |> Task.bind (forwardResultViaSignalR hubClients)
                |> Async.AwaitTask
        }
}
