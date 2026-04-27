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
using Xunit;

// Integration smoke tests for the recipe endpoints using a real test host.
//
// KNOWN GAPS (as of 2026-04-27):
//   1. Route mismatch: current implementation registers /api/recipes; design requires /api/v1/recipes.
//      All 2xx/4xx tests are skipped until Zoe corrects the route prefix.
//   2. IRecipeRepository / InMemoryRecipeRepository do not yet exist.
//      Tests that replace the real repository with a seeded in-memory fake are skipped.
//
// When Zoe's implementation lands:
//   a. Remove Skip from all tests.
//   b. Uncomment the IRecipeRepository replacement in RecipeApiFactory.ConfigureWebHost.
//   c. Verify all tests pass before merging.

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
            // Replace auth with test scheme (always succeeds when Authorization header present)
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            // TODO (pending Zoe's implementation): replace IRecipeRepository with InMemoryRecipeRepository
            // services.RemoveAll<IRecipeRepository>();
            // services.AddSingleton<IRecipeRepository, InMemoryRecipeRepository>();
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

    [Fact(Skip = "Pending Zoe's implementation: route /api/v1/recipes not yet registered (currently /api/recipes); IRecipeRepository not yet available")]
    public async Task GetRecipes_Authenticated_Returns200WithList()
    {
        _client.DefaultRequestHeaders.Authorization = BearerToken();
        var response = await _client.GetAsync("/api/v1/recipes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Pending Zoe's implementation: route /api/v1/recipes not yet registered (currently /api/recipes)")]
    public async Task GetRecipes_NoAuthToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/recipes");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "Pending Zoe's implementation: route /api/v1/recipes not yet registered; IRecipeRepository / query filtering not yet available")]
    public async Task GetRecipes_WithQFilter_ReturnsFilteredResults()
    {
        _client.DefaultRequestHeaders.Authorization = BearerToken();
        var response = await _client.GetAsync("/api/v1/recipes?q=carbonara");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Assert: response body contains only recipes matching "carbonara"
    }

    [Fact(Skip = "Pending Zoe's implementation: route /api/v1/recipes not yet registered; IRecipeRepository / tag filtering not yet available")]
    public async Task GetRecipes_WithTagsFilter_ReturnsAndFilteredResults()
    {
        _client.DefaultRequestHeaders.Authorization = BearerToken();
        var response = await _client.GetAsync("/api/v1/recipes?tags=Italian,Pasta");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Assert: every recipe in response has both "Italian" AND "Pasta" tags
    }

    // ── GET /api/v1/recipes/tags ──────────────────────────────────────────────

    [Fact(Skip = "Pending Zoe's implementation: /api/v1/recipes/tags endpoint not yet registered; IRecipeRepository not yet available")]
    public async Task GetTags_Authenticated_Returns200WithTagList()
    {
        _client.DefaultRequestHeaders.Authorization = BearerToken();
        var response = await _client.GetAsync("/api/v1/recipes/tags");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Pending Zoe's implementation: /api/v1/recipes/tags endpoint not yet registered")]
    public async Task GetTags_NoAuthToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/recipes/tags");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
