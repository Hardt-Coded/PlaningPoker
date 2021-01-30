module Elements

open Styling
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Fable.MaterialUI.MaterialDesignIcons
open Models

let toolbar state dispatch =
    let c = useStyles ()

    Mui.toolbar [ 
        Mui.typography [ 
            typography.variant.h6
            typography.color.inherit'
            typography.children "Feliz.MaterialUI"
            typography.classes.root c.appBarTitle 
        ]

        // Light/dark mode button
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
