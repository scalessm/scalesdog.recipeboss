# Project Context

- **Project:** scalesdog.recipeboss
- **Product:** RecipeBoss
- **Created:** 2026-04-27
- **Requested by:** Unspecified (`git config user.name` not set)

## Core Context

- RecipeBoss imports recipes from public URLs, uses Azure AI Foundry to extract structured recipe data and tags, stores images in Azure Blob Storage, and keeps per-user data in Cosmos DB.
- Frontend stack: React 18, TypeScript, Vite, shadcn/ui, Tailwind CSS.
- Backend/platform stack: ASP.NET Core 10 minimal APIs, Entra External ID, .NET Aspire, Azure Static Web Apps, Azure Container Apps.

## Recent Updates

- 📌 Team cast initialized on 2026-04-27 with Mal as Lead.
- 📌 Initial design and requirements live in `DESIGN.md`.

## Recent Updates

- 📌 2026-04-27: Solution-level scaffolding completed. Created `RecipeBoss.sln`, `global.json` (pinned .NET 10, rollForward latestMajor), `Directory.Build.props` (nullable, implicit usings, TreatWarningsAsErrors), `.editorconfig` (C# + TypeScript), and updated `README.md`. Added `RecipeBoss.Api` to the solution (already scaffolded by Zoe). `RecipeBoss.AppHost` and `RecipeBoss.ServiceDefaults` were not yet present (Book's work pending).

## Learnings

- Mal owns scope, architecture, and reviewer gating.
- Scribe owns shared memory; Ralph owns work monitoring.
- The .NET 10 SDK is not yet installed on the dev machine (only 9.0.300 is present). Run `dotnet sln` commands from the parent of the repo root to bypass `global.json` and use the available SDK.
- The frontend (`recipeboss-frontend/`) must never be added to `RecipeBoss.sln` — it is a standalone Node/Vite project.
- Decision `mal-solution-structure.md` was written to `.squad/decisions/inbox/` covering directory layout and project naming conventions.
- 2026-04-27: Restructured repo to move all source projects into `src\`. `ProjectReference` paths between sibling projects (`../RecipeBoss.X/...`) and the AppHost→frontend path (`../frontend`) required no changes after the move, as all projects remained siblings within `src\`. Only the `.sln` file needed updating via `dotnet sln remove/add`.
- Transitive Aspire dependencies (e.g. KubernetesClient) trigger NuGet audit vulnerability warnings that become errors under `TreatWarningsAsErrors`. Resolved by setting `NuGetAuditMode=direct` in `Directory.Build.props` to scope audits to our own package references only.
- 2026-04-27: Auth architecture defined for Entra External ID. Chose `login.microsoftonline.com` authority (works for both workforce and External ID tenants; `ciamlogin.com` only needed for Custom URL Domains). App ID URI `api://recipeboss.api` must match `Audience` in appsettings exactly. Client IDs are public values — committed to `.env.development`, not gitignored. MSAL env vars use `VITE_MSAL_*` prefix. Decision recorded in `mal-auth-entra-config.md`. Downstream: Zoe must wire `AddMicrosoftIdentityWebApi`; Kaylee must update hardcoded scope `api://recipeboss/Recipes.ReadWrite` → `api://recipeboss.api/Recipes.ReadWrite`.
