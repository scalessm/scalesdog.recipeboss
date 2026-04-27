# Project Context

- **Project:** scalesdog.recipeboss
- **Product:** RecipeBoss
- **Created:** 2026-04-27
- **Requested by:** Unspecified (`git config user.name` not set)

## Core Context

- RecipeBoss deploys on Azure with .NET Aspire as the orchestration layer.
- Frontend hosting targets Azure Static Web Apps; API and supporting services target Azure Container Apps.
- Secrets are expected to flow through Azure Key Vault and environment wiring managed via Aspire.

## Recent Updates

- 📌 Team cast initialized on 2026-04-27 with Book as DevOps.
- 📌 Deployment and infrastructure requirements are defined in `DESIGN.md`.

## Learnings

- Book owns environment wiring, deployment topology, and runtime concerns.
- Frontend and backend specialists remain primary owners of application code unless routed otherwise.
- The repo's `global.json` pins SDK to `10.0.0` (rollForward: latestMajor) but only .NET 9.0.300 is installed. All `dotnet` commands for this project must be run from outside the repo directory (e.g. `cd C:\Users\scale`) to avoid the SDK version conflict until .NET 10 SDK is installed.
- `dotnet workload install aspire` installs Aspire 9.0.0 templates (installed 2026-04-27).
- `Aspire.Hosting.NodeJs` package (v9.0.0) is required to use `AddNpmApp()` in the AppHost.
- ServiceDefaults project reference in AppHost must use `IsAspireProjectResource="false"` to suppress ASPIRE004 warning (ServiceDefaults is a library, not an executable resource).
- `Microsoft.Azure.Cosmos` v3.59.0 requires explicit Newtonsoft.Json or `AzureCosmosDisableNewtonsoftJsonCheck=true` in the csproj. RecipeBoss API uses System.Text.Json so the bypass property is set.
- Aspire scaffold generates `appsettings.Development.json` in AppHost by default; connection string stubs for CosmosDb and BlobStorage were appended to that file.
