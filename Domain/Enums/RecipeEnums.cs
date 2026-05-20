// ============================================================
//  Domain/Enums/RecipeEnums.cs
//  All enumerations for the Recipe Management domain.
//  Using explicit integer values ensures database stability
//  even if enum members are reordered in future.
// ============================================================

namespace RecipeApp.Domain.Enums;

/// <summary>
/// Meal category indicating when a recipe is typically served.
/// </summary>
public enum MealCategory
{
    Breakfast = 1,
    Lunch     = 2,
    Dinner    = 3,
    Dessert   = 4,
    Snack     = 5
}

/// <summary>
/// Cuisine origin / culinary tradition of a recipe.
/// </summary>
public enum CuisineType
{
    Tunisian  = 1,
    French    = 2,
    Italian   = 3,
    Moroccan  = 4,
    Lebanese  = 5,
    American  = 6,
    Other     = 99
}

/// <summary>
/// The unit in which an ingredient's caloric value is measured.
/// Used for consistent caloric calculations regardless of the
/// unit recorded in a RecipeIngredient.
/// </summary>
public enum UnitOfMeasure
{
    Gram        = 1,   // 100 g reference unit for nutrition facts
    Milliliter  = 2,
    Piece       = 3,   // e.g. 1 egg, 1 clove of garlic
    Tablespoon  = 4,
    Teaspoon    = 5,
    Cup         = 6
}

/// <summary>
/// Difficulty level of a recipe — used by the Meal Planner
/// to suggest approachable weekday meals vs elaborate weekend meals.
/// </summary>
public enum DifficultyLevel
{
    Easy     = 1,
    Medium   = 2,
    Advanced = 3
}