module PlaningPoker2.Server.Program

open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

[<EntryPoint>]
HostBuilder()
   .ConfigureFunctionsWorkerDefaults(
       fun (b:IFunctionsWorkerApplicationBuilder) ->
           b.Services.AddServerlessHub<Functions>()
           |> ignore
       )
    .Build()
    .Run()