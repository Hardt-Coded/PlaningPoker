module Elements

open Styling
open Feliz
open Feliz.MaterialUI
open Models
open Shared.Domain

let joinLink (gameid:GameId) =
    let linkAddress = $"{Browser.Dom.window.location.origin}/#{GameId.extract gameid}"
    Mui.link [
        prop.href linkAddress
        link.variant "h6"
        link.children "Join Link"

    ]


let toolbar theme currentPlayer (gameId:GameId option) dispatch =
    let c = useStyles ()
    let welcomeText =
        match currentPlayer with
        | None ->
            ""
        | Some player ->
            let _,name = Player.extract player
            $" - Welcome {name}!"

    Mui.toolbar [
        Mui.typography [
            typography.variant.h6
            prop.style [
                //style.color (co)
            ]
            //typography.color.inherit'
            typography.children $"F#ncy Planing Poker{welcomeText}"
            typography.classes.root c.appBarTitle
        ]

        match gameId with
        | None -> ()
        | Some gameId ->
            joinLink gameId





        //Light/dark mode button
        Mui.tooltip [
            tooltip.title (
                match theme with
                | Light -> "Using light theme"
                | Dark -> "Using dark theme"
            )
            tooltip.children (
                Mui.iconButton [
                    prop.onClick (fun _ -> dispatch ToggleTheme)
                    iconButton.color.inherit'
                    iconButton.children [
                        match theme with
                        | Light -> Icons.brightness7Icon []
                        | Dark -> Icons.brightness4Icon []
                    ]
                ]
            )
        ]

        // GitHub button
        Mui.tooltip [
            tooltip.title "F#ncy Planing Poker on GitHub"
            tooltip.children (
                Mui.iconButton [
                    prop.href "https://github.com/DieselMeister/PlaningPoker"
                    iconButton.component' "a"
                    iconButton.color.inherit'
                    iconButton.children (Icons.gitHubIcon [])
                ]
            )
        ]
    ]


let loadingSpinner isVisible =
    Mui.backdrop [
        backdrop.open' isVisible
        backdrop.in' isVisible
        backdrop.children [
            Mui.circularProgress [
                circularProgress.color.inherit'
            ]
        ]

    ]

let private rnd = System.Random(System.Guid.NewGuid().GetHashCode())


let getImageFromCard input =
    match input with
    | One       -> "./card1_transparent_small.png"
    | Two       -> "./card2_transparent_small.png"
    | Three     -> "./card3_transparent_small.png"
    | Five      -> "./card5_transparent_small.png"
    | Eight     -> "./card8_transparent_small.png"
    | Thirteen  -> "./card13_transparent_small.png"
    | Twenty    -> "./card20_transparent_small.png"
    | Forty    ->  "./card40_transparent_small.png"
    | Coffee    -> "./pause_transparent_small.png"
    | Stop      -> "./stop_transparent_small.png"
    | Wtf       -> "./wtf_transparent_small.png"
    | TrustMeBro ->"./trustmebro_transparent_small.png"
    | Observer ->  "./observer_transparent_small.png"

let card isOpen (onClick:unit->unit) (input:CardValue) =
    let pic = getImageFromCard input

    if isOpen then
        Html.div [
            prop.className $"card"
            prop.onClick (fun _ -> onClick())
            prop.children [
                    Html.img [
                        prop.src pic
                        prop.style [
                            style.height (length.percent 100)
                        ]
                    ]
            ]
        ]
    else
        Html.div [
            prop.className $"card"
            prop.onClick (fun _ -> onClick())
            prop.children [
                Html.img [
                    prop.src "./card_back_transparent_small.png"
                    prop.style [
                    ]
                ]
            ]
        ]






