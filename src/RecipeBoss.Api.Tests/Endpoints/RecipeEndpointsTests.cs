using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeBoss.Api.Infrastructure;
using Xunit;

namespace RecipeBoss.Api.Tests.Endpoints;

// ── Test auth scheme ─────────────────────────────────────────────────────────

/// <summary>
/// Authentication handler that always succeeds with a fixed userId claim.
/// Replaces Microsoft.Identity.Web in the test host.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string TestUserId = "test-user-001";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("No Authorization header"));

        var claims = new[]
        {
            new Claim("oid", TestUserId),
            new Claim(ClaimTypes.NameIdentifier, TestUserId),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// ── WebApplicationFactory ─────────────────────────────────────────────────────

public class RecipeApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRecipeRepository));
            if (descriptor != null)
                services.Remove(descriptor);
            services.AddSingleton<IRecipeRepository, InMemoryRecipeRepository>();
        });

        builder.UseEnvironment("Development");
    }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public class RecipeEndpointsTests : IClassFixture<RecipeApiFactory>
{
    private readonly HttpClient _client;

    public RecipeEndpointsTests(RecipeApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static AuthenticationHeaderValue BearerToken()
        => new("Bearer", "test-token");

    // ── GET /api/v1/recipes ───────────────────────────────────────────────────

    [Fact]
    public async Task GetRecipes_Authenticated_Returns200WithList()
    {
        _client.DefaultRequestHeaders.Authorization = BearerToken();
        var response = await _client.GetAsync("/api/v1/recipes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRecipes_NoAuthToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/recipes");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRecipes_WithQFilter_ReturnsFilteredResults()
    {
        _client.DefaultRequestHeaders.Authorization = BearerToken();
        var response = await _client.GetAsync("/api/v1/recipes?q=carbonara");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRecipes_WithTagsFilter_ReturnsAndFilteredResults()
    {
        _client.DefaultRequestHeaders.Authorization = BearerToken();
        var response = await _client.GetAsync("/api/v1/recipes?tags=Italian,Pasta");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── GET /api/v1/recipes/tags ──────────────────────────────────────────────

    [Fact]
    public async Task GetTags_Authenticated_Returns200WithTagList()
    {
        _client.DefaultRequestHeaders.Authorization = BearerToken();
        var response = await _client.GetAsync("/api/v1/recipes/tags");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTags_NoAuthToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/recipes/tags");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
