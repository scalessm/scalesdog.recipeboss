using Microsoft.Azure.Cosmos;
using RecipeBoss.Api.Models;

namespace RecipeBoss.Api.Infrastructure;

public class RecipeSeeder
{
    private static readonly Action<ILogger, int, string, Exception?> _logSeeded =
        LoggerMessage.Define<int, string>(LogLevel.Information, default, "Seeded {Count} recipes for user {UserId}");

    public static async Task SeedAsync(CosmosClient client, string userOid, ILogger logger)
    {
        var db = await client.CreateDatabaseIfNotExistsAsync("recipeboss");
        var container = await db.Database.CreateContainerIfNotExistsAsync(
            new ContainerProperties("recipes", "/userId"));

        var recipes = GetSeedRecipes(userOid);
        foreach (var recipe in recipes)
            await container.Container.UpsertItemAsync(recipe, new PartitionKey(recipe.UserId));

        _logSeeded(logger, recipes.Count, userOid, null);
    }

    private static List<Recipe> GetSeedRecipes(string userId) =>
    [
        new Recipe
        {
            Id = "seed-001",
            UserId = userId,
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
            Id = "seed-002",
            UserId = userId,
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
            Id = "seed-003",
            UserId = userId,
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
            Id = "seed-004",
            UserId = userId,
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
