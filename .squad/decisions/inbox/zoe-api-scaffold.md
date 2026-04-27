# Decision: RecipeBoss.Api Scaffold — Backend Architecture

**Date:** 2026-04-27  
**Author:** Zoe (Backend Dev)  
**Status:** Accepted

---

## Project Structure

`RecipeBoss.Api/` follows a minimal API layout rooted at the repo:

```
RecipeBoss.Api/
├── Endpoints/          # Route registration extensions (MapXyzEndpoints pattern)
├── Infrastructure/     # Cosmos DB, Blob Storage client wrappers
├── Models/             # Domain types and DTOs (Recipe, UserProfile, request/response records)
├── Services/           # Business logic (ImportService, ImageService, etc.)
├── Program.cs          # Composition root — auth, CORS, OpenAPI, endpoint mapping
└── appsettings.json    # Placeholder config sections for all integrations
```

Rationale: flat, role-named folders keep the service small and navigable. No layered architecture overhead at this stage; promote to vertical slices if complexity warrants it.

---

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Identity.Web` | 4.8.0 | JWT validation for Entra External ID tokens |
| `Microsoft.Identity.Web.MicrosoftGraph` | 4.8.0 | Optional Graph calls (user profile enrichment) |
| `Microsoft.Azure.Cosmos` | 3.59.0 | Cosmos DB for NoSQL SDK |
| `Azure.Storage.Blobs` | 12.27.0 | Blob Storage for recipe images |
| `Azure.AI.OpenAI` | 2.1.0 | Azure OpenAI / AI Foundry GPT-4o calls |
| `HtmlAgilityPack` | 1.12.4 | HTML scraping and cleanup |
| `Microsoft.AspNetCore.OpenApi` | 9.0.5 | OpenAPI document generation (built-in minimal API integration) |
| `Scalar.AspNetCore` | 2.14.5 | Developer API explorer UI (replaces Swagger UI) |

---

## appsettings.json Placeholder Layout

Config keys follow the same hierarchy used in `IOptions<T>` binding:

- `AzureAd` — Microsoft.Identity.Web binding; TenantId and ClientId are `__PLACEHOLDER__` strings, not real values.
- `CosmosDb` — connection string, database name (`RecipeBoss`), container name (`Items`).
- `BlobStorage` — connection string, container name (`recipe-images`).
- `AzureOpenAI` — endpoint URL and deployment name (`gpt-4o`); key is expected from Key Vault / environment variable, not config file.
- `Frontend.BaseUrl` — CORS allow-origin for the React SPA; defaults to `http://localhost:5173` for local dev.

No secrets are committed. Real values are provided via environment variables or Azure Key Vault references in the deployed environment.

---

## Notes

- Target framework is `net9.0` because .NET 10 SDK is not yet installed in the development environment. Upgrade target to `net10.0` once the SDK is available.
- `global.json` at repo root pins SDK `10.0.0` with `rollForward: latestMajor`. Run `dotnet` commands from outside the repo root (e.g., `Push-Location C:\`) to work around this until .NET 10 SDK is installed.
