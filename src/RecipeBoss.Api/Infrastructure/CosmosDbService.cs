using RecipeBoss.Api.Models;

namespace RecipeBoss.Api.Infrastructure;

public interface IRecipeRepository
{
    Task<IEnumerable<Recipe>> GetRecipesAsync(string userId, string? query, IEnumerable<string>? tags, int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<string>> GetTagsAsync(string userId, CancellationToken ct = default);
    Task<Recipe?> GetRecipeAsync(string userId, string recipeId, CancellationToken ct = default);
    Task<Recipe> UpsertRecipeAsync(Recipe recipe, CancellationToken ct = default);
    Task DeleteRecipeAsync(string userId, string recipeId, CancellationToken ct = default);
}

/// <summary>
/// In-memory repository seeded with sample data for development before Cosmos DB is provisioned.
/// </summary>
public class InMemoryRecipeRepository : IRecipeRepository
{
    private static readonly Dictionary<string, Recipe> _store = new(
        SeedData().Select(r => KeyValuePair.Create(r.Id, r)));

    private static IEnumerable<Recipe> SeedData()
    {
        const string devUser = "dev-user-001";
        return
        [
            new Recipe
            {
                Id = "recipe-001",
                UserId = devUser,
                SourceUrl = "https://example.com/spaghetti-carbonara",
                Title = "Spaghetti Carbonara",
                Description = "Classic Roman pasta with eggs, Pecorino Romano, guanciale, and black pepper.",
                Servings = 4,
                PrepTimeMinutes = 10,
                CookTimeMinutes = 20,
                Tags = ["Italian", "Pasta", "Quick"],
                ImageUrls = ["https://example.com/images/carbonara.jpg"],
                Rating = new RecipeRating(5, DateTimeOffset.UtcNow.AddDays(-3), "Perfect every time."),
                Ingredients =
                [
                    new("400g", "spaghetti"),
                    new("200g", "guanciale"),
                    new("4 large", "eggs"),
                    new("100g", "Pecorino Romano"),
                    new("to taste", "black pepper")
                ],
                Instructions =
                [
                    "Boil salted water and cook spaghetti until al dente.",
                    "Render guanciale in a pan until crisp, then remove from heat.",
                    "Whisk eggs with grated Pecorino and plenty of black pepper.",
                    "Combine hot pasta with guanciale fat, then stir in egg mixture off heat.",
                    "Serve immediately with extra Pecorino."
                ]
            },
            new Recipe
            {
                Id = "recipe-002",
                UserId = devUser,
                SourceUrl = "https://example.com/chicken-tikka-masala",
                Title = "Chicken Tikka Masala",
                Description = "Tender marinated chicken in a rich, spiced tomato-cream sauce.",
                Servings = 6,
                PrepTimeMinutes = 30,
                CookTimeMinutes = 40,
                Tags = ["Indian", "Chicken", "Spicy"],
                ImageUrls = ["https://example.com/images/tikka-masala.jpg"],
                Rating = new RecipeRating(4, DateTimeOffset.UtcNow.AddDays(-7), "Great flavour, reduce chilli next time."),
                Ingredients =
                [
                    new("700g", "chicken breast"),
                    new("200ml", "plain yoghurt"),
                    new("400ml", "coconut cream"),
                    new("400g tin", "chopped tomatoes"),
                    new("2 tbsp", "tikka masala paste")
                ],
                Instructions =
                [
                    "Marinate chicken in yoghurt and spices for at least 1 hour.",
                    "Grill or broil chicken until slightly charred.",
                    "Fry onions and tikka paste in oil until fragrant.",
                    "Add tomatoes and coconut cream; simmer 20 minutes.",
                    "Add chicken and simmer a further 10 minutes. Serve with rice."
                ]
            },
            new Recipe
            {
                Id = "recipe-003",
                UserId = devUser,
                SourceUrl = "https://example.com/tacos-al-pastor",
                Title = "Tacos al Pastor",
                Description = "Street-style marinated pork tacos with pineapple and fresh salsa.",
                Servings = 8,
                PrepTimeMinutes = 20,
                CookTimeMinutes = 25,
                Tags = ["Mexican", "Pork", "Street Food"],
                ImageUrls = ["https://example.com/images/al-pastor.jpg"],
                Rating = new RecipeRating(5, DateTimeOffset.UtcNow.AddDays(-1), "Crowd pleaser."),
                Ingredients =
                [
                    new("1kg", "pork shoulder, thinly sliced"),
                    new("200g", "pineapple chunks"),
                    new("3", "guajillo chillies"),
                    new("16", "small corn tortillas"),
                    new("1 bunch", "fresh coriander")
                ],
                Instructions =
                [
                    "Blend guajillo chillies, achiote, and spices into a marinade.",
                    "Marinate pork overnight in the fridge.",
                    "Cook pork on a hot griddle until caramelised.",
                    "Warm tortillas and top with pork, pineapple, onion, and coriander.",
                    "Serve with lime wedges and salsa verde."
                ]
            },
            new Recipe
            {
                Id = "recipe-004",
                UserId = devUser,
                SourceUrl = "https://example.com/japanese-ramen",
                Title = "Tonkotsu Ramen",
                Description = "Rich pork bone broth ramen with chashu pork, soft-boiled egg, and nori.",
                Servings = 2,
                PrepTimeMinutes = 45,
                CookTimeMinutes = 240,
                Tags = ["Japanese", "Soup", "Pork"],
                ImageUrls = ["https://example.com/images/ramen.jpg"],
                Rating = null,
                Ingredients =
                [
                    new("1kg", "pork bones"),
                    new("200g", "pork belly for chashu"),
                    new("2 portions", "ramen noodles"),
                    new("2", "soft-boiled eggs"),
                    new("4 sheets", "nori")
                ],
                Instructions =
                [
                    "Blanch pork bones, then simmer 4 hours for broth.",
                    "Roll and braise pork belly for chashu.",
                    "Marinate soft-boiled eggs in soy and mirin.",
                    "Cook noodles and divide into bowls.",
                    "Ladle hot broth, top with chashu, egg, nori, and spring onion."
                ]
            }
        ];
    }

