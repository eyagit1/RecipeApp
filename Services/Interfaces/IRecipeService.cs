using RecipeAnalytics.Domain.Entities;
using RecipeAnalytics.Domain.Enums;
using RecipeAnalytics.Services.DTOs;

namespace RecipeAnalytics.Services.Interfaces
{
    public interface IRecipeService
    {
        Task<List<Recipe>> GetAllRecipesAsync();
        Task<Recipe?> GetRecipeByIdAsync(int id);
        Task<List<Recipe>> SearchRecipesAsync(string? text, RecipeCategory? category, CuisineType? cuisine, double? maxCalories);
        Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync();
        
        // --- EXTRA CREDIT CREATIVITY FEATURE EXTENSION ---
        Task<WeeklyMealPlanDto> GenerateWeeklyMealPlanAsync(double dailyCaloricTarget);
        
        Task<Recipe> CreateRecipeAsync(Recipe recipe);
        Task UpdateRecipeAsync(Recipe recipe);
        Task DeleteRecipeAsync(int id);
    }
}