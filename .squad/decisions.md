# Squad Decisions

## Active Decisions

- 2026-04-27: RecipeBoss uses a Firefly assignment for the initial squad cast: Mal (Lead), Kaylee (Frontend), Zoe (Backend), River (Tester), and Book (DevOps).
- 2026-04-27: Scribe remains the dedicated session logger and Ralph remains the dedicated work monitor for all future work.
- 2026-04-27: Commit and push the dev/initial-setup branch periodically during development work (user directive).

## Architecture & Infrastructure

### Solution Structure and Project Layout (Mal, 2026-04-27)

The following directory layout is adopted at the repo root:

```
scalesdog.recipeboss/
├── RecipeBoss.sln                  # Visual Studio solution (all .NET projects)
├── src/
│   ├── RecipeBoss.Api/             # ASP.NET Core 10 minimal API (owned by Zoe)
│   ├── RecipeBoss.AppHost/         # .NET Aspire AppHost (owned by Book)
│   ├── RecipeBoss.ServiceDefaults/ # Shared Aspire service defaults (owned by Book)
│   └── frontend/                   # React 18 + Vite + TypeScript SPA (owned by Kaylee)
├── Directory.Build.props           # Shared .NET build settings (nullable, implicit usings, TreatWarningsAsErrors)
├── global.json                     # .NET SDK pinned to 10.0.0 (rollForward: latestMajor)
├── .editorconfig                   # Editor formatting for C# + TypeScript
├── README.md                       # Solution overview, how to run, stack summary
└── DESIGN.md                       # Full product design document
```

All new .NET projects MUST live under `RecipeBoss.<Name>/` with matching `.csproj`. Frontend MUST NOT be added to `.sln`. (See `mal-solution-structure.md` for full context.)

### Move Source Projects to src\ Directory (Mal, 2026-04-27)

All source code projects moved into `src\` subdirectory to separate source from repo-level concerns. Root-level files remain at root. `dotnet sln remove/add` and `ProjectReference` paths required no changes — all projects remain siblings within `src\`. `NuGetAuditMode=direct` added to `Directory.Build.props` to prevent transitive Aspire dependency failures. Verified: `dotnet build RecipeBoss.sln` — Build succeeded, 0 warnings, 0 errors. `npm run build` (in `src\frontend`) — Built successfully in 215ms.

### .NET Aspire Scaffold for RecipeBoss (Book, 2026-04-27)

RecipeBoss uses .NET Aspire as its local development orchestrator and Azure deployment manifest generator.

**Aspire Project Layout:**
| Project | Purpose |
|---------|---------|
| `RecipeBoss.AppHost` | Aspire host — declares all services and their relationships. Entry point for local dev (`dotnet run --project RecipeBoss.AppHost`). |
| `RecipeBoss.ServiceDefaults` | Shared defaults — OpenTelemetry, health checks, service discovery configuration injected into all participating services. |

Both target `net9.0` (upgrade to `net10.0` once .NET 10 SDK installed).

**Service Names in AppHost:**
| Aspire Resource Name | Maps To |
|---------------------|---------|
| `recipeboss-api` | `RecipeBoss.Api` — ASP.NET Core 10 Web API |
| `frontend` | `../frontend` npm app (Vite dev server, port 5173) |

Frontend is wired as an `AddNpmApp` resource with `ExcludeFromManifest()` — it deploys to Azure Static Web Apps, not as an Aspire-managed container.

`AddServiceDefaults()` called in API `Program.cs` wires OpenTelemetry, health check endpoints (`/health`, `/alive`), and service discovery. Connection string stubs for `CosmosDb` and `BlobStorage` in `RecipeBoss.AppHost/appsettings.Development.json` (never committed with real values).

Local development: `dotnet run --project RecipeBoss.AppHost` launches the Aspire dashboard at `http://localhost:15888` and orchestrates API and frontend processes. No Docker dependency for local dev.

**SDK version gap:** `global.json` specifies .NET 10 SDK; all `dotnet` CLI commands must be run from outside the repo directory until SDK is available.

### Entra External ID Auth Architecture (Mal, 2026-04-27)

RecipeBoss uses Entra External ID on tenant `scalesdog.onmicrosoft.com` with two app registrations: React SPA (frontend) and ASP.NET Core API (backend).

