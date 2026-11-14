# aspnetcore-getdocument-timeout-repro

Minimal repro: build-time OpenAPI generation with `Microsoft.Extensions.ApiDescription.Server` fails when generating all documents takes longer than the hardcoded 2-minute timeout in `dotnet-getdocument`.

## Versions

- .NET SDK 10.0.403
- `Microsoft.AspNetCore.OpenApi` 10.0.12
- `Microsoft.Extensions.ApiDescription.Server` 10.0.12

## Steps

```sh
dotnet build
```

`Program.cs` registers 20 OpenAPI documents with 3,000 endpoints each (GET + POST for each of `EndpointsPerDocument = 1500`). Build-time generation runs all documents in a single `dotnet-getdocument` process, so the total exceeds 2 minutes.

## Actual

```
error : System.TimeoutException: Process .../dotnet timed out after 2 minutes.
error :    at Microsoft.Extensions.ApiDescription.Tool.Exe.Run(...)
error :    at Microsoft.Extensions.ApiDescription.Tool.Commands.InvokeCommand.Execute()
error MSB3073: The command "dotnet ".../dotnet-getdocument.dll" --assembly ".../Repro.dll" ..." exited with code 1.
```

With `EndpointsPerDocument = 300` the same build succeeds (about 35 seconds).

## Expected

A way to raise the timeout (CLI option / MSBuild property), since the limit is hardcoded:

- `src/Tools/dotnet-getdocument/src/Exe.cs`: `process.WaitForExit(TimeSpan.FromMinutes(2))`
- `src/Tools/GetDocumentInsider/src/Commands/GetDocumentCommandWorker.cs`: `resultTask.Wait(TimeSpan.FromMinutes(1))` per document
