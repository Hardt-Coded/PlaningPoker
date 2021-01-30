module Index

open Elmish
open Fable.Remoting.Client
open Shared.Api
open Shared
open Shared.Domain
open Models
open Feliz.MaterialUI
open Feliz.Router


   


module Commands =

    let pokerApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.buildProxy<IPokerApi>


    let private sendCommandWithSucceed f succeedDispatch =
        fun dispatch ->
            async {
                dispatch <| IsLoading true

                let! state = f()
                succeedDispatch dispatch
                do! Async.Sleep 2000
                match state with
                | Ok state -> 
                    Browser.Dom.console.log ($"%A{state}")
                    
                    dispatch <| SetCurrentGameState state
                | Error e -> dispatch <| OnError e

                dispatch <| IsLoading false
            } |> Async.StartImmediate
        |> Cmd.ofSub


    let private sendCommand f =
        sendCommandWithSucceed f (ignore)

    


    let loadState gameId =
        sendCommand (fun () -> pokerApi.getState gameId)
        
        


    let createGame player =
        sendCommandWithSucceed (fun () -> pokerApi.createGame player) (fun dispatch -> dispatch <| SetCurrentPlayer player)


    let joinGame gameId player =
        sendCommandWithSucceed (fun () -> pokerApi.joinGame gameId player) (fun dispatch -> dispatch <| SetCurrentPlayer player)
        


let init isDarkMode =
    let currentUrl = Router.currentUrl ()
    let id =
        match currentUrl with
        | [] -> ""
        | _ -> currentUrl.[0]
        

    Browser.Dom.console.log ($"%A{currentUrl}")
    let myState = InGame {
        Game = Game.create (Player.create "Test")
        State = Beginning
        Players = []
        PlayedCards = []
    }
    {
        //CurrentGameState = myState
        CurrentGameState = Start
        GameId = None
        CurrentPlayer = None
        Name = ""
        Error = ""
        Theme = if isDarkMode then Dark else Light
        IsLoading = false
        Id = id
    }, Cmd.none




let update (msg:Models.Msg) state =
    Browser.Dom.console.log ($"%A{msg}")
    match msg with
    | CreateGame ->
        if (state.Name = "") then
            state, Cmd.ofMsg <| OnError "Name is empty!"
        else
            let player = Player.create state.Name
            state, Commands.createGame player

    | JoinGame ->
        if (state.Id = "") then
            state, Cmd.ofMsg <| OnError "Id is empty!"
        elif (state.Name = "") then
            state, Cmd.ofMsg <| OnError "Name is empty!"
        else
            let player = Player.create state.Name
            let gameId = GameId.create state.Id
            state, Commands.joinGame gameId player

    | GameMsg _ ->
        state, Cmd.none
    | ChangeName name ->
        { state with Name = name }, Cmd.none
    | ChangeId id ->
        { state with Id = id }, Cmd.none
    | SetGameId gameId ->
        { state with GameId = Some gameId }, Cmd.none
    | LoadState ->
        match state.GameId with
        | None ->
            state, Cmd.none
        | Some gameId ->
            state, Commands.loadState gameId
    | SetCurrentGameState gameModel ->
        let gameId =
            match gameModel with
            | InGame { Game = Game.GetGameId gameId } ->
                Some gameId
            | _ ->
                None
        { state with CurrentGameState = gameModel; Error = ""; GameId = gameId }, Cmd.none
    | SetCurrentPlayer player ->
        { state with CurrentPlayer = Some player }, Cmd.none
    | OnError error ->
        { state with Error = error }, Cmd.none
    | ClearError ->
        { state with Error = "" }, Cmd.none
    | ToggleTheme ->
        let newTheme = 
            match state.Theme with
            | Dark -> Light
            | Light -> Dark
        { state with Theme = newTheme }, Cmd.none
    | IsLoading b ->
        { state with IsLoading = b }, Cmd.none
    | SetPath segments ->
        state, Cmd.none
        


open Feliz
open Styling


