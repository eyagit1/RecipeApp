using System.ComponentModel.DataAnnotations;
using RecipeAnalytics.Domain.Enums;

namespace RecipeAnalytics.Domain.Entities
{
    public class Recipe
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Recipe title is mandatory.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Preparation instructions are required.")]
        public string Instructions { get; set; } = string.Empty;

        [Required]
        public RecipeCategory Category { get; set; }

        [Required]
        public CuisineType Cuisine { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Servings must be at least 1 and logical.")]
        public int Servings { get; set; }

        // DDD Encapsulated Domain Logic / Dynamic Calculations
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

        public double TotalCalories => RecipeIngredients.Sum(ri => ri.Quantity * (ri.Ingredient?.CaloriesPerUnit ?? 0));
        public double CaloriesPerServing => Servings > 0 ? TotalCalories / Servings : 0;
        public decimal TotalCost => RecipeIngredients.Sum(ri => (decimal)ri.Quantity * (ri.Ingredient?.CostPerUnit ?? 0M));
    }
}