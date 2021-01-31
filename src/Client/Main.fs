module Main

open Feliz
open Browser.Dom
open Fable.Core.JsInterop

importAll "./styles/styling.scss"
    
ReactDOM.render(
    App.PlaningPoker(),
    document.getElementById "elmish-app"
)

