using RecipeAnalytics.Domain.Enums;

namespace RecipeAnalytics.Services.DTOs
{
    public class DashboardAnalyticsDto
    {
        public int TotalRecipesCount { get; set; }
        public int TotalIngredientsCount { get; set; }
        public List<CategoryCountDto> RecipesByCategory { get; set; } = new();
        public List<CuisineCountDto> RecipesByCuisine { get; set; } = new();
    }

    public class CategoryCountDto
    {
        public RecipeCategory Category { get; set; }
        public string CategoryName => Category.ToString();
        public int Count { get; set; }
    }

    public class CuisineCountDto
    {
        public CuisineType Cuisine { get; set; }
        public string CuisineName => Cuisine.ToString();
        public int Count { get; set; }
    }
}