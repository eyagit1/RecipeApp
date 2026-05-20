using RecipeAnalytics.Domain.Entities;

namespace RecipeAnalytics.Services.DTOs
{
    public class WeeklyMealPlanDto
    {
        public Dictionary<DayOfWeek, Recipe> DailyMenu { get; set; } = new();
        public double TotalWeeklyCalories { get; set; }
        public List<ConsolidatedShoppingItemDto> ConsolidatedShoppingList { get; set; } = new();
    }

    public class ConsolidatedShoppingItemDto
    {
        public string IngredientName { get; set; } = string.Empty;
        public double TotalQuantityNeeded { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
    }
}