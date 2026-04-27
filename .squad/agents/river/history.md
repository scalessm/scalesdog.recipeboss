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
