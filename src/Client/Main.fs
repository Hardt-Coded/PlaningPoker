module Main

open Feliz
open Browser.Dom
open Fable.Core.JsInterop

//importAll "./styles/styling.scss"
ReactDOM.createRoot(document.getElementById "elmish-app").render(App.PlaningPoker())
//ReactDOM.render(
//    App.PlaningPoker(),
//    document.getElementById "elmish-app"
//)

