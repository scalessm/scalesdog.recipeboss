# Project Context

- **Project:** scalesdog.recipeboss
- **Product:** RecipeBoss
- **Created:** 2026-04-27
- **Requested by:** Unspecified (`git config user.name` not set)

## Core Context

- RecipeBoss must cover recipe import, AI extraction, tagging, image handling, recipe browsing, and user ratings.
- Auth and data isolation are critical: every request and persisted document is scoped to the authenticated user.
- Failure handling in the design includes invalid AI JSON, AI timeouts, blocked scraping, and image download failures.

## Recent Updates

- 📌 Team cast initialized on 2026-04-27 with River as Tester.
- 📌 The design doc in `DESIGN.md` defines the initial failure modes and acceptance surface.

## Learnings

- River acts as both test owner and reviewer gate.
- Reviewer rejection lockout must be enforced on rejected artifacts.
- Use `public partial class Program {}` at the end of `Program.cs` to expose the top-level Program class to `WebApplicationFactory<Program>` in the test project. This is the standard minimal-API pattern.
- Suppress CA1707 in the test `.csproj` via `<NoWarn>$(NoWarn);CA1707</NoWarn>` — underscore-separated test names (`Method_Scenario_Expected`) are xUnit convention and the analyzer fires otherwise.
- For `WebApplicationFactory` auth replacement: register a custom `AuthenticationHandler` that reads the `Authorization` header and succeeds with a fixed `oid` claim. This keeps test auth hermetic.
- Contract gap found (2026-04-27): Current `RecipeEndpoints.cs` registers routes under `/api/recipes`; DESIGN.md specifies the base as `/api/v1`. **Zoe must correct the route prefix** before any endpoint smoke tests can be unskipped.
- Contract gap found (2026-04-27): `IRecipeRepository` and `InMemoryRecipeRepository` do not yet exist in `RecipeBoss.Api`. All 10 repository unit tests and all 6 endpoint integration tests are pending these types.
- `GET /recipes/tags` is not yet wired in `RecipeEndpoints.cs` — the endpoint stub only has `GET /`, `GET /{id}`, `POST /import`, `PUT /{id}`, `DELETE /{id}`, `PUT /{id}/rating`.
- 📌 2026-04-27 (follow-up): Zoe's implementation delivered all contract items. Unskipped all 16 tests. Verified claim extraction. Result: **16/16 tests passing** ✓
- **RESOLVED (2026-04-27 14:02)**: Zoe implemented `IRecipeRepository`, `InMemoryRecipeRepository`, and both endpoints (`GET /api/v1/recipes` and `GET /api/v1/recipes/tags`). All 16 tests now pass. Tags are passed as comma-separated query string (`?tags=Italian,Pasta`) as designed.
- 📌 2026-04-27 (cross-agent): auth-entra session complete. JWT Bearer middleware wired by Zoe (zoe-3); MSAL wired by Kaylee (kaylee-2). All 16 tests including authenticated endpoint flows confirmed passing. `TestAuthHandler` `oid`/`sub` claim pattern is compatible with real JWT auth middleware — no test changes needed for auth wiring.