**Key Decisions:**
1. **Authority URL:** `https://login.microsoftonline.com/scalesdog.onmicrosoft.com` — standard endpoint preferred over `ciamlogin.com`; latter is only required for Custom URL Domains or CIAM-specific B2C policies.
2. **Scope:** `api://recipeboss.api/Recipes.ReadWrite` — App ID URI is `api://recipeboss.api` (registered on API app registration `37cb3b50-4dda-40f9-a3a9-c06f069bfacc`). Old placeholder `api://recipeboss/Recipes.ReadWrite` was incorrect and has been removed from all references.
3. **Env vars committed to `.env.development`** — Client IDs and authority URLs are public values visible in JS bundles. Committing to `.env.development` ensures all developers get correct values without manual setup. Secrets go in `.env.local` (gitignored via `*.local`).
4. **`AzureAd` config structure in `appsettings.json`:**
   ```json
   "AzureAd": {
     "Instance": "https://login.microsoftonline.com/",
     "TenantId": "scalesdog.onmicrosoft.com",
     "ClientId": "37cb3b50-4dda-40f9-a3a9-c06f069bfacc",
     "Audience": "api://recipeboss.api"
   }
   ```
   `Instance` + `TenantId` form the authority. `Audience` must match App ID URI exactly for `aud` claim validation.

## Frontend

### Frontend Scaffold Architecture Decisions (Kaylee, 2026-04-27)

RecipeBoss React SPA scaffolded at `src/frontend/`.

**Key Decisions:**
- Tailwind CSS v4 via `@tailwindcss/vite` plugin (no `tailwind.config.js`)
- Path alias `@` → `src/` (registered in `tsconfig.app.json` and `vite.config.ts`)
- MSAL v5 — sessionStorage caching, no cookie fallback
- `BrowserRouter` in `main.tsx` only; `Routes`/`Route` in `App.tsx`
- Page stubs under `src/pages/`, shared components in `src/components/`, auth in `src/auth/`, utilities in `src/lib/`
- Design tokens as CSS custom properties in `src/index.css` under `:root`

### Recipe Library API Contract Assumptions (Kaylee, 2026-04-27)

Built the Recipe Library page and RecipeCard component against assumed backend contracts. Awaiting Zoe validation.

**Assumed `GET /api/v1/recipes` query parameters:**
- `q` (string, optional) — full-text search term
- `tags` (string, optional) — comma-separated tag values (e.g. `tags=Italian,Pasta`)
- `page` (integer, optional) — pagination
- `pageSize` (integer, optional)

**Response:** `RecipeSummary[]` with fields: `id`, `title`, `description?`, `servings?`, `prepTimeMinutes?`, `cookTimeMinutes?`, `tags[]`, `imageUrls[]` (first used as thumbnail), `rating?` (number 1–5, ratedAt ISO 8601, notes?), `importedAt` (ISO 8601).

**Assumed `GET /api/v1/recipes/tags`:** Returns `string[]` — flat list of all tags.

**Auth:** All endpoints expect `Authorization: Bearer <token>` using scope `api://recipeboss/Recipes.ReadWrite`.

**Open questions:** (1) Tags filter format (comma-separated or repeated params)? (2) Pagination style (cursor/offset)? (3) Image URLs (absolute or relative)? (4) Tags endpoint scoped per user or global?

### MSAL Frontend Auth Integration (Kaylee, 2026-04-27)

Implemented and shipped on `dev/initial-setup`. Entra External ID tenant config applied to the SPA.

**Key Decisions:**
1. **Canonical env var names: `VITE_MSAL_*` prefix.** `msalConfig.ts` reads `VITE_MSAL_CLIENT_ID`, `VITE_MSAL_AUTHORITY`, `VITE_MSAL_REDIRECT_URI`, and `VITE_API_SCOPE`. Old `VITE_ENTRA_*` names deprecated and removed.
2. **Scope corrected to `api://recipeboss.api/Recipes.ReadWrite`.** Both `msalConfig.ts` and `RecipeLibraryPage.tsx` now import `apiScopes` from `@/auth/msalConfig`.
3. **AuthProvider pattern:** `PublicClientApplication` constructed once in `auth/AuthProvider.tsx`. `main.tsx` imports `<AuthProvider>` — avoids duplicate MSAL instances.
4. **LoginButton uses design tokens** (`var(--color-primary)`) not raw Tailwind color classes — consistent with `index.css` token system. Tailwind v4 compatible.
5. **Nav header added to `App.tsx`:** Persistent top nav with brand link, Library and Import links, and `LoginButton`. Nav uses `--color-surface` background token.

