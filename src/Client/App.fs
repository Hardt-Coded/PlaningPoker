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
            Index.init isDarkMode,
            Index.update,
            [| isDarkMode |> unbox |]

        )

    Index.view state dispatch
    //Html.h1 "Test"





