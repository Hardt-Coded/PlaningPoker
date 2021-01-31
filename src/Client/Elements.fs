module Elements

open Styling
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Fable.MaterialUI.MaterialDesignIcons
open Models
open Shared.Domain

let toolbar state dispatch =
    let c = useStyles ()

    Mui.toolbar [ 
        Mui.typography [ 
            typography.variant.h6
            typography.color.inherit'
            typography.children "F# Planing Poker"
            typography.classes.root c.appBarTitle 
        ]

        

        //Light/dark mode button
        Mui.tooltip [ 
            tooltip.title (
                match state.Theme with
                | Light -> "Using light theme"
                | Dark -> "Using dark theme"
            )
            tooltip.children (
                Mui.iconButton [ 
                    prop.onClick (fun _ -> dispatch ToggleTheme)
                    iconButton.color.inherit'
                    iconButton.children [ 
                        match state.Theme with
                        | Light -> brightness7Icon []
                        | Dark -> brightness4Icon [] 
                    ] 
                ]
            ) 
        ]

        // GitHub button
        Mui.tooltip [ 
            tooltip.title "F# Planing Poker on GitHub"
            tooltip.children (
                Mui.iconButton [ 
                    prop.href "https://github.com/DieselMeister/PlaningPoker"
                    iconButton.component' "a"
                    iconButton.color.inherit'
                    iconButton.children (gitHubIcon []) 
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

let card isOpen (onClick:unit->unit) (value:CardValue) =
    let pic = 
        match rnd.Next(0,4) with
        | 0 -> "spades"
        | 1 -> "diams"
        | 2 -> "hearts"
        | 3 -> "clubs"
        | _ -> "spades"

    let rank = 
        match value with
        | Zero      -> "a"
        | Halve     -> "a"
        | One       -> "a"
        | Two       -> "2"
        | Three     -> "3"
        | Five      -> "5"
        | Eight     -> "8"
        | Thirtheen -> "3"
        | Twenty    -> "2"
        | Fourty    -> "4"
        | Hundred   -> "10"
        | Coffee    -> "a"
        | Stop      -> "a"
        | IDontKnow -> "a"

    let value = 
        match value with
        | Zero      -> "0"
        | Halve     -> "½"
        | One       -> "1"
        | Two       -> "2"
        | Three     -> "3"
        | Five      -> "5"
        | Eight     -> "8"
        | Thirtheen -> "13"
        | Twenty    -> "20"
        | Fourty    -> "40"
        | Hundred   -> "100"
        | Coffee    -> "☕"
        | Stop      -> "🛑"
        | IDontKnow -> "?"
        
    
    Html.div [
        prop.style [
            style.overflow.hidden
            style.display.flex
            style.alignItems.center
            style.justifyContent.center
            style.minHeight 140
            style.minWidth 110
            style.height (length.percent 100)
            style.width (length.percent 100)
            style.custom ("box-sizing","initial")
        ]
        prop.children [
            if isOpen then
                Html.div [
                    prop.className $"card rank-{rank} {pic}"
                    prop.onClick (fun _ -> onClick())
                    prop.style [
                        style.custom ("boxSizing","initial")
                        style.transform.scale 1.5
                    ]
                    prop.children [
                        Html.span [
                            prop.className "rank"
                            prop.text $"{value}"
                                
                        ]
                        Html.span [
                            prop.className "suit"
                            prop.dangerouslySetInnerHTML $"&{pic};"
                                
                        ]
                    ]
                ]
            else
                Html.div [
                    prop.className "playingCards card back"
                    prop.style [
                        style.custom ("boxSizing","initial")
                        style.transform.scale 1.5
                    ]
                ]
        ]
    ]
    
    




