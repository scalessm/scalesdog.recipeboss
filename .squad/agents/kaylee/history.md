# Project Context

- **Project:** scalesdog.recipeboss
- **Product:** RecipeBoss
- **Created:** 2026-04-27
- **Requested by:** Unspecified (`git config user.name` not set)

## Core Context

- RecipeBoss includes a React 18 + TypeScript SPA with routes for dashboard, import, recipe library, recipe detail, edit, profile, and login.
- UI stack is Vite, shadcn/ui, Tailwind CSS, with dark mode default and purple/blue design tokens.
- Client auth uses MSAL React against Microsoft Entra External ID.

## Recent Updates

- 📌 Team cast initialized on 2026-04-27 with Kaylee as Frontend Dev.
- 📌 Initial source of truth for product and UI is `DESIGN.md`.

## Learnings

- Frontend ownership includes auth UX and recipe browsing/import flows.
- Backend/API coordination will usually route through Zoe, with contract review by Mal.
- Scaffolded `frontend/` on 2026-04-27: Vite + React 18 + TypeScript, Tailwind CSS v4 (via `@tailwindcss/vite` plugin), MSAL React v5, React Router v6, Axios.
- MSAL v5 removed `storeAuthStateInCookie` from `BrowserCacheOptions` — do not include it in msalConfig.
- TypeScript path aliases (`@/*` → `src/*`) require both `tsconfig.app.json` (`baseUrl` + `paths`) and `vite.config.ts` (`resolve.alias`) to be updated.
- Tailwind v4 no longer uses a `tailwind.config.js`; configuration is done entirely via the Vite plugin and CSS `@import "tailwindcss"`.
- Build verified clean: 186 modules, 228ms. Use `npm run build` from `frontend/` to validate.
- `tsconfig.json` has `verbatimModuleSyntax: true` — always use `import type` for type-only imports.
- `tsconfig.json` has `noUnusedLocals: true` and `noUnusedParameters: true` — keep imports and variables tight.
- API client functions (`src/api/recipes.ts`) accept `accessToken: string` as first parameter; token acquisition via `useMsal` hook happens in the calling component/page.
- Key file locations: `src/types/recipe.ts` (shared types), `src/api/recipes.ts` (API client), `src/components/RecipeCard.tsx`, `src/pages/RecipeLibraryPage.tsx`.
- Recipe Library page pattern: `useCallback` for token getter + `useEffect` chains for tags/recipes fetch; 300ms debounce on search via `useRef<ReturnType<typeof setTimeout>>`.
- MSAL token acquisition scope for recipe API: `["api://recipeboss.api/Recipes.ReadWrite"]` (corrected from `api://recipeboss/Recipes.ReadWrite`).
- 📌 2026-04-27: Built Recipe Library page and RecipeCard component against assumed Zoe backend contracts. Pushed to `dev/initial-setup`.
- 📌 2026-04-27: Wired MSAL auth with Entra External ID. Created `AuthProvider.tsx`, `LoginButton.tsx`, updated `msalConfig.ts` env vars to `VITE_MSAL_*` prefix, added nav header to `App.tsx`. Build clean at 1953 modules.
- `msalConfig.ts` env vars: `VITE_MSAL_CLIENT_ID`, `VITE_MSAL_AUTHORITY`, `VITE_MSAL_REDIRECT_URI`, `VITE_API_SCOPE` — all read from `.env.development`. Fallbacks hardcoded for the `api://recipeboss.api/Recipes.ReadWrite` scope.
- `AuthProvider.tsx` owns the singleton `PublicClientApplication` instance; `main.tsx` imports `AuthProvider` rather than constructing its own `MsalProvider`.
- `apiScopes` is the canonical export from `msalConfig.ts` for use in `acquireTokenSilent` calls across pages (e.g. `RecipeLibraryPage`).
- 📌 2026-04-27 (cross-agent): auth-entra session complete. Zoe (zoe-3) wired JWT Bearer middleware; CORS "DevCors" covers localhost:5173 and 5174. Backend `appsettings.json` now has matching AzureAd section (Audience: `api://recipeboss.api`). River confirmed all 16 tests pass — frontend auth contracts are correct.
- 📌 2026-04-27: Fixed MSAL loginRedirect silently failing with AADSTS650053. Root cause: `loginRequest.scopes` included the API scope `api://recipeboss.api/Recipes.ReadWrite`. Entra External ID validates all scopes at auth-flow start and rejects immediately if any scope is unknown — the login UI never appears. Fix: `loginRequest.scopes` now uses OIDC-only scopes `["openid", "profile", "offline_access"]`; `apiScopes` unchanged and used only in `acquireTokenSilent`. Also fixed `AuthCallbackPage` to capture `#error` fragment synchronously in `useState` initializer before MSAL's async init clears the hash — now shows error message + "Return home" instead of silently redirecting to `/`. Note: `Recipes.ReadWrite` scope must be verified as registered + admin-consented in Entra for API calls to work (Entra config task, not frontend). Commit: a00e985, branch: dev/initial-setup. Orchestration: `.squad/orchestration-log/2026-04-27T164913Z-kaylee.md`.
