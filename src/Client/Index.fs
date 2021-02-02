module Index

open Elmish
open Fable.Remoting.Client
open Shared.Api
open Shared
open Shared.Domain
open Models
open Feliz.MaterialUI
open Feliz.Router
open Fable.SimpleJson
        


let init isDarkMode =
    let currentUrl = Router.currentUrl ()
    let id =
        match currentUrl with
        | [] -> ""
        | _ -> currentUrl.[0]

    let state = {
        CurrentGameState = Start
        GameId = None
        CurrentPlayer = None
        Name = ""
        Error = ""
        Theme = if isDarkMode then Dark else Light
        IsLoading = false
        Id = id
        WebSocket = None
    }
    
    let gameId = Cookies.getGameId ()

    let cmd =
        if id <> "" && gameId <> Some (GameId.create id) then
            Commands.resetWhenGameNotExists id
        else
            Commands.joinGameFromCookiesOrCheckGameExisits ()

    state, cmd


let update (msg:Models.Msg) state =
    Browser.Dom.console.log ($"%A{msg}")
    match msg with
    | Reset ->
        Browser.Dom.console.log("reseting State")
        
        let currentTheme = state.Theme
        { 
            CurrentGameState = Start
            GameId = None
            CurrentPlayer = None
            Name = ""
            Error = ""
            IsLoading = false
            Id = ""
            Theme = currentTheme
            WebSocket = None
        }, Cmd.batch [ Commands.removeCookies (); Cmd.ofMsg (Navigate [ "" ])]
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


    | GameMsg (StartRound) ->
        let cmd =
            match state.GameId, state.CurrentPlayer with
            | Some gameId, Some player ->
                Commands.startRound gameId player
            | _ ->
                Cmd.none
        state, cmd

    | GameMsg (FinishRound) ->
        let cmd =
            match state.GameId, state.CurrentPlayer with
            | Some gameId, Some player ->
                Commands.finishRound gameId player
            | _ ->
                Cmd.none
        state, cmd
        
    | GameMsg (LeaveGame playerToLeave) ->
        let cmd =
            match state.GameId, state.CurrentPlayer with
            | Some gameId, Some player ->
                let cmds = [
                    Commands.leaveGame gameId player playerToLeave
                    if (player = playerToLeave) then
                        Cmd.ofMsg DisconnectWebSocket
                ]
                Cmd.batch cmds
            | _ ->
                Cmd.none
        state, cmd

    | GameMsg (EndGame) ->
        let cmd =
            match state.GameId, state.CurrentPlayer with
            | Some gameId, Some player ->
                Commands.endGame gameId player
            | _ ->
                Cmd.none
        state, cmd
        
    | GameMsg (PlayCard card) ->
        let cmd =
            match state.GameId, state.CurrentPlayer with
            | Some gameId, Some player ->
                Commands.playCard gameId player card
            | _ ->
                Cmd.none
        state, cmd
        
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
            | GameModel.GotGameId gameId -> Some gameId
            | _ -> None

        match gameModel, state.CurrentPlayer with
        | GameEnded gameId, _ -> // game was ended by admin
            { state with CurrentGameState = gameModel; Error = ""; GameId = Some gameId }, Cmd.ofMsg DisconnectWebSocket
        | GameModel.GotPlayers [], _ -> // no players left! go away
            { state with CurrentGameState = gameModel; Error = ""; GameId = gameId }, Cmd.ofMsg DisconnectWebSocket
        | GameModel.GotPlayers players, Some currentPlayer when players |> List.exists (fun p -> p = currentPlayer) |> not -> // if you arn't part of the game any more, go away!
            { state with CurrentGameState = gameModel; Error = ""; GameId = gameId }, Cmd.ofMsg DisconnectWebSocket
        | _ ->
            { state with CurrentGameState = gameModel; Error = ""; GameId = gameId }, Cmd.none

    | SetCurrentPlayer player ->
        { state with CurrentPlayer = Some player }, Cmd.none
    | ConnectToWebSocket gameId ->
        state, Commands.WebSocket.connectWebSocketCmd gameId
    | SetWebSocketHandler ws ->
        { state with WebSocket = Some ws },Cmd.none
    | DisconnectWebSocket ->
        let cmd = 
            match state.WebSocket with
            | None ->
                Cmd.none
            | Some ws ->
                Commands.WebSocket.disconnectWebsocket ws
        state,cmd
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
    | Navigate segments ->
        match segments with
        | [ id ] ->
            state, Cmd.ofSub (fun _ -> Router.navigate (segments,[]))
        | _ ->
            state, Cmd.none
    | SetCookies ->
        let cmd =
            match state.GameId, state.CurrentPlayer with
            | Some gameId, Some player ->
                Commands.setCookies gameId player
            | _ ->
                Cmd.none
        state, cmd

    | ReInit currentUrl ->
        
        let id =
            match currentUrl with
            | [] -> ""
            | _ -> currentUrl.[0]

        let newState = {
            CurrentGameState = Start
            GameId = None
            CurrentPlayer = None
            Name = ""
            Error = ""
            Theme = state.Theme
            IsLoading = false
            Id = id
            WebSocket = None
        }

        let cmd =
            if id <> "" then
                Commands.resetWhenGameNotExists id
            else
                Cmd.none

        if (id <> state.Id) then
            newState, cmd
        else
            state, Cmd.none
        