**Open Questions:** (1) Redirect URI in production (Static Web Apps URL)? Currently falls back to `window.location.origin`. (2) Backend JWT validation authority should match `https://login.microsoftonline.com/scalesdog.onmicrosoft.com`.

## Backend

### RecipeBoss.Api Scaffold — Backend Architecture (Zoe, 2026-04-27)

`RecipeBoss.Api/` follows a minimal API layout:

```
RecipeBoss.Api/
├── Endpoints/          # Route registration extensions (MapXyzEndpoints pattern)
├── Infrastructure/     # Cosmos DB, Blob Storage client wrappers
├── Models/             # Domain types and DTOs (Recipe, UserProfile, request/response records)
├── Services/           # Business logic (ImportService, ImageService, etc.)
├── Program.cs          # Composition root — auth, CORS, OpenAPI, endpoint mapping
└── appsettings.json    # Placeholder config sections for all integrations
```

**NuGet Packages:**
| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Identity.Web` | 4.8.0 | JWT validation for Entra External ID tokens |
| `Microsoft.Identity.Web.MicrosoftGraph` | 4.8.0 | Optional Graph calls (user profile enrichment) |
| `Microsoft.Azure.Cosmos` | 3.59.0 | Cosmos DB for NoSQL SDK |
| `Azure.Storage.Blobs` | 12.27.0 | Blob Storage for recipe images |
| `Azure.AI.OpenAI` | 2.1.0 | Azure OpenAI / AI Foundry GPT-4o calls |
| `HtmlAgilityPack` | 1.12.4 | HTML scraping and cleanup |
| `Microsoft.AspNetCore.OpenApi` | 9.0.5 | OpenAPI document generation |
| `Scalar.AspNetCore` | 2.14.5 | Developer API explorer UI (replaces Swagger UI) |

**appsettings.json:** Placeholder keys for `AzureAd`, `CosmosDb`, `BlobStorage`, `AzureOpenAI`, `Frontend.BaseUrl`. No secrets committed.

### Recipe Endpoint Patterns — GET /recipes and GET /recipes/tags (Zoe, 2026-04-27)

**Active Implementation:** `InMemoryRecipeRepository` registered as `IRecipeRepository` singleton, seeded with 4 realistic sample recipes (Italian, Indian, Mexican, Japanese). `CosmosRecipeRepository` exists as stub with TODOs; swap implementations by changing one line in `Program.cs`.

**Repository Interface Contract:** `IRecipeRepository` defines: `GetRecipesAsync`, `GetTagsAsync`, `GetRecipeAsync`, `UpsertRecipeAsync`, `DeleteRecipeAsync`. All methods async, accept `CancellationToken`.

**UserId Claim Extraction:** `oid` (Entra object ID, preferred) fallback to `sub` (other OIDC providers). Missing claim throws `UnauthorizedAccessException`.

**Tag Filtering:** Uses AND logic. `?tags=Italian,Pasta` returns recipes with **all** specified tags (stricter, more useful). Tags split on comma, trimmed, matched case-insensitively.

**List Endpoint:** `GET /api/v1/recipes` returns `IEnumerable<RecipeSummary>` projection (no `Ingredients` or `Instructions` fields). Full detail reserved for `GET /api/v1/recipes/{id}`.

**Route Prefix:** `/api/v1/recipes` (version segment per API design conventions).

### JWT Bearer Authentication — Entra External ID (Zoe, 2026-04-27)

Implemented real JWT Bearer auth validated against Entra External ID, replacing stub auth call.

**Key Decisions:**
1. **Auth Registration Pattern:** `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(config.GetSection("AzureAd"))` — explicit scheme, better self-documenting, canonical Microsoft.Identity.Web v4 usage.
2. **Explicit `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.7 package reference** — prevents version drift from transitive `Microsoft.Identity.Web` dependency.
3. **Named CORS Policy `"DevCors"`** — covers `http://localhost:5173` and `http://localhost:5174` (Vite fallback port). Named policies are explicit and composable.
4. **Middleware Order:** `UseCors("DevCors")` → `UseAuthentication()` → `UseAuthorization()` → endpoint mapping. CORS must precede auth to avoid 401 on OPTIONS preflight requests.
5. **`appsettings.json` AzureAd values:** Instance `https://login.microsoftonline.com/`, TenantId `scalesdog.onmicrosoft.com`, ClientId `37cb3b50-4dda-40f9-a3a9-c06f069bfacc`, Audience `api://recipeboss.api`. `appsettings.Development.json` does **not** override AzureAd.