let renderCreateGameView (c:CustomStyles) state dispatch =
    Mui.grid [
        grid.container true
        grid.spacing._2
        grid.children [
            Mui.grid [ grid.item true; grid.xs._3 ]
        
            Mui.grid [
                grid.item true
                grid.xs._6
                grid.children [
                    Mui.paper [
                        paper.classes.root c.centerPaper
                        paper.children [
                            Mui.grid [
                                grid.container true
                                grid.children [
                                    Mui.grid [
                                        grid.item true
                                        grid.xs._12
                                        grid.children [
                                            Html.h2 "Create a new game!"
                                        ]
                                    ]
        
                                    Mui.grid [
                                        grid.item true
                                        grid.xs._12
                                        grid.children [
                                            Mui.textField [
                                                textField.variant.outlined
                                                textField.label "Enter your Name"
                                                textField.defaultValue state.Name
                                                textField.onChange (ChangeName >> dispatch)
        
                                            ]
                                        ]
                                    ]
        
                                    Mui.grid [
                                        grid.item true
                                        grid.xs._12
                                        grid.children [
                                            Mui.button [
                                                prop.onClick (fun _ -> dispatch CreateGame)
                                                prop.text "Create!"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                                        
                                        
                                        
                        ]
                    ]
                ]
            ]
        
            Mui.grid [ grid.item true; grid.xs._3 ]
        ]
    ]


let renderJoinGameView (c:CustomStyles) state dispatch =
    Mui.grid [
        grid.container true
        grid.spacing._2
        grid.children [
            Mui.grid [ grid.item true; grid.xs._3 ]
        
            Mui.grid [
                grid.item true
                grid.xs._6
                grid.children [
                    Mui.paper [
                        paper.classes.root c.centerPaper
                        paper.children [
                            Mui.grid [
                                grid.container true
                                grid.children [
                                    Mui.grid [
                                        grid.item true
                                        grid.xs._12
                                        grid.children [
                                            Html.h2 "Join game!"
                                        ]
                                    ]
        
                                    Mui.grid [
                                        grid.item true
                                        grid.xs._12
                                        grid.children [
                                            Mui.textField [
                                                textField.variant.outlined
                                                textField.label "Enter Id"
                                                textField.defaultValue state.Id
                                                textField.onChange (ChangeId >> dispatch)
        
                                            ]
                                        ]
                                    ]
        
                                    Mui.grid [
                                        grid.item true
                                        grid.xs._12
                                        grid.children [
                                            Mui.textField [
                                                textField.variant.outlined
                                                textField.label "Enter Your Name"
                                                textField.defaultValue state.Name
                                                textField.onChange (ChangeName >> dispatch)
        
                                            ]
                                        ]
                                    ]
        
                                    Mui.grid [
                                        grid.item true
                                        grid.xs._12
                                        grid.children [
                                            Mui.button [
                                                prop.onClick (fun _ -> dispatch JoinGame)
                                                prop.text "Join!"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                                        
                                        
                                        
                        ]
                    ]
                ]
            ]
        
            Mui.grid [ grid.item true; grid.xs._3 ]
        ]
    ]


let renderStartView (c:CustomStyles) state dispatch =
    Html.div [
        Mui.grid [
            grid.container true
            grid.spacing._2
            grid.children [
                Mui.grid [
                    grid.item true
                    grid.xs._12
                    grid.children [
                        Mui.paper [
                            paper.classes.root c.centerPaper
                            paper.children [
                                Html.h1 "Welcome to the F# Planing Poker Game"
                            ]
                        ]
                    ]
                ]
            ]
        ]

        if (state.Id = "") then
            renderCreateGameView c state dispatch
        else
            renderJoinGameView c state dispatch


    ]
    

           

open Styling
let renderInGameView c state inGameState dispatch =
    Html.div [
        match state.CurrentPlayer with
        | None ->
            ()
        | Some player ->
            let (_,name) = Player.extract player
            Html.h1 $"Welcome {name}, let's play a game!"
            
            Html.h1 $"{inGameState.State}"
            Html.h1 $"{inGameState.Game}"
            Html.h1 "Players"
            Mui.grid [
                grid.container true
                grid.spacing._2
                grid.children [
                    for p in inGameState.Players do
                        let (_, pname) = Player.extract p
                        Mui.grid [
                            grid.item true
                            grid.xs.auto
                            grid.children [
                                Mui.card [
                                    card.classes.root c.playerCard
                                    card.children [
                                        Mui.typography [
                                            typography.align.center
                                            typography.variant.h4
                                            prop.text pname
                                        ]
                                    ]
                                ]    
                            ]
                            
                        ]
                        
                    
                ]
            ]

    ]


let view state dispatch =
    Browser.Dom.console.log ($"%A{state}")
    let c = useStyles ()
    React.router [
        router.onUrlChanged (SetPath >> dispatch)
        router.children [
            Mui.themeProvider [
                themeProvider.theme (match state.Theme with | Dark -> Theme.dark | Light -> Theme.light)
                themeProvider.children [
                    Mui.cssBaseline []
                    Html.div [
                        prop.className c.root
                        prop.children [
                            Mui.cssBaseline []
                            Mui.appBar [
                                appBar.classes.root c.appBar
                                appBar.position.fixed'
                                appBar.children [
                                    Elements.toolbar state dispatch
                                ]
                            ]
                            
                            Html.main [
                                prop.className c.content
                                prop.children [
                                    Html.div [ prop.className c.toolbar ]

                                    match state.CurrentGameState with
                                    | Start ->
                                        renderStartView c state dispatch
                                    | InGame inGameState ->
                                        renderInGameView c state inGameState dispatch

                                    Dialog.AlertDialog (state.Error<>"") (fun () -> dispatch ClearError) "Error" state.Error

                            
                                ]
                            ]
                        ]
                    ]

                    Elements.loadingSpinner state.IsLoading
                ]
            ]
        ]
    ]
