namespace RecipeBoss.Api.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes")
            .RequireAuthorization();

        group.MapGet("/", () => Results.Ok(new List<object>()))
            .WithName("GetRecipes")
            .WithSummary("Get all recipes for the authenticated user");

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
}
