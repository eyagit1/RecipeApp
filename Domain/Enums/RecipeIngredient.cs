using System.ComponentModel.DataAnnotations;

namespace RecipeAnalytics.Domain.Entities
{
    public class RecipeIngredient
    {
        [Required]
        public int RecipeId { get; set; }
        public virtual Recipe? Recipe { get; set; }

        [Required]
        public int IngredientId { get; set; }
        public virtual Ingredient? Ingredient { get; set; }

        [Required]
        [Range(0.001, 10000.0, ErrorMessage = "Quantity must be strictly greater than 0.")]
        public double Quantity { get; set; }
    }
}