**Outcome:** All 16 tests pass. `TestAuthHandler` populates `oid`/`sub` claims matching endpoint claim extraction logic.

## Testing

### Test Scaffold Decision: RecipeBoss.Api.Tests (River, 2026-04-27)

Created `src/RecipeBoss.Api.Tests/` as an xUnit 2.9.3 project targeting net10.0. Added `Microsoft.AspNetCore.Mvc.Testing` 10.0.0 and `FluentAssertions` 8.2.0. Suppressed CA1707 (underscore test method naming is xUnit convention). Added `public partial class Program {}` to `RecipeBoss.Api/Program.cs` for test host access.

**Repository Unit Tests** (10 tests):
- User isolation, empty list, baseline listing
- Title/description full-text search (case-insensitive)
- Tags AND-filter logic
- Combined tags + q filter
- Pagination skip logic
- Distinct tags per user (no duplication, no cross-user leakage)

**Endpoint Integration Tests** (6 tests):
- Happy path 200 responses, 401 for missing auth
- Query and tags param filtering
- Tags endpoint 200 and 401

**Test Status:** All 16 tests skip cleanly (no failures, build clean). Tests await Zoe's implementation to unskip.

**Blockers Found (Action Required by Zoe):**
1. **Route Prefix Mismatch:** Current `/api/recipes`, should be `/api/v1/recipes` per DESIGN.md.
2. **Missing `IRecipeRepository` interface** — all 16 tests depend on this.
3. **Missing `InMemoryRecipeRepository` implementation** — required for both unit tests and `WebApplicationFactory` service replacement.
4. **Missing `GET /recipes/tags` endpoint** — must add alongside route prefix fix.

**Unskip Checklist:** When Zoe's implementation lands, River must remove `[Fact(Skip = ...)]` from all 16 tests, uncomment `IRecipeRepository` replacement in test factory, verify `TestAuthHandler` claim name matches endpoint extraction, and run full suite to confirm 16/16 pass.

### Test Verdict — Auth Session: All 16/16 Pass (River, 2026-04-27)

Status: ✅ APPROVED. All blockers from the scaffold decision resolved by Zoe.

| Suite | Tests | Result |
|---|---|---|
| InMemoryRecipeRepository unit tests | 10 | ✅ Pass |
| RecipeEndpoints integration tests | 6 | ✅ Pass |
| **Total** | **16** | **✅ 16/16** |

**Contract Verification:**
- Route prefix `/api/v1/recipes` ✅
- Tags parameter comma-separated (`?tags=Italian,Pasta`) confirmed ✅
- `IRecipeRepository` + `InMemoryRecipeRepository` present ✅
- 4 seed recipes for `dev-user-001` ✅

**Test Changes by River:**
- `InMemoryRecipeRepositoryTests.cs`: Full implementations added (user isolation, search, tag AND-filter, pagination, distinct tags)
- `RecipeEndpointsTests.cs`: All `[Fact(Skip=...)]` removed; `InMemoryRecipeRepository` registered in test factory; `services.RemoveAll<IRecipeRepository>()` refactored

**Verdict:** Clear to merge into `dev/initial-setup`. No follow-up on API implementation needed.

### MSAL Frontend Auth—Login Scopes & Error Surfacing (Kaylee, 2026-04-27)

Diagnosed and fixed silent MSAL login failure. Root cause: `loginRequest.scopes` in `msalConfig.ts` included the API scope `api://recipeboss.api/Recipes.ReadWrite`, causing Entra to reject the authorization request before any login UI appeared (error: `AADSTS650053`). 

**Fixes applied:**
1. `loginRequest.scopes` changed to OIDC-only (`["openid", "profile", "offline_access"]`). API scopes remain in `apiScopes` constant for lazy `acquireTokenSilent` calls after authentication.
2. `AuthCallbackPage.tsx` now captures hash fragments synchronously and displays error descriptions instead of silently redirecting on Entra error responses.

**Remaining action:** Verify `Recipes.ReadWrite` scope is registered and admin-consented in Entra portal (37cb3b50-4dda-40f9-a3a9-c06f069bfacc → Expose an API). Commit: a00e985, branch: dev/initial-setup.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
