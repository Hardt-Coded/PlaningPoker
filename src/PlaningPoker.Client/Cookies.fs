module Cookies

open Shared.Domain


module JsCookie =

    open System
    open Fable.Core
    open Fable.Core.JsInterop

    type private IJSCookie =
      abstract remove: name: string -> unit
      abstract get: name: string -> string option
      abstract set: name: string -> value: string -> options:obj -> unit // Returns null, must be represented with option

    let private imported = importDefault<IJSCookie> "js-cookie"

    let get = imported.get

    let set = imported.set

    let remove = imported.remove


let private playerFromStr (str:string) =
    let s = str.Split("|") |> Array.toList
    match s with
    | [ name; id ] ->
        (Player.build id name) |> Result.toOption
    | _ ->
        None

let private playerToStr (Player.ExtractPlayerInfo (id, name)) =
    $"{name}|{id}"

let [<Literal>] GameIdCookie = "current-game"
let [<Literal>] PlayerCookie = "current-player"

let getGameId () : GameId option =
    JsCookie.get GameIdCookie
    |> Option.bind (GameId.create >> Result.toOption)


let getCurrentPlayer () : Player option =
    JsCookie.get PlayerCookie
    |> Option.bind playerFromStr


let setGameId (GameId.GetGameId gameId) =
    JsCookie.set GameIdCookie gameId {| expires = 1|}


let setCurrentPlayer (player:Player) =
    JsCookie.set PlayerCookie (player |> playerToStr) {| expires = 1|}


let removeAllCookies () =
    JsCookie.remove GameIdCookie
    JsCookie.remove PlayerCookie




