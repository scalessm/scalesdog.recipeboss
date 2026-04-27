var builder = DistributedApplication.CreateBuilder(args);

// Cosmos DB — always run the vnext-preview emulator locally (supports linux/amd64 and linux/arm64).
// In publish/deploy mode Aspire provisions a real Azure Cosmos DB and ignores RunAsEmulator.
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator(emulator => emulator
        .WithImageTag("vnext-preview"));

// Add the RecipeBoss API
var api = builder.AddProject<Projects.RecipeBoss_Api>("recipeboss-api");
api.WithReference(cosmos).WaitFor(cosmos);

// Add the React frontend as a Vite dev server (local dev only)
builder.AddNpmApp("frontend", "../frontend", scriptName: "dev")
    .WithReference(api)
    .WithHttpEndpoint(env: "VITE_PORT", port: 5173)
    .ExcludeFromManifest(); // not deployed as Aspire resource — goes to Static Web Apps

builder.Build().Run();