open Feliz
open Styling


open Fable.MaterialUI.Icons

let renderLoginForm (classes:CustomStyles) (title:string) (buttonText:string) (onTextChange: string -> unit) (onClick:unit -> unit) state  =
    Mui.container [
        container.component' "main"
        container.maxWidth.xs
        container.children [
          Mui.paper [
            paper.classes.root classes.loginPaper
            prop.children [
              if state.IsLoading then
                Mui.circularProgress [
                  circularProgress.color.secondary
                ]
              else
                Mui.avatar [
                  avatar.classes.root classes.loginAvatar
                  avatar.children [
                    lockOutlinedIcon []
                  ]
                ]
    
              Mui.typography [
                typography.component' "h1"
                typography.variant.h5
                typography.children title
              ]
              Html.form [
                prop.className classes.loginForm
                prop.children [
                  Mui.textField [
                    textField.value state.Name
                    textField.onChange onTextChange
                    textField.variant.outlined
                    textField.margin.normal
                    textField.required true
                    textField.fullWidth true
                    textField.id "name"
                    textField.label "Name"
                    textField.name "name"
                    textField.autoFocus true
                  ]

                  Mui.button [
                    prop.type'.submit
                    button.fullWidth true
                    button.variant.contained
                    button.color.primary
                    button.classes.root classes.loginSubmit
                    button.children buttonText
                    button.disabled state.IsLoading
                    prop.onClick (fun e ->
                      e.preventDefault ()
                      onClick()
                    )
                  ]
                  
                ]
              ]
            ]
          ]
        ]
      ]



let renderCreateGameView (classes:CustomStyles) state dispatch =
    Mui.grid [
        grid.container true
        grid.spacing._2
        grid.children [
            renderLoginForm 
                classes
                "Create Game"
                "Create Game!"
                (ChangeName>>dispatch)
                (fun () -> dispatch CreateGame)
                state
        ]
    ]


let renderJoinGameView (classes:CustomStyles) state dispatch =
    Mui.grid [
        grid.container true
        grid.spacing._2
        grid.children [
            renderLoginForm 
                classes
                "Join Game"
                "Join Game!"
                (ChangeName>>dispatch)
                (fun () -> dispatch JoinGame)
                state
        ]
    ]


let renderStartView (classes:CustomStyles) state dispatch =
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
                            paper.classes.root classes.centerPaper
                            paper.children [
                                Html.h1 "Welcome to the F# Planing Poker Game"
                            ]
                        ]
                    ]
                ]
            ]
        ]

        if (state.Id = "") then
            renderCreateGameView classes state dispatch
        else
            renderJoinGameView classes state dispatch


    ]
    



let renderAdminView (classes:CustomStyles) state inGameState dispatch =
    Html.div [
        match inGameState.State with
        | Beginning
        | DisplayResult ->
            // Start Round
            Mui.button [
                button.fullWidth true
                button.variant.contained
                button.color.primary
                button.classes.root classes.loginSubmit
                button.children "Start Round!"
                button.disabled state.IsLoading
                prop.onClick (fun e ->
                  e.preventDefault ()
                  dispatch (GameMsg StartRound)
                )
              ]
        | InRound ->
            // Finish Round
            Mui.button [
                button.fullWidth true
                button.variant.contained
                button.color.primary
                button.classes.root classes.loginSubmit
                button.children "Finish the Round! (before everyone has choosen!)"
                button.disabled state.IsLoading
                prop.onClick (fun e ->
                  dispatch (GameMsg FinishRound)
                )
            ]

        Mui.button [
            button.fullWidth true
            button.variant.contained
            button.color.secondary
            button.classes.root classes.loginSubmit
            button.children "End Game!!!"
            button.disabled state.IsLoading
            prop.onClick (fun e ->
              dispatch (GameMsg EndGame)
            )
        ]

    ]


let renderPlayerAdminView (classes:CustomStyles) state player dispatch =
    Html.div [
        Mui.button [
            button.fullWidth true
            button.variant.contained
            button.color.primary
            button.classes.root classes.loginSubmit
            button.children "Remove Player!"
            button.disabled state.IsLoading
            prop.onClick (fun e ->
              e.preventDefault ()
              dispatch (GameMsg <| LeaveGame player)
            )
          ]
    ]


