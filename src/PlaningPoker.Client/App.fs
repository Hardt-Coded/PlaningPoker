module Main

open Feliz
open Browser.Dom
let dingsbums = document.getElementById "hits-app"
console.log "Hello Planing Poker!"
console.log dingsbums
ReactDOM.createRoot(document.getElementById "hits-app").render(App.PlaningPoker())

