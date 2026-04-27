namespace RecipeBoss.Api.Models;

public class Recipe
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Servings { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public List<Ingredient> Ingredients { get; set; } = [];
    public List<string> Instructions { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<string> ImageUrls { get; set; } = [];
    public Rating? Rating { get; set; }
    public string ImportStatus { get; set; } = "complete";
    public bool NeedsReview { get; set; }
    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Type { get; set; } = "recipe";
}

public class Ingredient
{
    public string Quantity { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
}

public class Rating
{
    public int Stars { get; set; }
    public DateTimeOffset RatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Notes { get; set; }
}
