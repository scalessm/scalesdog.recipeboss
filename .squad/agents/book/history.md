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
