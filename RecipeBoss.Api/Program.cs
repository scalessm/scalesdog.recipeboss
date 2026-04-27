using Microsoft.Identity.Web;
using RecipeBoss.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Auth — Microsoft Entra External ID
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// OpenAPI
builder.Services.AddOpenApi();

// CORS — allow frontend origin (configure via config in production)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapRecipeEndpoints();

app.Run();