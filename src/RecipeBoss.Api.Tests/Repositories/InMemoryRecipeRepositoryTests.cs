using FluentAssertions;
using RecipeBoss.Api.Infrastructure;
using Xunit;

namespace RecipeBoss.Api.Tests.Repositories;

public class InMemoryRecipeRepositoryTests
{
    private const string DevUserId = "dev-user-001";
    private readonly InMemoryRecipeRepository _repository = new();

    // ── User isolation ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecipesAsync_ReturnsOnlyRecipesForRequestedUser()
    {
        var nonExistentUserId = "non-existent-user";
        var result = await _repository.GetRecipesAsync(nonExistentUserId, null, null, 1, 100);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecipesAsync_UserWithNoRecipes_ReturnsEmptyList()
    {
        var result = await _repository.GetRecipesAsync("unknown-user", null, null, 1, 100);
        result.Should().NotBeNull().And.BeEmpty();
    }

    // ── No-filter listing ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecipesAsync_NoFilters_ReturnsAllRecipesForUser()
    {
        var result = await _repository.GetRecipesAsync(DevUserId, null, null, 1, 100);
        result.Should().HaveCount(4);
    }

    // ── Full-text search (q) ────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecipesAsync_WithQ_FiltersByTitleCaseInsensitive()
    {
        var result = await _repository.GetRecipesAsync(DevUserId, "carbonara", null, 1, 100);
        result.Should().HaveCount(1).And.ContainSingle(r => r.Title.Contains("Carbonara"));
    }

    [Fact]
    public async Task GetRecipesAsync_WithQ_FiltersByDescriptionCaseInsensitive()
    {
        var result = await _repository.GetRecipesAsync(DevUserId, "cream", null, 1, 100);
        result.Should().NotBeEmpty();
        result.Should().Contain(r => r.Description != null && r.Description.Contains("cream", StringComparison.OrdinalIgnoreCase));
    }

    // ── Tag filtering (AND-logic) ────────────────────────────────────────────────

    [Fact]
    public async Task GetRecipesAsync_WithTags_AppliesAndLogic()
    {
        var tags = new[] { "Italian", "Pasta" };
        var result = await _repository.GetRecipesAsync(DevUserId, null, tags, 1, 100);
        result.Should().HaveCount(1).And.ContainSingle(r => r.Title == "Spaghetti Carbonara");
    }

    // ── Combined filters ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecipesAsync_WithTagsAndQ_AppliesBothFilters()
    {
        var tags = new[] { "Italian", "Pasta" };
        var result = await _repository.GetRecipesAsync(DevUserId, "carbonara", tags, 1, 100);
        result.Should().HaveCount(1).And.ContainSingle(r => r.Title == "Spaghetti Carbonara");
    }

    // ── Pagination ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecipesAsync_PageTwoPageSizeOne_SkipsFirstResult()
    {
        var pageOne = await _repository.GetRecipesAsync(DevUserId, null, null, 1, 1);
        var pageTwo = await _repository.GetRecipesAsync(DevUserId, null, null, 2, 1);
        
        pageOne.Should().HaveCount(1);
        pageTwo.Should().HaveCount(1);
        pageOne.First().Id.Should().NotBe(pageTwo.First().Id);
    }

    // ── Tags endpoint ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTagsAsync_ReturnsDistinctTagsForUser()
    {
        var result = await _repository.GetTagsAsync(DevUserId);
        var tags = result.ToList();
        
        tags.Should().Contain("Italian");
        tags.Should().Contain("Pasta");
        tags.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetTagsAsync_DoesNotReturnOtherUsersTagsAsync()
    {
        var result = await _repository.GetTagsAsync("different-user");
        result.Should().BeEmpty();
    }
}