let renderPlayerView (classes:CustomStyles) state player dispatch =
    Html.div [
        Mui.button [
            button.fullWidth true
            button.variant.contained
            button.color.secondary
            button.classes.root classes.loginSubmit
            button.children "Leave the Game!"
            button.disabled state.IsLoading
            prop.onClick (fun e ->
              dispatch (GameMsg <| LeaveGame player)
            )
        ]
    ]

let renderDeck state inGameState dispatch =
    let cards = [
        One
        Two
        Three
        Five
        Eight
        Thirtheen
        Twenty
        Fourty
        Hundred
        Stop
        Coffee
        IDontKnow
    ]
    Mui.grid [
        grid.container true
        grid.spacing._2
        grid.children [
            for c in cards do
                Mui.grid [
                    grid.item true
                    grid.xs._1
                    
                    grid.children [
                        match inGameState.State with
                        | InRound ->
                            let onClick = (fun () -> dispatch (GameMsg (PlayCard (Card.create c))))
                            Elements.card 1.3 true onClick c
                        | _ ->
                            let onClick = fun () -> ()
                            Elements.card 1.3 false onClick c
                    ]
                    
                        
                ]
        ]
    ]



    
let renderPlayerState classes state inGameState player currentPlayer isAdmin dispatch =
    let (_, pname) = Player.extract player
    Mui.grid [
        grid.item true
        grid.xs._2
        grid.children [
            Mui.card [
                card.classes.root classes.playerCard
                card.children [
                    Mui.typography [
                        typography.align.center
                        typography.variant.h4
                        prop.text pname
                    ]

                    // Show Card
                    match inGameState.PlayedCards |> List.tryFind (fun pc -> pc.Player = player) with
                    | Some playedCard ->
                        let isVisible = 
                            match inGameState.State with
                            | InRound
                            | Beginning -> player = currentPlayer // only own card
                            | DisplayResult -> true // all cards
                        let cardValue = Card.extract playedCard.Card
                        Elements.card 1.6 isVisible (fun() -> ()) cardValue
                    | None ->
                        Html.div [
                            prop.style [
                                style.height 149
                            ]
                        ]

                    // Show Player Admin Panel
                    if (isAdmin) then
                        renderPlayerAdminView classes state player dispatch
                ]
            ]    
        ]
            
    ]
    


