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

    {
        CurrentGameState = Start
        GameId = None
        CurrentPlayer = None
        Name = ""
        Error = ""
        Theme = if isDarkMode then Dark else Light
        IsLoading = false
        Id = id
    }, Commands.joinGameFromCookies ()


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
    | ConnectToWebSocket gameId ->
        state, Commands.WebSocket.connectWebSocketCmd gameId
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
                  //circularProgress.classes.root classes.progressBar
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
                    textField.autoComplete "name"
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
    



let renderAdminView (classes:CustomStyles) state dispatch =
    Html.div [
        
    ]


let joinLink inGameState =
    let (gameid, _) = Game.extract inGameState.Game
    let linkAddress = $"{Browser.Dom.window.location.origin}/#{GameId.extract gameid}"
    Mui.link [
        prop.href linkAddress
        link.variant "body2"
        link.children "Join Link"
        
    ]
    

let renderInGameView classes state inGameState dispatch =
    Html.div [
        match state.CurrentPlayer with
        | None  ->
            Mui.typography "Somethings seems to not working here!"
            ()
        | Some player ->
            let (_,name) = Player.extract player
            let (id, admin) = Game.extract inGameState.Game
            Html.h1 $"Welcome {name}, let's play a game!"

            joinLink inGameState
            
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
                                    card.classes.root classes.playerCard
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

            Mui.grid [
                grid.container true
                grid.spacing._2
                grid.children [
                    Mui.grid [
                        grid.item true
                        grid.xs._1
                        grid.children [
                            Elements.card false Zero
                        ]
                    ]
                ]
            ]


            
            
            Elements.card true Zero
            Elements.card true One
            Elements.card true Two
            Elements.card true Three
            Elements.card true Five
            Elements.card true Eight
            Elements.card true Thirtheen
            Elements.card true Twenty
            Elements.card true Fourty
            Elements.card true Hundred
            Elements.card true Stop
            Elements.card true Coffee
            Elements.card true IDontKnow

    ]


let view state dispatch =
    Browser.Dom.console.log ($"%A{state}")
    let classes = useStyles ()
    React.router [
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

                                    match state.CurrentGameState with
                                    | Start ->
                                        renderStartView classes state dispatch
                                    | InGame inGameState ->
                                        renderInGameView classes state inGameState dispatch


                                    

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
