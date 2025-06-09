module View

open Feliz.MaterialUI.themeStatic.theme.palette
open Shared.Domain
open Models
open Feliz.MaterialUI
open Feliz.Router
open Feliz
open Styling


let renderLoginForm (classes:CustomStyles) (title:string) (buttonText:string) (onTextChange: string -> unit) (onClick:unit -> unit) isLoading (name:string)  =
    let paper =
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
                    Icons.lockIcon [
                      icon.fontSize.large
                    ]
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

    Mui.container [
        container.component' "main"
        container.maxWidth.xs
        prop.children [ paper ]

      ]



let renderCreateGameView (classes:CustomStyles) isLoading name dispatch =
    Mui.grid [
        grid.container
        grid.spacing 2
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
        grid.container
        grid.spacing 2
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
            grid.container
            grid.spacing 2
            grid.children [
                Mui.grid [
                    grid.item
                    grid.xs 12
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
        prop.className "admin-view"
        prop.children [
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
    ]


let renderPlayerAdminView isLoading player dispatch =
    Html.div [
        prop.children [
            Mui.button [
                prop.className "player-remove-button"
                button.fullWidth true
                button.variant.contained
                button.color.primary
                button.children "Remove Player!"
                button.disabled isLoading
                prop.onClick (fun e ->
                  e.preventDefault ()
                  dispatch (GameMsg <| LeaveGame player)
                )
            ]
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
        Thirteen
        Twenty
        Forty
        Stop
        Coffee
        Wtf
        TrustMeBro
        Observer
    ]

    Html.div [
        let blur = if inGameState.State <> InRound then "blur" else ""
        prop.className $"card-selection {blur}"
        prop.children [
            let half = List.length cards / 2

            let renderCard c =

                match inGameState.State with
                | InRound ->
                    let onClick = (fun () -> dispatch (GameMsg (PlayCard (Card.create c))))
                    Elements.card true onClick c
                | _ ->
                    let onClick = fun () -> ()
                    Elements.card false onClick c

            Html.div [
                prop.className "card-row"
                prop.children [
                    for c in cards |> List.truncate half do
                        renderCard c
                ]
            ]

            Html.div [
                prop.className "card-row"
                prop.children [
                    for c in cards |> List.skip half do
                        renderCard c
                ]
            ]

        ]
    ]




let renderPlayerState index playerCount isLoading inGameState player currentPlayer isAdmin dispatch =
    let _, pname = Player.extract player

    let renderPlayerCard () =
        let playerCard = inGameState.PlayedCards |> List.tryFind (fun pc -> pc.Player = player)
        match playerCard with
        | Some playedCard ->
            let isVisible =
                match inGameState.State with
                | InRound
                | Beginning -> player = currentPlayer
                | DisplayResult -> true

            let visibleByCard =
                match Card.extract playedCard.Card with
                | Observer
                | Coffee -> true
                | _ -> isVisible

            let cardValue = Card.extract playedCard.Card
            Elements.card visibleByCard (fun() -> ()) cardValue
        | None ->
            Html.none



    Html.div [
        prop.className "player"
        prop.style [
            style.custom("--i", index)
            style.custom("--player-count", playerCount)
        ]
        prop.children [
            Html.div [
                prop.className "player-card"
                prop.children [
                    renderPlayerCard ()
                ]
            ]
            Html.div [ prop.className "player-name"; prop.text pname ]
            if isAdmin then
                renderPlayerAdminView isLoading player dispatch
        ]
    ]


let renderMessages inGameState =
    Mui.grid [
        grid.item
        grid.xs 8
        grid.children [
            match inGameState.State with
            | InRound ->
                Mui.typography [
                    typography.variant.h4
                    typography.align.center
                    typography.children "Choose your card!"
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
                        Mui.tableRow [
                            Mui.tableCell "1"
                            Mui.tableCell "2"
                            Mui.tableCell "3"
                            Mui.tableCell "5"
                            Mui.tableCell "8"
                            Mui.tableCell "13"
                            Mui.tableCell "20"
                            Mui.tableCell "40"
                            Mui.tableCell "Stop"
                            Mui.tableCell "Coffee"
                            Mui.tableCell "WTF?"
                            Mui.tableCell "Trust Me Bro!"
                            Mui.tableCell "Observer"
                        ]
                    ]
                    Mui.tableBody [

                        let counts =
                            [
                                One
                                Two
                                Three
                                Five
                                Eight
                                Thirteen
                                Twenty
                                Forty
                                Stop
                                Coffee
                                Wtf
                                TrustMeBro
                                Observer
                            ]
                            |> List.map (fun v -> inGameState |> GameModel.countPlayedCards v)

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
                                            typography.color.success
                                        ]
                                    ]
                                | _ ->
                                    Mui.tableCell $"{count}"
                        ]
                    ]
                ]
        ]
    ]

let renderInGameView classes isLoading currentPlayer gameId inGameState baseState dispatch =
    let _,name = Player.extract currentPlayer
    let gameId, admin = Game.extract inGameState.Game
    let isAdmin = admin = currentPlayer
    Html.div [
        prop.className "game-container"
        prop.children [

            Html.div [
                prop.className "player-circle"
                prop.children [
                    let count = inGameState.Players |> List.length
                    for i, player in inGameState.Players |> List.indexed do
                        renderPlayerState i count isLoading inGameState player currentPlayer isAdmin dispatch
                ]
            ]


            Html.div [
                prop.className (if inGameState.State = InRound then "message-box-above" else "message-box")
                prop.children [
                    renderMessages inGameState
                ]
            ]



            if baseState.CardRecentlySelected then
                Html.div [
                    prop.className "overlay-fade"
                    prop.children [
                        inGameState.PlayedCards
                        |> List.tryFind (fun pc -> pc.Player = currentPlayer)
                        |> Option.map (fun pc -> Card.extract pc.Card)
                        |> Option.map (fun card ->
                            Html.div [
                                prop.className "selected-card"
                                prop.children [
                                    Html.img [
                                        prop.src (Elements.getImageFromCard card)
                                    ]
                                ]
                            ]
                        )
                        |> Option.defaultValue Html.none

                    ]
                ]


            renderDeck inGameState dispatch

            if isAdmin then
                renderAdminView classes isLoading inGameState dispatch



        ]
    ]


let view state dispatch =
    let theme = Styles.useTheme ()
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
                                        renderInGameView classes state.IsLoading player gameId inGameState state dispatch
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
