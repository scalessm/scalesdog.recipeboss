using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// Repositories
builder.Services.AddSingleton<IRecipeRepository, InMemoryRecipeRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapRecipeEndpoints();

app.Run();

// Exposed for WebApplicationFactory in integration tests
public partial class Program { }