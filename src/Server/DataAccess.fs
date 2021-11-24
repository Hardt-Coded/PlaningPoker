module DataAccess

    open FSharp.Azure.Storage.Table

    type DbGame = {
        [<RowKey>][<PartitionKey>] GameId: string
        GameData: string
    }

    module DbGame =
        
        open Domain
        open Shared.Domain
        open Thoth.Json.Net

        let toGameState dbGame : GameState option =
            let res = 
                result {
                    let! gameModel = Thoth.Json.Net.Decode.Auto.fromString<GameModel>(dbGame.GameData, CamelCase)    
                    let! gameId = GameId.create dbGame.GameId
                    return (gameId, gameModel)
                }

            match res with
            | Error _ ->
                None
            |Ok gameState ->
                Some gameState


        let fromGameState (gameState:GameState) =
            let (gameId, gameModel) = gameState
            let id = GameId.extract gameId
            let json = Thoth.Json.Net.Encode.Auto.toString(4, gameModel, CamelCase)
            {
                GameId = id
                GameData = json
            }
    
         

    open Microsoft.Azure.Cosmos.Table
    open Domain
    open Shared.Domain

    type GameRepo = {
        getGameState:       GameId-> Async<GameState option>
        addGameState:       GameState -> Async<unit>
        updateGameState:    GameState -> Async<unit>
        deleteGameState:    GameId -> Async<unit>
    }
    
    

    let private getGameState tableClient tableName gameId : Async<GameState option> =
        async {
            let id = GameId.extract gameId
            let fromGameTable query = fromTableAsync tableClient tableName query
            let! result =
                Query.all<DbGame>
                |> Query.where <@ fun g s -> s.PartitionKey = id @>
                |> fromGameTable

            let gameState =
                result
                |> Seq.tryHead
                |> Option.map (fun (data,_) -> data)
                |> Option.bind (DbGame.toGameState)
            
            return gameState
        }

    let private addOrUpdateGameState tableClient tableName (gameState:GameState) =
        async {
            let gameDb = gameState |> DbGame.fromGameState
            let inGameTable game = inTableAsync tableClient tableName game
            let! result = gameDb |> InsertOrReplace |> inGameTable
            ()
        }

    let private deleteGameState tableClient tableName (gameId:GameId) =
        async {
            let id = GameId.extract gameId

            let fromGameTable query = fromTableAsync tableClient tableName query

            let! result =
                Query.all<DbGame>
                |> Query.where <@ fun g s -> s.PartitionKey = id @>
                |> fromGameTable

            match result |> Seq.tryHead with
            | None ->
                ()
            | Some (dbItem, meta) ->
                let inGameTable game = inTableAsync tableClient tableName game
                let! _ = (dbItem, meta.Etag) |> Delete |> inGameTable
                ()
        }

    


    let initGameRepository connectionString =
        let tableName = "PokerGames"
        let account = CloudStorageAccount.Parse connectionString //Or your connection string here
        let tableClient = account.CreateCloudTableClient()
        let table = tableClient.GetTableReference(tableName)
        table.CreateIfNotExists() |> ignore

        {
            getGameState = getGameState tableClient tableName
            addGameState = addOrUpdateGameState tableClient tableName
            updateGameState = addOrUpdateGameState tableClient tableName
            deleteGameState = deleteGameState tableClient tableName
        }

