module View

open Shared.Domain
open Models
open Feliz.MaterialUI
open Feliz.Router
open Feliz
open Styling


open Fable.MaterialUI.Icons
open Feliz.MaterialUI

let renderLoginForm (classes:CustomStyles) (title:string) (buttonText:string) (onTextChange: string -> unit) (onClick:unit -> unit) isLoading (name:string)  =
    Mui.container [
        container.component' "main"
        container.maxWidth.xs
        container.children [
          Mui.paper [
            paper.classes.root classes.loginPaper
            prop.children [
              if isLoading then
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
                    textField.value name
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
                    button.disabled isLoading
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



let renderCreateGameView (classes:CustomStyles) isLoading name dispatch =
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
                isLoading
                name
        ]
    ]


let renderJoinGameView (classes:CustomStyles) isLoading name dispatch =
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
                isLoading
                name
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
                                Mui.typography [
                                    typography.children "Welcome to the F#ncy Planing Poker Game"
                                    typography.align.center
                                    typography.variant.h3
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

        match state.View with
        | CreateGameView viewState ->
            renderCreateGameView classes state.IsLoading viewState.Name dispatch
        | JoinGameView viewState ->
            renderJoinGameView classes state.IsLoading viewState.Name dispatch
        | _ ->
            ()


    ]
    



let renderAdminView (classes:CustomStyles) isLoading inGameState dispatch =
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
                button.disabled isLoading
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
                button.disabled isLoading
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
            button.disabled isLoading
            prop.onClick (fun e ->
              dispatch (GameMsg EndGame)
            )
        ]

    ]


let renderPlayerAdminView (classes:CustomStyles) isLoading player dispatch =
    Html.div [
        Mui.button [
            button.fullWidth true
            button.variant.contained
            button.color.primary
            button.classes.root classes.loginSubmit
            button.children "Remove Player!"
            button.disabled isLoading
            prop.onClick (fun e ->
              e.preventDefault ()
              dispatch (GameMsg <| LeaveGame player)
            )
          ]
    ]


let renderPlayerView (classes:CustomStyles) isLoading player dispatch =
    Html.div [
        Mui.button [
            button.fullWidth true
            button.variant.contained
            button.color.secondary
            button.classes.root classes.loginSubmit
            button.children "Leave the Game!"
            button.disabled isLoading
            prop.onClick (fun e ->
              dispatch (GameMsg <| LeaveGame player)
            )
        ]
    ]

let renderDeck inGameState dispatch =
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



    
let renderPlayerState classes isLoading inGameState player currentPlayer isAdmin dispatch =
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
                        renderPlayerAdminView classes isLoading player dispatch
                ]
            ]    
        ]
            
    ]
    


let renderInGameView classes isLoading currentPlayer gameId inGameState dispatch =
    Mui.container [
        let (_,name) = Player.extract currentPlayer
        let (gameId, admin) = Game.extract inGameState.Game
        let isAdmin = admin = currentPlayer

        Elements.loadingSpinner isLoading
        
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
                    renderPlayerState classes isLoading inGameState player currentPlayer isAdmin dispatch

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
                    renderPlayerState classes isLoading inGameState row2.[0] currentPlayer isAdmin dispatch
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

                                    let counts = 
                                        [
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
                                        |> List.map (fun v -> inGameState |> GameModel.countPlayeredCards v)

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
                    renderPlayerState classes isLoading inGameState row2.[1] currentPlayer isAdmin dispatch
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
                    renderPlayerState classes isLoading inGameState player currentPlayer isAdmin dispatch

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
                            renderPlayerState classes isLoading inGameState player currentPlayer isAdmin dispatch
                    ]
                ]


        

        renderDeck inGameState dispatch

        if not isAdmin then
            renderPlayerView classes isLoading currentPlayer dispatch

        if isAdmin then
            renderAdminView classes isLoading inGameState dispatch
    ]


let view state dispatch =
    Browser.Dom.console.log ($"%A{state}")
    let classes = useStyles ()
    React.router [
        router.onUrlChanged (UrlChanged >> dispatch)
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
                                    match state.View with
                                    | JoinGameView _
                                    | CreateGameView _ ->
                                        Elements.toolbar state.Theme None None dispatch
                                    | InGameView { CurrentGameState = _; CurrentPlayer= player; GameId = gameId; } ->
                                        Elements.toolbar state.Theme (Some player) (Some gameId) dispatch
                                ]
                            ]
                            
                            Html.main [
                                prop.className classes.content
                                prop.children [
                                    Html.div [ prop.className classes.toolbar ]


                                    match state.View with
                                    | JoinGameView _
                                    | CreateGameView _ ->
                                        renderStartView classes state dispatch
                                    | InGameView { CurrentGameState = (InGame inGameState); CurrentPlayer= player; GameId = gameId; } ->
                                        renderInGameView classes state.IsLoading player gameId inGameState dispatch
                                    | _ ->
                                        Mui.typography "Wait ... for the end ..."

                                    Dialog.AlertDialog (state.Error<>"") (fun () -> dispatch ClearError) "Error" state.Error
                                        
                                    Dialog.AlertDialog (state.Message<>("","")) (fun () -> dispatch ClearMessage) (state.Message |> fst) (state.Message |> snd)

                            
                                ]
                            ]
                        ]
                    ]

                    
                ]
            ]
        ]
    ]
