module Index

open Elmish
open Fable.Remoting.Client
open Shared.Api
open Shared
open Shared.Domain
open Models
open Feliz.MaterialUI

   


module Commands =

    let pokerApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.buildProxy<IPokerApi>

    let loadState gameId =
        async {
            let! state = pokerApi.getState gameId
            match state with
            | Ok state -> return SetCurrentGameState state
            | Error e -> return OnError e
        }
        |> Cmd.OfAsync.result


let init () =
    let isDarkMode = Hooks.useMediaQuery "@media (prefers-color-scheme: dark)"
    {
        CurrentGameState = Start
        GameId = None
        Name = ""
        Error = ""
        Theme = if isDarkMode then Dark else Light
    }, Cmd.none



let update (msg:Models.Msg) state =
    match msg with
    | GameMsg _ ->
        state, Cmd.none
    | ChangeName name ->
        { state with Name = name }, Cmd.none
    | SetGameId gameId ->
        { state with GameId = Some gameId }, Cmd.none
    | LoadState ->
        match state.GameId with
        | None ->
            state, Cmd.none
        | Some gameId ->
            state, Commands.loadState gameId
    | SetCurrentGameState gameModel ->
        { state with CurrentGameState = gameModel; Error = "" }, Cmd.none
    | OnError error ->
        { state with Error = error }, Cmd.none
    | ToggleTheme ->
        let newTheme = 
            match state.Theme with
            | Dark -> Light
            | Light -> Dark
        { state with Theme = newTheme }, Cmd.none
        


open Feliz
open Feliz.MaterialUI
open Styling

let view state dispatch =
    let c = useStyles ()
    Mui.themeProvider [
        themeProvider.theme (match state.Theme with | Dark -> Theme.dark | Light -> Theme.light)
        themeProvider.children [
            Mui.textField [
                prop.className "myInput"
                textField.variant.filled
                textField.label "Input"
                textField.value state.Name
                textField.onChange (ChangeName >> dispatch)
                textField.helperText ["Current value: "; state.Name ]   
            ]
        ]
    ]
