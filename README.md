# RecipeBoss

A modern web application that lets authenticated users import, store, rate, and browse recipes. Paste any recipe URL and RecipeBoss scrapes the page, uses Azure AI Foundry (GPT-4o) to extract and normalize the recipe, applies AI-generated tags, downloads images, and stores everything in Cosmos DB.

---

## Solution Structure

```
scalesdog.recipeboss/
├── RecipeBoss.sln                  # Visual Studio solution (all .NET projects)
├── RecipeBoss.Api/                 # ASP.NET Core 10 minimal API backend
├── RecipeBoss.AppHost/             # .NET Aspire AppHost (local dev + deployment orchestration)
├── RecipeBoss.ServiceDefaults/     # Shared Aspire service defaults (telemetry, health checks)
├── recipeboss-frontend/            # React 18 + TypeScript + Vite + shadcn/ui frontend
├── Directory.Build.props           # Shared .NET build settings
├── global.json                     # .NET SDK version pin
├── .editorconfig                   # Editor formatting rules (C# + TypeScript)
└── DESIGN.md                       # Full product design document
```

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript, Vite, shadcn/ui, Tailwind CSS |
| Backend | ASP.NET Core 10 minimal APIs |
| AI | Azure AI Foundry (GPT-4o) — recipe extraction & tagging |
| Database | Azure Cosmos DB for NoSQL (per-user partition) |
| Blob Storage | Azure Blob Storage (recipe images) |
| Auth | Microsoft Entra External ID (CIAM), MSAL React, Microsoft.Identity.Web |
| Orchestration | .NET Aspire (AppHost + ServiceDefaults) |
| Hosting | Azure Container Apps (API), Azure Static Web Apps (SPA) |
| Secrets | Azure Key Vault |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/dotnet-aspire-sdk): `dotnet workload install aspire`
- Azure subscription (for Cosmos DB, Blob Storage, Entra External ID, AI Foundry)

---

## Getting Started (Local Dev)

### 1. Restore dependencies

```bash
dotnet restore RecipeBoss.sln
cd recipeboss-frontend && npm install
```

### 2. Configure secrets

Set user secrets or environment variables for:
- `AzureAd:TenantId`, `AzureAd:ClientId` — Entra External ID app registration
- `ConnectionStrings:CosmosDb` — Cosmos DB connection string
- `ConnectionStrings:BlobStorage` — Azure Blob Storage connection string
- `AzureAiFoundry:Endpoint`, `AzureAiFoundry:ApiKey` — AI Foundry endpoint

### 3. Run via Aspire

```bash
dotnet run --project RecipeBoss.AppHost
```

The Aspire dashboard opens at `https://localhost:15888` showing all running services.

### 4. Run the frontend (standalone)

```bash
cd recipeboss-frontend
npm run dev
```

---

## Project Descriptions

### `RecipeBoss.Api`
ASP.NET Core 10 minimal API. Handles recipe import, storage, retrieval, and rating endpoints. Validates JWTs from Entra External ID. Talks to Cosmos DB, Blob Storage, and Azure AI Foundry.

### `RecipeBoss.AppHost`
.NET Aspire AppHost that wires together the API and frontend for local development, manages service discovery, and produces the Azure deployment manifest.

### `RecipeBoss.ServiceDefaults`
Shared library referenced by all .NET projects. Configures OpenTelemetry, health checks, and service defaults following Aspire conventions.

### `recipeboss-frontend`
React 18 SPA using Vite, TypeScript, shadcn/ui, and Tailwind CSS. Authenticates via MSAL React against Entra External ID and calls the RecipeBoss API.

---

## Architecture Overview

See [DESIGN.md](./DESIGN.md) for the full design document including data models, API contract, AI integration details, and deployment architecture.