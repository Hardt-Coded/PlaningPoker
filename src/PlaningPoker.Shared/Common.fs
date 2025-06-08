module PlaningPoker2.Shared.API

open System

let (|ValidId|InvalidId|) (id:string) =
    match Guid.TryParse id with
    | true, guid when guid <> Guid.Empty -> ValidId guid
    | _ -> InvalidId ()
