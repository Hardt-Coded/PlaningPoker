module Main

open Feliz
open Browser.Dom
    
ReactDOM.render(
    App.PlaningPoker(),
    document.getElementById "elmish-app"
)

