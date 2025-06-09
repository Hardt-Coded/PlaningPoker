[<RequireQualifiedAccess>]
module Commands

open Fable.Remoting.Client
open Shared
open Shared.Api
open Shared.Domain
open Models
open SignalRHelper
open Fable.Core




[<Global>]
let pokerBaseUrl:string = jsNative

let pokerApi =
    Remoting.createApi()
    |> Remoting.withBaseUrl pokerBaseUrl
    |> Remoting.withRouteBuilder IPokerApi.EndpointBuilder
    |> Remoting.buildProxy<IPokerApi>


[<RequireQualifiedAccess>]
module SignalR =

    open Fable.SimpleHttp
    open Fable.SimpleJson

    type SignalRConnectionInfo = {
        url:string
        accessToken:string
    }

    let private getConnectionInfo (gameId:string) =
        async {
            let url = $"{pokerBaseUrl}/api/negotiate?username={gameId}&hubname={SignalR.hubName}"

            let! respo =
                Http.request url
                |> Http.method HttpMethod.GET
                |> Http.header (Header.Header ("x-ms-client-principal-id", gameId))
                |> Http.send

            match respo.statusCode with
            | 200 ->
                JS.console.log "signalr negotiated!"
                let content = Json.parseAs<SignalRConnectionInfo> respo.responseText
                return content
            | _ ->
                return failwith "error getting signalr service info"
        }


    open Fable.Core.JsInterop

    let private openSignalRConnection (info:SignalRConnectionInfo) (gameId:string) onNewState =
        async {

            JS.console.log "signalr init connection!"

            let option =
                { new IHttpConnectionOptions with
                    member this.accessTokenFactory() = failwith "todo"
                    member this.headers = [ ("x-ms-client-principal-id", gameId) ] |> dict

                }
            let connection =
                signalR.CreateHubConnectionBuilder()
                    //.withUrl(info.url, !!{| accessTokenFactory = (fun () -> info.accessToken) |})
                    .withUrl("http://localhost:7071/api", option)
                    .withAutomaticReconnect()
                    .build()


            connection.on("newState",(fun (data:obj) -> onNewState data))

            JS.console.log "signalr connecting!"
            do! connection.start() |> Async.AwaitPromise
            JS.console.log "signalr connected!"

            return connection
        }



    let connectSignalRCmd gameId =
        fun dispatch ->
            async {
                try
                    let gameId = gameId |> GameId.extract
                    let! info = getConnectionInfo gameId
                    let! connection = openSignalRConnection info gameId (fun data ->
                        JS.console.log("SignalR new state received")
                        JS.console.dir(data)
                        let payload = Json.parseAs<MessageFormat> (data |> string)
                        let incomingGameId = payload.GameId |> GameId.extract
                        if (gameId <> incomingGameId) then
                            ()
                        else
                            dispatch <| SetCurrentGameState (payload.GameModel, None)
                    )
                    dispatch <| SignalRConnected connection
                with
                | ex ->
                    dispatch (OnError ex.Message)
                    raise ex

            } |> Async.StartImmediate


    let disconnectSignalRCmd (connection:IHubConnection option) =
        fun dispatch ->
            async {
                try
                    match connection with
                    | None ->
                        ()
                    | Some connection ->
                        connection.off("newState")
                        do! connection.stop() |> Async.AwaitPromise
                        dispatch <| SignalRDisconnected
                with
                | ex ->
                    dispatch (OnError ex.Message)
                    raise ex

            } |> Async.StartImmediate


let private sendCommandWithSucceed f player  succeedDispatch =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            try

                let! state = f()
                JS.console.log("Command sent to server")
                JS.console.log(state)
                match state with
                | Ok state ->
                    dispatch <| SetCurrentGameState (state, player)
                    succeedDispatch dispatch
                | Error e -> dispatch <| OnError e
            with
            | ex ->
                dispatch <| OnError ex.Message

            dispatch <| IsLoading false
        } |> Async.StartImmediate


let private sendCommand f player =
    sendCommandWithSucceed f player ignore


let loadState gameId =
    sendCommand (fun () -> pokerApi.getGameState gameId) None


let sendMsgToServer gameId currentPlayer msg =
    sendCommand (fun () -> pokerApi.sendCommand gameId currentPlayer msg) currentPlayer


let resetWhenGameNotExists gameId =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            try
                let! state = pokerApi.getGameState gameId
                match state with
                | Ok _ ->
                    ()
                | Error e ->
                    Browser.Dom.console.log(e)
                    dispatch <| Reset
                    dispatch <| Navigate []
            with
            | ex ->
                dispatch <| OnError ex.Message

            dispatch <| IsLoading false
        } |> Async.StartImmediate


let joinGameFromCookiesOrCheckGameExists () =
    fun dispatch ->
        async {
            dispatch <| IsLoading true
            match Cookies.getCurrentPlayer(), Cookies.getGameId() with
            | Some currentPlayer, Some gameId ->

                try
                    let! state = pokerApi.sendCommand gameId (Some currentPlayer) (GameMsg.JoinGame currentPlayer)
                    match state with
                    | Ok state ->
                        match state with
                        | GameModel.GotGameId (gameId, _) ->
                            dispatch <| GameJoined (currentPlayer, gameId, state)
                        | _ ->
                            dispatch <| OnError "Game Server doesn't switch to the right Game State. Please try again!"
                    | Error e ->
                        Browser.Dom.console.log(e)
                        dispatch <| Reset
                with
                | ex ->
                    dispatch <| OnError ex.Message

            | _ ->
                ()
            dispatch <| IsLoading false
        } |> Async.StartImmediate

let resetSelectedCardFlag (timeout:int) =
    fun dispatch ->
        async {
            do! Async.Sleep timeout
            dispatch <| ResetCardRecentlySelectedFlag
        } |> Async.StartImmediate


let setCookies gameId player =
    fun _ ->
        Cookies.setCurrentPlayer player
        Cookies.setGameId gameId

let removeCookies () =
    fun _ ->
        Cookies.removeAllCookies ()






[<RequireQualifiedAccess>]
module WebSocket =


    type ChannelMessage = { Topic: string; Payload: string }
