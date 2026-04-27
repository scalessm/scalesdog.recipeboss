var builder = DistributedApplication.CreateBuilder(args);

// Cosmos DB — use real Azure connection string if configured in user secrets,
// otherwise the API falls back to InMemoryRecipeRepository automatically.
var cosmosConnectionString = builder.Configuration["ConnectionStrings:cosmos"];
var hasCosmosConnectionString = !string.IsNullOrEmpty(cosmosConnectionString);

IResourceBuilder<AzureCosmosDBResource>? cosmos = null;
if (hasCosmosConnectionString)
{
    cosmos = builder.AddAzureCosmosDB("cosmos");
}

// Add the RecipeBoss API
var api = builder.AddProject<Projects.RecipeBoss_Api>("recipeboss-api");
if (cosmos != null)
{
    api.WithReference(cosmos).WaitFor(cosmos);
}

// Add the React frontend as a Vite dev server (local dev only)
builder.AddNpmApp("frontend", "../frontend", scriptName: "dev")
    .WithReference(api)
    .WithHttpEndpoint(env: "VITE_PORT", port: 5173)
    .ExcludeFromManifest(); // not deployed as Aspire resource — goes to Static Web Apps

builder.Build().Run();
