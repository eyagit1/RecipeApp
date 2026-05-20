using System.ComponentModel.DataAnnotations;

namespace RecipeAnalytics.Domain.Entities
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingredient name is mandatory.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ingredient name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 5000.0, ErrorMessage = "Caloric value must be between 0.1 and 5000.0.")]
        public double CaloriesPerUnit { get; set; }

        [Required(ErrorMessage = "Unit of measure is mandatory.")]
        [StringLength(20, ErrorMessage = "Unit of measure cannot exceed 20 characters (e.g., grams, ml, piece).")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        [Required]
        [Range(0.0, 10000.0, ErrorMessage = "Cost must be a positive value.")]
        public decimal CostPerUnit { get; set; }

        // Navigation property for many-to-many link
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}