    public Task<IEnumerable<Recipe>> GetRecipesAsync(
        string userId, string? query, IEnumerable<string>? tags, int page, int pageSize, CancellationToken ct = default)
    {
        var results = _store.Values.Where(r => r.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            results = results.Where(r =>
                r.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (r.Description != null && r.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        if (tags != null)
        {
            var tagList = tags.ToList();
            if (tagList.Count > 0)
                results = results.Where(r => tagList.All(t => r.Tags.Contains(t, StringComparer.OrdinalIgnoreCase)));
        }

        var paged = results
            .OrderByDescending(r => r.ImportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Task.FromResult(paged);
    }

    public Task<IEnumerable<string>> GetTagsAsync(string userId, CancellationToken ct = default)
    {
        var tags = _store.Values
            .Where(r => r.UserId == userId)
            .SelectMany(r => r.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t);

        return Task.FromResult<IEnumerable<string>>(tags);
    }

    public Task<Recipe?> GetRecipeAsync(string userId, string recipeId, CancellationToken ct = default)
    {
        _store.TryGetValue(recipeId, out var recipe);
        var result = recipe?.UserId == userId ? recipe : null;
        return Task.FromResult(result);
    }

    public Task<Recipe> UpsertRecipeAsync(Recipe recipe, CancellationToken ct = default)
    {
        _store[recipe.Id] = recipe;
        return Task.FromResult(recipe);
    }

    public Task DeleteRecipeAsync(string userId, string recipeId, CancellationToken ct = default)
    {
        if (_store.TryGetValue(recipeId, out var recipe) && recipe.UserId == userId)
            _store.Remove(recipeId);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Cosmos DB repository stub — to be implemented once Cosmos DB is provisioned.
/// Reads connection string from IConfiguration["CosmosDb:ConnectionString"].
/// Partition key strategy: /userId for per-user data isolation.
/// </summary>
public class CosmosRecipeRepository : IRecipeRepository
{
    private readonly IConfiguration _configuration;

    public CosmosRecipeRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        // TODO: Initialise CosmosClient using _configuration["CosmosDb:ConnectionString"]
        // TODO: Get container reference: database "recipeboss", container "recipes"
    }

    public Task<IEnumerable<Recipe>> GetRecipesAsync(
        string userId, string? query, IEnumerable<string>? tags, int page, int pageSize, CancellationToken ct = default)
    {
        // TODO: Query Cosmos with LINQ or SQL API scoped to userId partition
        throw new NotImplementedException("CosmosRecipeRepository is not yet implemented.");
    }

    public Task<IEnumerable<string>> GetTagsAsync(string userId, CancellationToken ct = default)
    {
        // TODO: Query distinct tags from Cosmos for userId partition
        throw new NotImplementedException("CosmosRecipeRepository is not yet implemented.");
    }

    public Task<Recipe?> GetRecipeAsync(string userId, string recipeId, CancellationToken ct = default)
    {
        // TODO: Point-read by (recipeId, userId) partition key
        throw new NotImplementedException("CosmosRecipeRepository is not yet implemented.");
    }

    public Task<Recipe> UpsertRecipeAsync(Recipe recipe, CancellationToken ct = default)
    {
        // TODO: UpsertItemAsync into Cosmos container partitioned by userId
        throw new NotImplementedException("CosmosRecipeRepository is not yet implemented.");
    }

    public Task DeleteRecipeAsync(string userId, string recipeId, CancellationToken ct = default)
    {
        // TODO: DeleteItemAsync by (recipeId, userId) partition key
        throw new NotImplementedException("CosmosRecipeRepository is not yet implemented.");
    }
}
