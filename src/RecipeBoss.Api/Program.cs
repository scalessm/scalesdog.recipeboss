using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using RecipeBoss.Api.Endpoints;
using RecipeBoss.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// CORS — named dev policy covering both Vite default ports
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Auth — Microsoft Entra External ID via JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

// OpenAPI
builder.Services.AddOpenApi();

// Cosmos DB
var cosmosConnectionString = builder.Configuration.GetConnectionString("cosmos");
if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    builder.Services.AddSingleton(new CosmosClient(cosmosConnectionString, new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        },
        AllowBulkExecution = true
    }));
    builder.Services.AddSingleton<IRecipeRepository, CosmosRecipeRepository>();
}
else
{
    // Fall back to in-memory for environments where Cosmos is not configured
    builder.Services.AddSingleton<IRecipeRepository, InMemoryRecipeRepository>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapRecipeEndpoints();

// Seed database on startup in development
if (app.Environment.IsDevelopment())
{
    var cosmosClient = app.Services.GetService<CosmosClient>();
    if (cosmosClient is not null)
    {
        var seedLogger = app.Services.GetRequiredService<ILogger<Program>>();
        var seedUserOid = app.Configuration["SeedData:UserOid"] ?? "";
        if (!string.IsNullOrEmpty(seedUserOid))
            await RecipeSeeder.SeedAsync(cosmosClient, seedUserOid, seedLogger);
    }
}

app.Run();

// Exposed for WebApplicationFactory in integration tests
public partial class Program { }