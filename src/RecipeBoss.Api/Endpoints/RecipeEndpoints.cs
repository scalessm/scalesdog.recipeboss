using RecipeBoss.Api.Infrastructure;
using RecipeBoss.Api.Models;

namespace RecipeBoss.Api.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/recipes")
            .RequireAuthorization();

        group.MapGet("/", GetRecipesAsync)
            .WithName("GetRecipes")
            .WithSummary("Get all recipes for the authenticated user");

        group.MapGet("/tags", GetTagsAsync)
            .WithName("GetRecipeTags")
            .WithSummary("Get all distinct tags for the authenticated user's recipes");

        group.MapGet("/{id}", (string id) => Results.NotFound())
            .WithName("GetRecipe")
            .WithSummary("Get a specific recipe by ID");

        group.MapPost("/import", () => Results.Accepted())
            .WithName("ImportRecipe")
            .WithSummary("Import a recipe from a URL");

        group.MapPut("/{id}", (string id) => Results.NoContent())
            .WithName("UpdateRecipe")
            .WithSummary("Update recipe metadata");

        group.MapDelete("/{id}", (string id) => Results.NoContent())
            .WithName("DeleteRecipe")
            .WithSummary("Delete a recipe");

        group.MapPut("/{id}/rating", (string id) => Results.NoContent())
            .WithName("RateRecipe")
            .WithSummary("Rate a recipe 1-5 stars");
    }

    private static async Task<IResult> GetRecipesAsync(
        HttpContext context,
        IRecipeRepository repository,
        string? q = null,
        string? tags = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = context.User.FindFirst("oid")?.Value
                  ?? context.User.FindFirst("sub")?.Value
                  ?? throw new UnauthorizedAccessException("No user identity claim found.");

        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var parsedTags = string.IsNullOrWhiteSpace(tags)
            ? null
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var recipes = await repository.GetRecipesAsync(userId, q, parsedTags, page, pageSize, ct);
        var summaries = recipes.Select(ToSummary);
        return Results.Ok(summaries);
    }

    private static async Task<IResult> GetTagsAsync(
        HttpContext context,
        IRecipeRepository repository,
        CancellationToken ct = default)
    {
        var userId = context.User.FindFirst("oid")?.Value
                  ?? context.User.FindFirst("sub")?.Value
                  ?? throw new UnauthorizedAccessException("No user identity claim found.");

        var tags = await repository.GetTagsAsync(userId, ct);
        return Results.Ok(new { tags });
    }

    private static RecipeSummary ToSummary(Recipe r) => new(
        r.Id,
        r.Title,
        r.Description,
        r.Servings,
        r.PrepTimeMinutes,
        r.CookTimeMinutes,
        r.Tags,
        r.ImageUrls,
        r.Rating,
        r.ImportedAt
    );
}
