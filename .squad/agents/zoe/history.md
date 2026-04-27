# Project Context

- **Project:** scalesdog.recipeboss
- **Product:** RecipeBoss
- **Created:** 2026-04-27
- **Requested by:** Unspecified (`git config user.name` not set)

## Core Context

- RecipeBoss backend is ASP.NET Core 10 in minimal API style.
- Key integrations are Azure AI Foundry for extraction/tagging, HtmlAgilityPack or PuppeteerSharp for scraping, Cosmos DB for per-user documents, Blob Storage for images, and Microsoft.Identity.Web for JWT validation.
- Data isolation uses `userId` on all recipe documents and `/userId` as the Cosmos partition key.

## Recent Updates

- 📌 Team cast initialized on 2026-04-27 with Zoe as Backend Dev.
- 📌 Backend requirements and fallback handling are defined in `DESIGN.md`.

## Learnings

- API ownership includes import, image, auth, and persistence concerns.
- Security and correctness around per-user access are part of baseline backend work.
- `dotnet add package` fails when run from inside the repo root if `global.json` pins an SDK version not installed. Workaround: `Push-Location C:\` first to escape the global.json scope, then pass the full `.csproj` path to `dotnet add`.
- The repo `global.json` pins SDK `10.0.0` with `rollForward: latestMajor`, but only .NET 9.0.300 is installed. The project scaffolds and builds under net9.0 because `dotnet build` run from outside the repo root resolves correctly using 9.0.300.
- Scaffold generated `net9.0` target framework (not `net10.0`) because .NET 10 SDK is not yet installed in this environment.

## Learnings

- API ownership includes import, image, auth, and persistence concerns.
- Security and correctness around per-user access are part of baseline backend work.
- `dotnet add package` fails when run from inside the repo root if `global.json` pins an SDK version not installed. Workaround: `Push-Location C:\` first to escape the global.json scope, then pass the full `.csproj` path to `dotnet add`.
- The repo `global.json` pins SDK `10.0.0` with `rollForward: latestMajor`, but only .NET 9.0.300 is installed. The project scaffolds and builds under net9.0 because `dotnet build` run from outside the repo root resolves correctly using 9.0.300.
- Scaffold generated `net9.0` target framework (not `net10.0`) because .NET 10 SDK is not yet installed in this environment.

## Recent Updates

- 📌 2026-04-27: Scaffolded `RecipeBoss.Api/` — minimal API project with all NuGet packages, domain models (Recipe, Ingredient, Rating, UserProfile), RecipeEndpoints stub, CORS/auth/OpenAPI wiring in Program.cs, and placeholder appsettings.json sections. Build passes.
- 📌 2026-04-27: Implemented `GET /api/v1/recipes` and `GET /api/v1/recipes/tags`. Updated `Models/Recipe.cs` to immutable records with nullable optional fields. Created `Infrastructure/CosmosDbService.cs` with `IRecipeRepository` interface, `InMemoryRecipeRepository` (4 seeded realistic recipes, tag AND-filter, contains-search, pagination), and stub `CosmosRecipeRepository`. Registered `InMemoryRecipeRepository` as singleton in `Program.cs`. UserId extracted from `oid` then `sub` JWT claim. Build passes with 0 errors. Pushed to `dev/initial-setup`.
- 📌 2026-04-27: All decisions documented in `.squad/decisions.md` (architecture, endpoints, project structure, repository contract).
