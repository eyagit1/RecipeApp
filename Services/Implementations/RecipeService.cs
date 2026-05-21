using Microsoft.EntityFrameworkCore;
using RecipeAnalytics.Data;
using RecipeAnalytics.Domain.Entities;
using RecipeAnalytics.Domain.Enums;
using RecipeAnalytics.Services.DTOs;
using RecipeAnalytics.Services.Interfaces;

namespace RecipeAnalytics.Services.Implementations
{
    public class RecipeService : IRecipeService
    {
        private readonly AppDbContext _context;

        public RecipeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Recipe?> GetRecipeByIdAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Recipe>> SearchRecipesAsync(string? text, RecipeCategory? category, CuisineType? cuisine, double? maxCalories)
        {
            // Build deferred IQueryable expression tree executing entirely at the SQL database layer
            IQueryable<Recipe> query = _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient);

            if (!string.IsNullOrWhiteSpace(text))
            {
                string searchLower = text.ToLower();
                query = query.Where(r => r.Title.ToLower().Contains(searchLower) || r.Description.ToLower().Contains(searchLower));
            }

            if (category.HasValue)
            {
                query = query.Where(r => r.Category == category.Value);
            }

            if (cuisine.HasValue)
            {
                query = query.Where(r => r.Cuisine == cuisine.Value);
            }

            var results = await query.ToListAsync();

            // Client-Side analytical evaluation for derived domain logic property expressions
            if (maxCalories.HasValue)
            {
                results = results.Where(r => r.CaloriesPerServing <= maxCalories.Value).ToList();
            }

            return results;
        }

        public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync()
        {
            var analytics = new DashboardAnalyticsDto
            {
                TotalRecipesCount = await _context.Recipes.CountAsync(),
                TotalIngredientsCount = await _context.Ingredients.CountAsync(),
                
                RecipesByCategory = await _context.Recipes
                    .GroupBy(r => r.Category)
                    .Select(g => new CategoryCountDto
                    {
                        Category = g.Key,
                        Count = g.Count()
                    }).ToListAsync(),

                RecipesByCuisine = await _context.Recipes
                    .GroupBy(r => r.Cuisine)
                    .Select(g => new CuisineCountDto
                    {
                        Cuisine = g.Key,
                        Count = g.Count()
                    }).ToListAsync()
            };

            return analytics;
        }

      
        public async Task<WeeklyMealPlanDto> GenerateWeeklyMealPlanAsync(double dailyCaloricTarget)
        {
            var plan = new WeeklyMealPlanDto();
            var allRecipes = await GetAllRecipesAsync();

            if (!allRecipes.Any()) return plan;

            var random = new Random();
            var daysOfWeek = Enum.GetValues<DayOfWeek>();

            foreach (var day in daysOfWeek)
            {
                // Core heuristic algorithm seeking recipes closest to target baseline
                var chosenRecipe = allRecipes
                    .OrderBy(r => Math.Abs(r.TotalCalories - dailyCaloricTarget))
                    .ThenBy(_ => random.Next())
                    .FirstOrDefault();

                if (chosenRecipe != null)
                {
                    plan.DailyMenu[day] = chosenRecipe;
                    plan.TotalWeeklyCalories += chosenRecipe.TotalCalories;
                }
            }

            // Consolidate ingredient objects across the weekly schedule to generate a shopping list
            var rawAggregatedIngredients = plan.DailyMenu.Values
                .SelectMany(r => r.RecipeIngredients)
                .GroupBy(ri => ri.IngredientId)
                .Select(g => new ConsolidatedShoppingItemDto
                {
                    IngredientName = g.First().Ingredient?.Name ?? "Unknown Ingredient",
                    UnitOfMeasure = g.First().Ingredient?.UnitOfMeasure ?? string.Empty,
                    TotalQuantityNeeded = g.Sum(ri => ri.Quantity),
                    EstimatedCost = g.Sum(ri => (decimal)ri.Quantity * (ri.Ingredient?.CostPerUnit ?? 0M))
                })
                .OrderBy(item => item.IngredientName)
                .ToList();

            plan.ConsolidatedShoppingList = rawAggregatedIngredients;
            return plan;
        }

        public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            _context.Entry(recipe).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecipeAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();
            }
        }
    }
}