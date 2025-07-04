open Fake
open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators

open BuildHelpers
open BuildTools

initializeContext()

let publishPath = Path.getFullName "publish"
let srcPath = Path.getFullName "src"
let clientSrcPath = srcPath </> "PlaningPoker.Client"
let serverSrcPath = srcPath </> "PlaningPoker.Server"
let appPublishPath = publishPath </> "app"

// Targets
let clean proj = [ proj </> "bin"; proj </> "obj" ] |> Shell.cleanDirs

Target.create "Clean" (fun _ ->
    serverSrcPath |> clean
    clientSrcPath |> clean
)

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    Tools.node "--version" clientSrcPath
    printfn "Yarn version:"
    Tools.yarn "--version" clientSrcPath
    Tools.yarn "install --frozen-lockfile" clientSrcPath
)

Target.create "Publish" (fun _ ->
    [ publishPath ] |> Shell.cleanDirs
    let publishArgs = sprintf "publish -c Release -o \"%s\"" appPublishPath
    Tools.dotnet publishArgs serverSrcPath
    [ appPublishPath </> "local.settings.json" ] |> File.deleteAll
    Tools.yarn "build" ""
)

Target.create "Run" (fun _ ->
    let server = async {
        Tools.dotnet "watch msbuild /t:RunFunctions" serverSrcPath
        Tools.func "host start --cors *" serverSrcPath
    }
    let client = async {
        Tools.yarn "start" ""
    }
    [server;client]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

let dependencies = [
    "InstallClient"
        ==> "Clean"
        ==> "Publish"

    "InstallClient"
        ==> "Clean"
        ==> "Run"
]

[<EntryPoint>]
let main args = runOrDefault "Run" args