let renderInGameView classes state inGameState currentPlayer dispatch =
    Mui.container [
        let (_,name) = Player.extract currentPlayer
        let (gameId, admin) = Game.extract inGameState.Game
        let isAdmin = admin = currentPlayer

        Elements.loadingSpinner state.IsLoading
        
        let row1 = 
            if inGameState.Players.Length <= 4 then
                inGameState.Players
            else
                inGameState.Players.[0..3]
        Mui.grid [
            grid.container true
            grid.spacing._1
            grid.children [

                Mui.grid [
                    grid.item true
                    grid.xs._2
                ]

                for player in row1 do
                    renderPlayerState classes state inGameState player currentPlayer isAdmin dispatch

                Mui.grid [
                    grid.item true
                    grid.xs._2
                ]
                
            ]
        ]
        
        let row2 = 
            if inGameState.Players.Length <= 6 then
                inGameState.Players.[4..inGameState.Players.Length-1]
            else
                inGameState.Players.[4..5]
        Mui.grid [
            grid.container true
            grid.spacing._1
            grid.children [
                if (row2.Length > 0) then 
                    renderPlayerState classes state inGameState row2.[0] currentPlayer isAdmin dispatch
                else 
                    Mui.grid [
                        grid.item true
                        grid.xs._2
                    ]

                Mui.grid [
                    grid.item true
                    grid.xs._8
                    grid.classes.root classes.playerCard
                    grid.children [
                        match inGameState.State with
                        | InRound ->
                            Mui.typography [ 
                                typography.variant.h4
                                typography.align.center
                                typography.children "Choose your Card!"
                            ]
                        | Beginning ->
                            Mui.typography [ 
                                typography.variant.h4
                                typography.align.center
                                typography.children "Prepare yourself, the Game is starting soon!"
                            ]
                        | DisplayResult ->
                            Mui.typography [ 
                                typography.variant.h4
                                typography.align.center
                                typography.children "Look, what we got here"
                            ]

                            Mui.table [
                                Mui.tableHead [
                                    Mui.tableCell "1"
                                    Mui.tableCell "2"
                                    Mui.tableCell "3"
                                    Mui.tableCell "5"
                                    Mui.tableCell "8"
                                    Mui.tableCell "13"
                                    Mui.tableCell "20"
                                    Mui.tableCell "40"
                                    Mui.tableCell "100"
                                    Mui.tableCell "Stop"
                                    Mui.tableCell "Coffee"
                                    Mui.tableCell "WTF?"
                                ]
                                Mui.tableBody [

                                    let counts = [
                                        state.CurrentGameState |> GameModel.countPlayeredCards One
                                        state.CurrentGameState |> GameModel.countPlayeredCards Two
                                        state.CurrentGameState |> GameModel.countPlayeredCards Three
                                        state.CurrentGameState |> GameModel.countPlayeredCards Five
                                        state.CurrentGameState |> GameModel.countPlayeredCards Eight
                                        state.CurrentGameState |> GameModel.countPlayeredCards Thirtheen
                                        state.CurrentGameState |> GameModel.countPlayeredCards Twenty
                                        state.CurrentGameState |> GameModel.countPlayeredCards Fourty
                                        state.CurrentGameState |> GameModel.countPlayeredCards Hundred
                                        state.CurrentGameState |> GameModel.countPlayeredCards Stop
                                        state.CurrentGameState |> GameModel.countPlayeredCards Coffee
                                        state.CurrentGameState |> GameModel.countPlayeredCards IDontKnow
                                    ]

                                    let maxCount = counts |> List.max


                                    Mui.tableRow [
                                        for count in counts do
                                            match count with
                                            | 0 ->
                                                Mui.tableCell $"-"
                                            | c when c = maxCount ->
                                                Mui.tableCell [
                                                    Mui.typography [
                                                        typography.variant.h5
                                                        typography.children $"{c}"
                                                        typography.color.secondary
                                                    ]
                                                ]
                                            | _ ->
                                                Mui.tableCell $"{count}"
                                                
                                                
                                            
                                    ]
                                ]
                            ]
                            
                                
                    ]
                        
                ]


                if (row2.Length > 1) then
                    renderPlayerState classes state inGameState row2.[1] currentPlayer isAdmin dispatch
                else
                    Mui.grid [
                        grid.item true
                        grid.xs._2
                    ]
            
                ]
            ]

        let row3 =
            if inGameState.Players.Length <= 10 then
                inGameState.Players.[6..inGameState.Players.Length - 1]
            else
                inGameState.Players.[6..9]
        Mui.grid [
            grid.container true
            grid.spacing._1
            grid.children [

                Mui.grid [
                    grid.item true
                    grid.xs._2
                ]

                for player in row3 do
                    renderPlayerState classes state inGameState player currentPlayer isAdmin dispatch

                Mui.grid [
                    grid.item true
                    grid.xs._2
                ]
                
            ]
        ]

        if (inGameState.Players.Length > 10) then
            let rest = 
                inGameState.Players.[11..]
                |> List.chunkBySize 6

            for chucks in rest do

                Mui.grid [
                    grid.container true
                    grid.spacing._1
                    grid.children [

                        for player in chucks do
                            renderPlayerState classes state inGameState player currentPlayer isAdmin dispatch
                    ]
                ]


        

        renderDeck state inGameState dispatch

        if not isAdmin then
            renderPlayerView classes state currentPlayer dispatch

        if isAdmin then
            renderAdminView classes state inGameState dispatch
    ]


let view state dispatch =
    Browser.Dom.console.log ($"%A{state}")
    let classes = useStyles ()
    React.router [
        router.onUrlChanged (ReInit >> dispatch)
        router.children [
            Mui.themeProvider [
                themeProvider.theme (match state.Theme with | Dark -> Theme.dark | Light -> Theme.light)
                themeProvider.children [
                    Mui.cssBaseline []
                    Html.div [
                        prop.className classes.root
                        prop.children [
                            Mui.cssBaseline []
                            Mui.appBar [
                                appBar.classes.root classes.appBar
                                appBar.position.fixed'
                                appBar.children [
                                    Elements.toolbar state dispatch
                                ]
                            ]
                            
                            Html.main [
                                prop.className classes.content
                                prop.children [
                                    Html.div [ prop.className classes.toolbar ]

                                    match state.CurrentGameState, state.CurrentPlayer with
                                    | Start, _ ->
                                        renderStartView classes state dispatch
                                    | InGame inGameState, Some currentPlayer ->
                                        renderInGameView classes state inGameState currentPlayer dispatch
                                    | GameEnded _, _ ->
                                        Mui.typography "Wait ... for the end ..."
                                    | _ ->
                                        Mui.typography "Somethings seems to not working here!"


                                    

                                    Dialog.AlertDialog (state.Error<>"") (fun () -> dispatch ClearError) "Error" state.Error

                            
                                ]
                            ]
                        ]
                    ]

                    
                ]
            ]
        ]
    ]
