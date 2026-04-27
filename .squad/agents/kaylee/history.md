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
- MSAL token acquisition scope for recipe API: `["api://recipeboss/Recipes.ReadWrite"]`.
- 📌 2026-04-27: Built Recipe Library page and RecipeCard component against assumed Zoe backend contracts. Pushed to `dev/initial-setup`.
