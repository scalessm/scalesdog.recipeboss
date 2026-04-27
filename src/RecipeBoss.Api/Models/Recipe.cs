namespace RecipeBoss.Api.Models;

public record Ingredient(string Quantity, string Item);

public record RecipeRating(int Rating, DateTimeOffset RatedAt, string? Notes);

public record Recipe
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string UserId { get; init; } = string.Empty;
    public string? SourceUrl { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? Servings { get; init; }
    public int? PrepTimeMinutes { get; init; }
    public int? CookTimeMinutes { get; init; }
    public List<Ingredient> Ingredients { get; init; } = [];
    public List<string> Instructions { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<string> ImageUrls { get; init; } = [];
    public RecipeRating? Rating { get; init; }
    public DateTimeOffset ImportedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string Type { get; init; } = "recipe";
}

// Summary projection for list responses (no ingredients/instructions)
public record RecipeSummary(
    string Id,
    string Title,
    string? Description,
    int? Servings,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    List<string> Tags,
    List<string> ImageUrls,
    RecipeRating? Rating,
    DateTimeOffset ImportedAt
);
