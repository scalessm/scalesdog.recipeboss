var builder = DistributedApplication.CreateBuilder(args);

// Add the RecipeBoss API
var api = builder.AddProject<Projects.RecipeBoss_Api>("recipeboss-api");

// Add the React frontend as a Vite dev server (local dev only)
builder.AddNpmApp("frontend", "../frontend", scriptName: "dev")
    .WithReference(api)
    .WithHttpEndpoint(env: "VITE_PORT", port: 5173)
    .ExcludeFromManifest(); // not deployed as Aspire resource — goes to Static Web Apps

builder.Build().Run();
