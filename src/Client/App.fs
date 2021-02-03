module App

open Feliz
open Feliz.UseElmish
open Feliz.MaterialUI



[<ReactComponent>]
let PlaningPoker () =
    //let isDarkMode = Hooks.useMediaQuery "@media (prefers-color-scheme: dark)"
    let isDarkMode = true
    let state, dispatch = 
        React.useElmish(
            Update.init isDarkMode,
            Update.update,
            [| isDarkMode |> unbox |]

        )

    View.view state dispatch
    





