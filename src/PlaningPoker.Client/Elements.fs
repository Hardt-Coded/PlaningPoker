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
        prop.style [
            style.marginRight 150
        ]
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


        // Theme Selection
        Mui.typography [
            typography.variant.h6
            prop.style [
                style.marginRight 20
            ]
            typography.children $"Card Theme:"
        ]

        Mui.button [
            button.color.primary
            button.variant.outlined
            prop.onClick (fun _ -> dispatch <| SwitchCardTheme Monster)
            prop.text "Monster"
            prop.style [
                style.marginRight 5
            ]
        ]

        Mui.button [
            button.color.primary
            button.variant.outlined
            prop.onClick (fun _ -> dispatch <| SwitchCardTheme Scifi)
            prop.text "Scifi"
            prop.style [
                style.marginRight 5
            ]
        ]

        Mui.button [
            button.color.primary
            button.variant.outlined
            prop.onClick (fun _ -> dispatch <| SwitchCardTheme Unicorn)
            prop.text "Unicorn"
            prop.style [
                style.marginRight 5
            ]
        ]


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


let getImageFromCard theme input =
    let themePrefix =
        match theme with
        | Monster -> "monster"
        | Scifi -> "scifi"
        | Unicorn -> "unicorn"

    let frontSide =
        match input with
        | One       -> $"./images/{themePrefix}_card_1.png"
        | Two       -> $"./images/{themePrefix}_card_2.png"
        | Three     -> $"./images/{themePrefix}_card_3.png"
        | Five      -> $"./images/{themePrefix}_card_5.png"
        | Eight     -> $"./images/{themePrefix}_card_8.png"
        | Thirteen  -> $"./images/{themePrefix}_card_13.png"
        | Twenty    -> $"./images/{themePrefix}_card_20.png"
        | Forty     -> $"./images/{themePrefix}_card_40.png"
        | Coffee    -> $"./images/{themePrefix}_card_pause.png"
        | Stop      -> $"./images/{themePrefix}_card_stop.png"
        | Wtf       -> $"./images/{themePrefix}_card_wtf.png"
        | TrustMeBro-> $"./images/{themePrefix}_card_trustmebro.png"
        | Observer  -> $"./images/{themePrefix}_card_observer.png"

    frontSide, $"./images/{themePrefix}_card_backside.png"

let card isOpen theme (onClick:unit->unit) (input:CardValue) =
    let (pic, backside) = getImageFromCard theme input

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
                    prop.src backside
                    prop.style [
                    ]
                ]
            ]
        ]






