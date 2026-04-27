# Decisions

**Last Updated:** 2026-04-27T10:32:56Z

## Active Decisions

### Cosmos DB Emulator — vnext-preview Tag for ARM64 Compatibility

**Date:** 2026-04-27  
**Author:** Book (DevOps)  
**Branch:** dev/cosmos-db

#### Decision

Replace the previous conditional Cosmos DB connection-string check in RecipeBoss.AppHost/Program.cs with an unconditional RunAsEmulator call using the next-preview image tag.

`csharp
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator(emulator => emulator
        .WithImageTag("vnext-preview"));
`

#### Context

The original mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator image is x86-only and fails on ARM64 hosts (Apple Silicon, AWS Graviton). A prior workaround skipped the emulator entirely and fell back to InMemoryRecipeRepository when no ConnectionStrings:cosmos user secret was set.

Microsoft has released a new multi-architecture emulator under the next-preview tag (mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview) that supports both linux/amd64 and linux/arm64/v8.

#### Rationale

- **Removes the InMemory fallback gap:** All developers now run with the full Cosmos emulator regardless of architecture.
- **No conditional logic needed:** Aspire's RunAsEmulator is silently ignored in publish/deploy mode; real Azure Cosmos DB is provisioned from the Aspire manifest instead.
- **Simpler AppHost:** Removes the nullable cosmos variable and the if-guards around WithReference/WaitFor.
- **ARM64 safe:** The next-preview tag is multi-arch, so CI on ARM64 runners and developer machines on Apple Silicon both work.

#### Impact

- ppsettings.Development.json in AppHost: removed the empty ConnectionStrings:cosmos stub (no longer needed); updated note to explain the emulator-first approach.
- API InMemoryRecipeRepository remains as the implementation — Cosmos integration is a separate backlog item (Zoe owns).
- The WaitFor(cosmos) ensures the API does not start until the emulator is healthy.
