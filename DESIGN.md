# RecipeBoss — Design Document

> **Status:** Draft v1.0 — Ready for Review

---

## Table of Contents

1. [Overview](#overview)
2. [Goals & Non-Goals](#goals--non-goals)
3. [Technology Stack](#technology-stack)
4. [System Architecture](#system-architecture)
5. [Authentication & Authorization](#authentication--authorization)
6. [Data Models](#data-models)
7. [AI Integration](#ai-integration)
8. [Frontend Design](#frontend-design)
9. [API Design](#api-design)
10. [Image Handling](#image-handling)
11. [Deployment Architecture](#deployment-architecture)
12. [Cost & Infrastructure Considerations](#cost--infrastructure-considerations)
13. [Security Considerations](#security-considerations)
14. [Open Questions](#open-questions)

---

## Overview

**RecipeBoss** is a modern web application that lets authenticated users import, store, rate, and browse recipes. A user pastes a URL to any recipe page; the app scrapes the page, passes the raw content through an AI model (Azure AI Foundry) to extract and standardize the recipe, applies AI-generated tags, downloads associated images, and stores everything in the database. Users can then browse their personal recipe library, filter by tags, and rate recipes.

---

## Goals & Non-Goals

### Goals

| # | Goal |
|---|------|
| G1 | Import a recipe from any publicly accessible URL |
| G2 | Use AI to normalize recipe content into a consistent schema |
| G3 | AI-generated tagging (cuisine, dietary restrictions, meal type, etc.) |
| G4 | Store recipe images alongside structured recipe data |
| G5 | Browse, search, and filter recipes by one or more tags |
| G6 | Rate recipes (1–5 stars) |
| G7 | Secure, per-user data isolation via Entra External ID |
| G8 | Deploy cost-efficiently on Azure using .NET Aspire |

### Non-Goals (v1)

- Social / sharing features
- Meal planning / scheduling
- Grocery list generation
- Real-time collaboration
- Native mobile applications

---

## Technology Stack

| Layer | Technology | Notes |
|-------|-----------|-------|
| Frontend | React 18 + TypeScript | Vite build tooling |
| UI Component Library | shadcn/ui + Tailwind CSS | Purple/blue design tokens |
| Backend | ASP.NET Core 8 (Web API) | Minimal API style |
| AI Orchestration | Azure AI Foundry (Azure OpenAI) | GPT-4o for extraction & tagging |
| Web Scraping | .NET `HtmlAgilityPack` / `PuppeteerSharp` | Fallback to headless browser for JS-heavy pages |
| Database | Azure Cosmos DB for NoSQL | Per-user partitioning |
| Blob Storage | Azure Blob Storage | Recipe images |
| Authentication | Microsoft Entra External ID (CIAM) | MSAL React on frontend; JWT validation on API |
| Orchestration | .NET Aspire | Service composition, local dev dashboard, deployment |
| Hosting | Azure Container Apps | Aspire-native deployment target |
| CDN / Static Hosting | Azure Static Web Apps | React SPA |
| Secret Management | Azure Key Vault | Referenced by Aspire |

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Browser (React SPA)                      │
│         Hosted on Azure Static Web Apps / Container Apps        │
└───────────────────────────┬─────────────────────────────────────┘
                            │ HTTPS (JWT in Authorization header)
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                  RecipeBoss API (ASP.NET Core 8)                │
│                    Azure Container Apps                         │
│                                                                 │
│  ┌─────────────┐  ┌──────────────────┐  ┌────────────────────┐ │
│  │  Recipe     │  │  Import          │  │  Image             │ │
│  │  Endpoints  │  │  Service         │  │  Service           │ │
│  └──────┬──────┘  └────────┬─────────┘  └────────┬───────────┘ │
│         │                  │                       │            │
└─────────┼──────────────────┼───────────────────────┼────────────┘
          │                  │                       │
          ▼                  ▼                       ▼
  ┌───────────────┐  ┌───────────────┐      ┌────────────────────┐
  │ Azure Cosmos  │  │ Azure AI      │      │ Azure Blob Storage │
  │ DB (NoSQL)    │  │ Foundry       │      │ (recipe images)    │
  └───────────────┘  │ (OpenAI GPT)  │      └────────────────────┘
                     └───────────────┘
                            ▲
                   ┌────────┴────────┐
                   │  Web Scraper    │
                   │  (HtmlAgility / │
                   │  Puppeteer)     │
                   └─────────────────┘

          ┌────────────────────────────────┐
          │   Microsoft Entra External ID  │
          │   (CIAM — Auth & Identity)     │
          └────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility |
|-----------|---------------|
| React SPA | UI rendering, MSAL authentication, calling the REST API |
| ASP.NET Core API | Request routing, business logic, auth middleware, Cosmos DB access |
| Import Service | Scrape URL, invoke AI Foundry, persist structured recipe |
| Image Service | Download images from recipe page, store in Blob, return CDN URLs |
| Azure AI Foundry | Extract recipe schema from raw HTML; generate taxonomy tags |
| Cosmos DB | Persistent store for recipes, user profiles, ratings |
| Blob Storage | Binary storage for recipe images |
| Entra External ID | User authentication and JWT issuance |
| .NET Aspire | Local dev orchestration, service discovery, Azure deployment manifest |

---

## Authentication & Authorization

### Provider: Microsoft Entra External ID (CIAM)

Entra External ID is Microsoft's customer-identity platform, suitable for consumer-facing apps with straightforward sign-up/sign-in flows.

**Flow:**

1. User clicks **Sign In** in the React SPA.
2. MSAL React redirects to the Entra External ID hosted sign-in page.
3. User authenticates (email + password or social providers if configured).
4. Entra issues an **ID token** (user identity) and an **Access token** (API scope).
5. SPA stores tokens in memory (not localStorage) and attaches the access token as `Authorization: Bearer <token>` on every API call.
6. The ASP.NET Core API validates the JWT signature, audience, and issuer on every request via the `Microsoft.Identity.Web` middleware.

### Data Isolation

- Every Cosmos DB document includes a `userId` field (the Entra object ID).
- API queries always add a `WHERE userId = @currentUserId` predicate.
- Container partition key is `/userId`, so cross-user data leakage is prevented at the database level.

### Scopes

| Scope | Description |
|-------|-------------|
| `api://recipeboss/Recipes.ReadWrite` | CRUD operations on recipes |
| `api://recipeboss/Ratings.ReadWrite` | Create and update ratings |

---

## Data Models

### Recipe

```json
{
  "id": "uuid-v4",
  "userId": "entra-object-id",
  "sourceUrl": "https://example.com/recipe/pasta",
  "title": "Classic Spaghetti Carbonara",
  "description": "A rich Roman pasta dish...",
  "servings": 4,
  "prepTimeMinutes": 15,
  "cookTimeMinutes": 20,
  "ingredients": [
    { "quantity": "200g", "item": "spaghetti" },
    { "quantity": "150g", "item": "guanciale or pancetta" },
    { "quantity": "4", "item": "large eggs" }
  ],
  "instructions": [
    "Bring a large pot of salted water to a boil...",
    "Fry the guanciale until crispy..."
  ],
  "tags": ["Italian", "Pasta", "Quick", "Gluten-containing"],
  "imageUrls": [
    "https://<storage>.blob.core.windows.net/recipes/<id>/hero.jpg"
  ],
  "rating": null,
  "importedAt": "2026-04-25T22:00:00Z",
  "updatedAt": "2026-04-25T22:00:00Z",
  "type": "recipe"
}
```

### Rating

Embedded in the Recipe document (single user app per account; ratings are personal).

```json
{
  "rating": 4,
  "ratedAt": "2026-04-26T10:00:00Z",
  "notes": "Delicious — used bacon instead of guanciale."
}
```

### User Profile

Minimal profile, created on first login.

```json
{
  "id": "entra-object-id",
  "userId": "entra-object-id",
  "displayName": "Jane Smith",
  "email": "jane@example.com",
  "createdAt": "2026-04-25T20:00:00Z",
  "type": "userProfile"
}
```

### Tag Taxonomy (Reference Data)

AI-generated tags are free-form strings stored directly on the Recipe document. The frontend dynamically derives the tag list from the user's recipe corpus for filtering purposes. No separate tag collection is required in v1.

Common expected tags (AI-generated):

- **Cuisine:** Italian, Mexican, Thai, American, Indian, …
- **Meal Type:** Breakfast, Lunch, Dinner, Snack, Dessert, Appetizer
- **Dietary:** Vegetarian, Vegan, Gluten-Free, Dairy-Free, Keto, Paleo
- **Cook Method:** Baked, Grilled, Instant Pot, Slow Cooker, No-Cook
- **Time:** Quick (under 30 min), Easy, Make-Ahead

---

## AI Integration

### Provider: Azure AI Foundry (Azure OpenAI — GPT-4o)

All AI calls are made server-side from the Import Service. The client never has direct access to the AI endpoint.

### Recipe Extraction Prompt Strategy

**Input:** Raw HTML content of the recipe page (stripped of `<script>`, `<style>`, and navigation boilerplate, keeping visible text and `<img>` `src` attributes).

**System Prompt:**

```
You are a recipe extraction assistant. Given raw web page text, 
extract the recipe into the provided JSON schema. 
- Normalize ingredient quantities to metric units where possible.
- Standardize instruction steps into clear, numbered sentences.
- Infer missing fields where reasonable.
- Return ONLY valid JSON matching the schema, no prose.
```

**User Prompt:** `<stripped page text>`

**Expected Output:** JSON conforming to the Recipe schema above (excluding `id`, `userId`, `imageUrls`, `rating`, `importedAt`).

### Tagging Prompt Strategy

Tags are generated in the same AI call by including a `tags` field in the output schema and a few-shot example in the system prompt showing diverse tag sets.

### Fallback & Error Handling

| Scenario | Handling |
|----------|---------|
| AI response is not valid JSON | Retry once with a corrective prompt; if still invalid, store raw extracted text and flag recipe as `needsReview: true` |
| AI times out | Return partial recipe with title only; mark `importStatus: "partial"` |
| Page scraping blocked (403/bot detection) | Return error to user: "This site could not be scraped. Try pasting the recipe text manually." |
| Image download fails | Store recipe without image; `imageUrls: []` |

---

## Frontend Design

### Theme & Design Language

| Token | Value |
|-------|-------|
| Primary | `#7C3AED` (Purple 600) |
| Primary Dark | `#5B21B6` (Purple 800) |
| Secondary | `#2563EB` (Blue 600) |
| Secondary Light | `#DBEAFE` (Blue 100) |
| Background | `#0F0F1A` (Near-black) — dark mode default |
| Surface | `#1E1E2E` |
| Text Primary | `#F8F8FC` |
| Text Secondary | `#A0A0C0` |
| Accent Gradient | `linear-gradient(135deg, #7C3AED, #2563EB)` |
| Border Radius | `12px` (cards), `8px` (inputs) |
| Font | Inter (sans-serif) |

Both **dark mode** (default) and **light mode** (optional) will be supported via Tailwind's `dark:` variant.

### Pages & Routes

| Route | Page | Description |
|-------|------|-------------|
| `/` | Home / Dashboard | Hero CTA + recent recipes grid |
| `/import` | Import Recipe | URL input form + import status |
| `/recipes` | Recipe Library | Filterable, searchable card grid |
| `/recipes/:id` | Recipe Detail | Full recipe view with rating |
| `/recipes/:id/edit` | Edit Recipe | Manual edit of AI-extracted data |
| `/profile` | User Profile | Account info |
| `/login` | Login | Redirect to Entra; MSAL redirect handler |

### Key UI Components

#### Home / Dashboard

- **Hero section:** Gradient headline ("Your personal recipe library, powered by AI"), URL import input bar front and center, "Import Recipe" CTA button.
- **Recent Recipes grid:** Last 6 imported recipes as cards.
- **Tag Cloud:** Most-used tags as clickable chips → navigates to filtered `/recipes` view.

#### Recipe Card

```
┌────────────────────────────────────┐
│  [Recipe Image — 16:9 aspect]      │
├────────────────────────────────────┤
│  Classic Spaghetti Carbonara       │
│  ⭐⭐⭐⭐☆                           │
│  🕐 35 min  👤 4 servings          │
│  [Italian] [Pasta] [Quick]         │
└────────────────────────────────────┘
```

- Image with purple/blue gradient overlay on hover.
- Tag chips in brand colors.
- Star rating rendered inline.

#### Recipe Library

- **Top bar:** Search input + multi-select tag filter (checkbox chips).
- **Grid layout:** 3 columns desktop, 2 tablet, 1 mobile.
- **Empty state:** Illustration + "Import your first recipe" CTA.

#### Recipe Detail

- Hero image (full-width, 40vh height with gradient overlay).
- Title, rating control (interactive stars), meta row (time, servings).
- Two-column layout: Ingredients (left) | Instructions (right).
- Tag chips.
- Source URL link.
- Edit and Delete action buttons.

#### Import Flow

1. User enters URL and clicks **Import**.
2. Loading state: animated gradient progress bar + status messages ("Fetching page…", "Extracting recipe with AI…", "Saving…").
3. On success: navigate to the new Recipe Detail page.
4. On error: inline error message with guidance.

---

## API Design

### Base URL

`https://api.recipeboss.example.com/api/v1`

All endpoints require `Authorization: Bearer <token>` unless marked public.

### Endpoints

#### Recipes

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/recipes` | List user's recipes. Supports `?tags=Italian,Pasta&q=carbonara&page=1&pageSize=20` |
| `POST` | `/recipes/import` | Import recipe from URL |
| `GET` | `/recipes/{id}` | Get single recipe |
| `PUT` | `/recipes/{id}` | Update recipe (manual edits) |
| `DELETE` | `/recipes/{id}` | Delete recipe |
| `PATCH` | `/recipes/{id}/rating` | Set or update rating |
| `GET` | `/recipes/tags` | Get distinct tags for current user |

#### Import

**`POST /recipes/import`**

Request:
```json
{
  "url": "https://www.seriouseats.com/spaghetti-carbonara-recipe"
}
```

Response (202 Accepted → poll or SSE):
```json
{
  "importId": "uuid",
  "status": "processing"
}
```

The import is asynchronous. The frontend polls `GET /recipes/import/{importId}` or receives a Server-Sent Event on completion.

**`GET /recipes/import/{importId}`**

```json
{
  "importId": "uuid",
  "status": "completed",
  "recipeId": "recipe-uuid"
}
```

Possible statuses: `processing` | `completed` | `partial` | `failed`

#### Ratings

**`PATCH /recipes/{id}/rating`**

Request:
```json
{
  "rating": 4,
  "notes": "Great weeknight dinner!"
}
```

Response: `200 OK` with updated recipe document.

#### Tags

**`GET /recipes/tags`**

Response:
```json
{
  "tags": ["Italian", "Pasta", "Vegetarian", "Quick", "Baked"]
}
```

### Error Response Schema

```json
{
  "error": {
    "code": "IMPORT_FAILED",
    "message": "Could not scrape the provided URL.",
    "details": "HTTP 403 returned by target site."
  }
}
```

---

## Image Handling

### Import Flow

1. After AI extraction, the Import Service parses `<img>` tags from the original HTML.
2. Images are scored by size (prefer large images) and position (prefer images in recipe/article content areas). Top 3 are downloaded.
3. Images are stored in Azure Blob Storage under the path: `recipes/{userId}/{recipeId}/{n}.jpg`.
4. Blob container is private; access is via **SAS tokens** with 1-hour expiry, generated by the API on each recipe read request.
5. `imageUrls` on the recipe document stores the blob path (not the SAS URL); SAS URLs are generated at read time.

### Image Processing

- Images are resized server-side to a maximum of **1200×900px** and converted to JPEG (quality 85) using `SkiaSharp` to reduce storage costs.
- A 400×300px thumbnail is also generated for card views.

---

## Deployment Architecture

### .NET Aspire Application Model

```
AppHost (RecipeBoss.AppHost)
├── recipeboss-api          → ASP.NET Core Web API (Container App)
├── recipeboss-web          → React SPA (Static Web App or Container App)
├── cosmos-db               → Azure Cosmos DB for NoSQL (Aspire resource)
├── blob-storage            → Azure Blob Storage (Aspire resource)
└── azure-openai            → Azure OpenAI connection (Aspire resource)
```

### Azure Resources

| Resource | SKU / Tier | Notes |
|----------|-----------|-------|
| Azure Container Apps Environment | Consumption | Scales to zero when idle |
| Azure Container Apps (API) | Consumption | Min replicas: 0 |
| Azure Static Web Apps | Free tier | React SPA |
| Azure Cosmos DB | Serverless | Pay-per-operation; cost-efficient at low volume |
| Azure Blob Storage | LRS Hot | Recipe images |
| Azure AI Foundry (OpenAI) | Pay-as-you-go (GPT-4o) | Per-token billing |
| Azure Key Vault | Standard | Secrets for API keys, connection strings |
| Entra External ID | Free tier (50,000 MAU free) | Auth |
| Azure Container Registry | Basic | Docker images for API |

### Environment Strategy

| Environment | Trigger | Notes |
|------------|---------|-------|
| Local Dev | `dotnet run --project AppHost` | Aspire dashboard at `localhost:15888` |
| Staging | PR merge to `main` | GitHub Actions → `azd up` |
| Production | Manual or tag-triggered | Same `azd up`, different parameter file |

### CI/CD Pipeline (GitHub Actions)

```yaml
# Simplified
on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    steps:
      - Build API Docker image and push to ACR
      - Build React SPA (npm run build)
      - Run tests
      - azd up --environment staging
```

---

## Cost & Infrastructure Considerations

### Cost Optimization Strategies

| Area | Strategy |
|------|---------|
| Container Apps | Scale-to-zero on inactivity (consumption plan) |
| Cosmos DB | Serverless mode — no RU provisioning cost at idle |
| OpenAI | Cache AI responses keyed by URL hash; avoid re-importing the same URL |
| Blob Storage | LRS replication is sufficient; no geo-redundancy needed in v1 |
| Entra External ID | Free tier covers 50,000 MAU — more than sufficient |

### Estimated Monthly Cost (10 active users, ~50 recipes/month)

| Service | Estimated Cost |
|---------|---------------|
| Container Apps (API) | ~$0–5 (scale-to-zero, low traffic) |
| Cosmos DB Serverless | ~$1–3 |
| Blob Storage | ~$0.10 |
| Azure OpenAI (GPT-4o) | ~$2–10 (depends on page size) |
| Entra External ID | $0 (free tier) |
| Static Web Apps | $0 (free tier) |
| **Total** | **~$3–20/month** |

---

## Security Considerations

| Area | Control |
|------|---------|
| Authentication | Entra External ID JWT; short-lived access tokens |
| API Authorization | `[Authorize]` on all endpoints; user ID extracted from token claim |
| Data Isolation | Cosmos DB partition key = `userId`; all queries scoped to current user |
| Secrets | Stored in Azure Key Vault; never in code or environment files |
| CORS | API allows only the known SPA origin |
| Input Validation | URL must pass `Uri.IsWellFormedUriString` + allowlist of HTTP/HTTPS |
| SSRF Prevention | The Import Service only fetches `http://` and `https://` URLs; private/internal IP ranges are blocked (RFC1918 check) |
| Image URLs | Served via time-limited SAS tokens; blob container is not publicly accessible |
| Content Security Policy | Set on SPA to prevent XSS |
| Rate Limiting | API-level rate limiting on `/recipes/import` to prevent AI cost abuse |

---

## Open Questions

| # | Question | Stakeholder |
|---|----------|------------|
| OQ1 | Should users be able to share recipes with other users, or is this strictly personal? | Product |
| OQ2 | Should ratings be 1–5 stars, or a simpler thumbs up/down? | UX |
| OQ3 | Do we want light mode as the default, or dark mode? | UX |
| OQ4 | Should the app support multiple images per recipe, or only a hero image? | Product |
| OQ5 | Do we need manual recipe entry (no URL) in v1? | Product |
| OQ6 | What is the acceptable latency for recipe import? (AI + scraping can take 10–30 seconds) | Product |
| OQ7 | Should tags be editable by the user after AI assignment? | Product |
| OQ8 | Do we want to support importing from common recipe sites via their structured data (`schema.org/Recipe`) as a fast path before falling back to AI? | Engineering |
| OQ9 | Which social login providers should Entra External ID expose (Google, Microsoft, Apple)? | Product |
| OQ10 | What is the data retention policy if a user deletes their account? | Legal/Product |

---

*Document prepared by GitHub Copilot Coding Agent · April 2026*
