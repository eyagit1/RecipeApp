using Microsoft.EntityFrameworkCore;
using RecipeAnalytics.Domain.Entities;
using RecipeAnalytics.Domain.Enums;

namespace RecipeAnalytics.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite primary key for Many-to-Many join table
            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            // Set up explicit navigation paths
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure database column mappings for clean decimals
            modelBuilder.Entity<Ingredient>()
                .Property(i => i.CostPerUnit)
                .HasPrecision(18, 2);

            // --- DATA SEEDING (Authentic Tunisian and French Recipes) ---
            
            // 1. Seed Ingredients
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { Id = 1, Name = "Couscous Semolina", CaloriesPerUnit = 3.6, UnitOfMeasure = "g", CostPerUnit = 0.002M },
                new Ingredient { Id = 2, Name = "Olive Oil", CaloriesPerUnit = 8.84, UnitOfMeasure = "ml", CostPerUnit = 0.012M },
                new Ingredient { Id = 3, Name = "Tomato Paste", CaloriesPerUnit = 0.82, UnitOfMeasure = "g", CostPerUnit = 0.004M },
                new Ingredient { Id = 4, Name = "Lamb Meat", CaloriesPerUnit = 2.94, UnitOfMeasure = "g", CostPerUnit = 0.022M },
                new Ingredient { Id = 5, Name = "Harissa", CaloriesPerUnit = 1.2, UnitOfMeasure = "g", CostPerUnit = 0.006M },
                new Ingredient { Id = 6, Name = "Butter", CaloriesPerUnit = 7.17, UnitOfMeasure = "g", CostPerUnit = 0.008M },
                new Ingredient { Id = 7, Name = "Eggs", CaloriesPerUnit = 143, UnitOfMeasure = "piece", CostPerUnit = 0.25M },
                new Ingredient { Id = 8, Name = "Heavy Cream", CaloriesPerUnit = 3.45, UnitOfMeasure = "ml", CostPerUnit = 0.005M },
                new Ingredient { Id = 9, Name = "Gruyere Cheese", CaloriesPerUnit = 4.13, UnitOfMeasure = "g", CostPerUnit = 0.018M }
            );

            // 2. Seed Recipes
            modelBuilder.Entity<Recipe>().HasData(
                new Recipe
                {
                    Id = 1,
                    Title = "Authentic Tunisian Lamb Couscous",
                    Description = "Traditional festive Tunisian dish with tender lamb, aromatic spices, and steamed semolina.",
                    Instructions = "1. Sauté lamb and onions in olive oil. 2. Add tomato paste, harissa, spices and water; simmer. 3. Steam couscous over the stew broth. 4. Combine and serve with chickpeas and vegetables.",
                    Category = RecipeCategory.Lunch,
                    Cuisine = CuisineType.Tunisian,
                    Servings = 4
                },
                new Recipe
                {
                    Id = 2,
                    Title = "Classic French Quiche Lorraine",
                    Description = "Savory French tart featuring a rich custard of eggs, heavy cream, and melted cheese.",
                    Instructions = "1. Pre-bake pastry crust. 2. Mix whisked eggs, heavy cream, salt, and pepper. 3. Lay diced proteins and cheese on the crust base. 4. Pour egg mixture over and bake at 180°C for 35 mins.",
                    Category = RecipeCategory.Dinner,
                    Cuisine = CuisineType.French,
                    Servings = 6
                }
            );

            // 3. Seed Relationships (Recipe Ingredients Link)
            modelBuilder.Entity<RecipeIngredient>().HasData(
                // Lamb Couscous links
                new RecipeIngredient { RecipeId = 1, IngredientId = 1, Quantity = 500 }, // 500g Semolina
                new RecipeIngredient { RecipeId = 1, IngredientId = 2, Quantity = 50 },  // 50ml Olive Oil
                new RecipeIngredient { RecipeId = 1, IngredientId = 3, Quantity = 70 },  // 70g Tomato paste
                new RecipeIngredient { RecipeId = 1, IngredientId = 4, Quantity = 600 }, // 600g Lamb
                new RecipeIngredient { RecipeId = 1, IngredientId = 5, Quantity = 25 },  // 25g Harissa
                
                // Quiche Lorraine links
                new RecipeIngredient { RecipeId = 2, IngredientId = 6, Quantity = 50 },  // 50g Butter
                new RecipeIngredient { RecipeId = 2, IngredientId = 7, Quantity = 4 },   // 4 Eggs
                new RecipeIngredient { RecipeId = 2, IngredientId = 8, Quantity = 250 }, // 250ml Cream
                new RecipeIngredient { RecipeId = 2, IngredientId = 9, Quantity = 150 }  // 150g Gruyere
            );
        }
    }
}