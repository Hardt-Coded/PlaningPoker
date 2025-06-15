module DataAccess

    open System.Threading.Tasks
    open FSharp.Azure.Storage.Table

    type DbGame = {
        [<RowKey>][<PartitionKey>] GameId: string
        GameData: string
    }

    module DbGame =

        open FsToolkit.ErrorHandling
        open Shared.Domain
        open Thoth.Json.Net

        let toGameState dbGame : Domain.GameState option =
            let res =
                result {
                    let! gameModel = Decode.Auto.fromString<GameModel>(dbGame.GameData, CamelCase)
                    let! gameId = GameId.create dbGame.GameId
                    return (gameId, gameModel)
                }

            match res with
            | Error _ ->
                None
            |Ok gameState ->
                Some gameState


        let fromGameState (gameState:Domain.GameState) =
            let gameId, gameModel = gameState
            let id = GameId.extract gameId
            let json = Encode.Auto.toString(4, gameModel, CamelCase)
            {
                GameId = id
                GameData = json
            }



    open Microsoft.Azure.Cosmos.Table
    open Shared.Domain

    type GameRepo = {
        getGameState:       GameId      -> Task<Domain.GameState option>
        addGameState:       Domain.GameState   -> Task<unit>
        updateGameState:    Domain.GameState   -> Task<unit>
        deleteGameState:    GameId      -> Task<unit>
    }



    let private getGameState tableClient tableName gameId : Task<Domain.GameState option> =
        task {
            let id = GameId.extract gameId
            let fromGameTable query = fromTableAsync tableClient tableName query
            let! result =
                Query.all<DbGame>
                |> Query.where <@ fun _ s -> s.PartitionKey = id @>
                |> fromGameTable

            let gameState =
                result
                |> Seq.tryHead
                |> Option.map (fun (data,_) -> data)
                |> Option.bind DbGame.toGameState

            return gameState
        }

    let private addOrUpdateGameState tableClient tableName (gameState:Domain.GameState) =
        task {
            let gameDb = gameState |> DbGame.fromGameState
            let inGameTable game = inTableAsync tableClient tableName game
            let! _ = gameDb |> InsertOrReplace |> inGameTable
            ()
        }

    let private deleteGameState tableClient tableName (gameId:GameId) =
        task {
            let id = GameId.extract gameId

            let fromGameTable query = fromTableAsync tableClient tableName query

            let! result =
                Query.all<DbGame>
                |> Query.where <@ fun _ s -> s.PartitionKey = id @>
                |> fromGameTable

            match result |> Seq.tryHead with
            | None ->
                ()
            | Some (dbItem, meta) ->
                let inGameTable game = inTableAsync tableClient tableName game
                let! _ = (dbItem, meta.Etag) |> Delete |> inGameTable
                ()
        }




    let initGameRepository (connectionString: string) =
        let tableName = "PokerGames"
        let account =
            if connectionString.Contains("UseDevelopmentStorage") then
                CloudStorageAccount.DevelopmentStorageAccount
            else
                CloudStorageAccount.Parse connectionString
        let tableClient = account.CreateCloudTableClient()
        let table = tableClient.GetTableReference(tableName)
        table.CreateIfNotExists() |> ignore

        {
            getGameState = getGameState tableClient tableName
            addGameState = addOrUpdateGameState tableClient tableName
            updateGameState = addOrUpdateGameState tableClient tableName
            deleteGameState = deleteGameState tableClient tableName
        }

