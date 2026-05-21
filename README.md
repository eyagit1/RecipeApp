# RecipeApp — Complete Evaluation Study Guide
> **.NET 10 · Blazor Server · Entity Framework Core · SQLite · Radzen**

---

## 1. What is This Project?

A **Blazor Server** web application built on **.NET 10** that manages culinary recipes.  
It lets users browse recipes, create new ones, search/filter by multiple criteria, view analytics charts, and generate a smart weekly meal plan with a consolidated shopping list.

The namespace used throughout the code is `RecipeAnalytics` (not `RecipeApp`), so expect that everywhere in `using` statements.

---

## 2. Technology Stack — Know Each Piece

| Technology | Role | Key Detail |
|---|---|---|
| **.NET 10** | Runtime framework | `net10.0` target in `.csproj` |
| **Blazor Server** | UI rendering mode | `@rendermode InteractiveServer` on every page |
| **Entity Framework Core 10** | ORM / database access | `Microsoft.EntityFrameworkCore.Sqlite` package |
| **SQLite** | Database engine | File `recipe_app.db` in project root |
| **Radzen.Blazor** | Chart components | Pie, Donut, Bar, Column charts |
| **Bootstrap 5** | CSS styling | Included in `wwwroot/lib/bootstrap` |
| **Bootstrap Icons** | Icon font | `bi bi-*` classes in HTML |

---

## 3. Project Folder Structure

```
RecipeApp/
├── Domain/                        ← Business model (entities + enums)
│   ├── Entities/
│   │   ├── Recipe.cs
│   │   ├── Ingredient.cs
│   │   └── RecipeIngredient.cs    ← Join table entity
│   └── Enums/
│       ├── RecipeCategory.cs      ← Breakfast, Lunch, Dinner, Dessert
│       └── CuisineType.cs         ← Tunisian, French, Italian, Mediterranean, International
├── Data/
│   └── AppDbContext.cs            ← EF Core context + seeding
├── Services/
│   ├── Interfaces/
│   │   └── IRecipeService.cs      ← Contract (interface)
│   ├── Implementations/
│   │   └── RecipeService.cs       ← Concrete implementation
│   └── DTOs/
│       ├── DashboardAnalyticsDto.cs
│       └── WeeklyMealPlanDto.cs
├── Components/
│   ├── Pages/                     ← Blazor pages (UI)
│   │   ├── Home.razor             ← "/" route
│   │   ├── Dashboard.razor        ← "/dashboard"
│   │   ├── Recipes.razor          ← "/recipes"
│   │   ├── AddRecipe.razor        ← "/recipes/add"
│   │   ├── MealPlanner.razor      ← "/meal-planner"
│   │   ├── Error.razor
│   │   └── NotFound.razor
│   ├── UI/
│   │   ├── KpiCard.razor          ← Reusable KPI card component
│   │   └── RecipeGrid.razor       ← Reusable recipe grid component
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── App.razor
│   ├── Routes.razor
│   └── _Imports.razor
├── GlobalUsings.cs                ← Global using statements
├── Program.cs                     ← App entry point + DI registration
├── appsettings.json
└── recipe_app.db                  ← SQLite database file
```

---

## 4. Domain Layer — The Data Model

### 4.1 `Recipe` Entity

```csharp
public class Recipe
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(150, MinimumLength = 3)]
    public string Title { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; }

    [Required]
    public string Instructions { get; set; }

    [Required]
    public RecipeCategory Category { get; set; }   // enum

    [Required]
    public CuisineType Cuisine { get; set; }        // enum

    [Required, Range(1, 100)]
    public int Servings { get; set; }

    // Navigation property (many-to-many via join table)
    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; }

    // ★ Computed Properties (no DB column, calculated in-memory)
    public double TotalCalories     => RecipeIngredients.Sum(ri => ri.Quantity * (ri.Ingredient?.CaloriesPerUnit ?? 0));
    public double CaloriesPerServing => Servings > 0 ? TotalCalories / Servings : 0;
    public decimal TotalCost        => RecipeIngredients.Sum(ri => (decimal)ri.Quantity * (ri.Ingredient?.CostPerUnit ?? 0M));
}
```

**Key point:** `TotalCalories`, `CaloriesPerServing`, and `TotalCost` are **computed properties** — they are NOT stored in the database. They are calculated every time from related `RecipeIngredients`. This is why the service always uses `.Include()` to load them.

---

### 4.2 `Ingredient` Entity

```csharp
public class Ingredient
{
    [Key] public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required, Range(0.1, 5000.0)]
    public double CaloriesPerUnit { get; set; }    // calories per 1 unit

    [Required, StringLength(20)]
    public string UnitOfMeasure { get; set; }      // e.g., "g", "ml", "piece"

    [Required, Range(0.0, 10000.0)]
    public decimal CostPerUnit { get; set; }       // cost per 1 unit

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; }
}
```

---

### 4.3 `RecipeIngredient` — The Join Table Entity

```csharp
public class RecipeIngredient
{
    [Required] public int RecipeId { get; set; }
    public virtual Recipe? Recipe { get; set; }

    [Required] public int IngredientId { get; set; }
    public virtual Ingredient? Ingredient { get; set; }

    [Required, Range(0.001, 10000.0)]
    public double Quantity { get; set; }    // How much of this ingredient is used
}
```

This implements a **Many-to-Many** relationship between `Recipe` and `Ingredient`. A recipe can have many ingredients, and an ingredient can be used in many recipes. The `Quantity` field is the extra data on the relationship.

---

### 4.4 Enums

```csharp
public enum RecipeCategory { Breakfast, Lunch, Dinner, Dessert }

public enum CuisineType { Tunisian, French, Italian, Mediterranean, International }
```

---

## 5. Database Layer — `AppDbContext`

### 5.1 DbSets (Tables)

```csharp
public DbSet<Recipe> Recipes => Set<Recipe>();
public DbSet<Ingredient> Ingredients => Set<Ingredient>();
public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
```

### 5.2 Key Model Configurations (`OnModelCreating`)

**Composite Primary Key** on the join table (no auto-generated ID):
```csharp
modelBuilder.Entity<RecipeIngredient>()
    .HasKey(ri => new { ri.RecipeId, ri.IngredientId });
```

**Cascade Delete behavior:**
- Deleting a `Recipe` → automatically deletes its `RecipeIngredient` rows (`Cascade`)
- Deleting an `Ingredient` → **blocked** if it's used in a recipe (`Restrict`) — prevents orphaned data

**Decimal precision:**
```csharp
modelBuilder.Entity<Ingredient>()
    .Property(i => i.CostPerUnit)
    .HasPrecision(18, 2);
```

### 5.3 Data Seeding

The context seeds **9 ingredients** and **2 recipes** with their relationships on first run:
- Recipe 1: "Authentic Tunisian Lamb Couscous" (Lunch, Tunisian, 4 servings)
- Recipe 2: "Classic French Quiche Lorraine" (Dinner, French, 6 servings)

### 5.4 Database Initialization in `Program.cs`

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();   // Creates DB + seeds if it doesn't exist
}
```

`EnsureCreated()` ≠ Migrations. It creates the schema once and never updates it. No migration files exist in this project.

---

## 6. Service Layer

### 6.1 Interface: `IRecipeService`

```csharp
public interface IRecipeService
{
    Task<List<Recipe>> GetAllRecipesAsync();
    Task<Recipe?> GetRecipeByIdAsync(int id);
    Task<List<Recipe>> SearchRecipesAsync(string? text, RecipeCategory? category, CuisineType? cuisine, double? maxCalories);
    Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync();
    Task<WeeklyMealPlanDto> GenerateWeeklyMealPlanAsync(double dailyCaloricTarget);  // Extra credit
    Task<Recipe> CreateRecipeAsync(Recipe recipe);
    Task UpdateRecipeAsync(Recipe recipe);
    Task DeleteRecipeAsync(int id);
}
```

### 6.2 `GetAllRecipesAsync` — Always Eager-Load Ingredients

```csharp
return await _context.Recipes
    .Include(r => r.RecipeIngredients)
    .ThenInclude(ri => ri.Ingredient)
    .AsNoTracking()        // ← read-only, better performance
    .ToListAsync();
```

`Include` + `ThenInclude` loads the full object graph so that computed properties like `TotalCalories` work correctly. Without this, `RecipeIngredients` would be an empty list.

### 6.3 `SearchRecipesAsync` — Mixed Strategy (important!)

This method uses a **two-phase approach**:

**Phase 1 — SQL level** (translated to WHERE clause):
```csharp
if (!string.IsNullOrWhiteSpace(text))
    query = query.Where(r => r.Title.ToLower().Contains(searchLower)
                          || r.Description.ToLower().Contains(searchLower));

if (category.HasValue)
    query = query.Where(r => r.Category == category.Value);

if (cuisine.HasValue)
    query = query.Where(r => r.Cuisine == cuisine.Value);
```

**Phase 2 — In-memory (client-side) for computed properties:**
```csharp
var results = await query.ToListAsync();    // ← executes SQL here

if (maxCalories.HasValue)
    results = results.Where(r => r.CaloriesPerServing <= maxCalories.Value).ToList();
```

`CaloriesPerServing` cannot be translated to SQL because it's a C# computed property, so the calorie filter must happen after data is loaded into memory.

### 6.4 `GetDashboardAnalyticsAsync` — EF Core GroupBy

```csharp
RecipesByCategory = await _context.Recipes
    .GroupBy(r => r.Category)
    .Select(g => new CategoryCountDto
    {
        Category = g.Key,
        Count = g.Count()
    }).ToListAsync(),
```

This runs as a single `GROUP BY` SQL query.

### 6.5 `GenerateWeeklyMealPlanAsync` — Extra Credit Feature

The algorithm:
1. Loads all recipes
2. For each of the 7 days of the week, picks the recipe whose `TotalCalories` is **closest to the daily target** (using `Math.Abs`)
3. Aggregates all required ingredients using `GroupBy(ri => ri.IngredientId)` to produce a consolidated shopping list
4. Returns a `WeeklyMealPlanDto` with the daily schedule, total weekly calories, and shopping list

```csharp
var chosenRecipe = allRecipes
    .OrderBy(r => Math.Abs(r.TotalCalories - dailyCaloricTarget))
    .ThenBy(_ => random.Next())     // tie-breaking with randomness
    .FirstOrDefault();
```

---

## 7. DTOs (Data Transfer Objects)

### `DashboardAnalyticsDto`
```csharp
public class DashboardAnalyticsDto
{
    public int TotalRecipesCount { get; set; }
    public int TotalIngredientsCount { get; set; }
    public List<CategoryCountDto> RecipesByCategory { get; set; }
    public List<CuisineCountDto> RecipesByCuisine { get; set; }
}
```

### `WeeklyMealPlanDto`
```csharp
public class WeeklyMealPlanDto
{
    public Dictionary<DayOfWeek, Recipe> DailyMenu { get; set; }
    public double TotalWeeklyCalories { get; set; }
    public List<ConsolidatedShoppingItemDto> ConsolidatedShoppingList { get; set; }
}
```

---

## 8. Dependency Injection — `Program.cs`

```csharp
// Blazor Server + interactive components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Radzen UI components (needed for charts, etc.)
builder.Services.AddRadzenComponents();

// EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Service layer registration (interface → implementation)
builder.Services.AddScoped<IRecipeService, RecipeService>();
```

`AddScoped` means one `RecipeService` instance per HTTP request (the standard choice for services that use a DbContext).

---

## 9. Blazor Pages — What Each Does

### `Home.razor` (`/`)
- Calls `GetDashboardAnalyticsAsync()`
- Shows total recipe and ingredient counts as cards
- Renders a **Radzen Pie chart** (recipes by cuisine) and a **Radzen Column chart** (recipes by category)
- Uses two local helper classes `ChartPieItem` and `ChartBarItem` to map DTOs to chart-compatible models

### `Dashboard.razor` (`/dashboard`)
- The "power user" dashboard — also calls `GetDashboardAnalyticsAsync()` for KPI cards
- Renders a **Radzen Donut chart** (by category) and a **Radzen Bar chart** (by cuisine)
- Has a full real-time multi-criteria search engine with: text input, category select, cuisine select, and a **range slider** for max calories
- Calls `SearchRecipesAsync(...)` on every filter change via `@bind:after="TriggerEngineSearch"`
- Uses the reusable `<RecipeGrid>` component to display results

### `Recipes.razor` (`/recipes`)
- Shows all recipes as a **card grid**
- Each card shows: title, cuisine badge, description, servings, calories/serving, total cost
- Has filter controls (same as Dashboard)
- Has a **Delete** button on each card that calls `DeleteRecipeAsync(id)`
- Button "Create New Recipe" navigates to `/recipes/add`

### `AddRecipe.razor` (`/recipes/add`)
- Uses Blazor's **`<EditForm>`** with `<DataAnnotationsValidator />` and `<ValidationSummary />`
- Bound to a `Recipe` object: title, description, category (select), cuisine (select), servings, instructions
- `OnValidSubmit` calls `CreateRecipeAsync()` then navigates back to `/recipes`

### `MealPlanner.razor` (`/meal-planner`)
- Extra credit feature
- User inputs a daily caloric target (default 1200 kcal)
- Clicking "Compile Weekly Schedule" calls `GenerateWeeklyMealPlanAsync(target)`
- Displays a **weekly table** (7 days × recipe/cuisine/calories)
- Displays a **consolidated shopping list** with ingredient quantities and costs
- Shows total estimated weekly cost

---

## 10. Reusable Components

### `KpiCard.razor`
A simple card that accepts `Title`, `Value`, `IconName`, and `ColorTheme` as parameters and renders a Bootstrap-styled metric card.

### `RecipeGrid.razor`
A component that accepts a `List<Recipe>` parameter and renders them in a grid format. Used in `Dashboard.razor` to avoid code duplication.

---

## 11. Key C# / .NET Concepts to Know

### Data Annotations Validation
```csharp
[Required(ErrorMessage = "...")]
[StringLength(150, MinimumLength = 3)]
[Range(1, 100, ErrorMessage = "...")]
```
Used on entity properties. Blazor's `<DataAnnotationsValidator>` automatically reads these in forms.

### `async/await` Pattern
All service methods return `Task<T>` and use `await` for non-blocking DB operations.  
All Blazor page lifecycle methods (`OnInitializedAsync`) follow the same pattern.

### LINQ
Used heavily for filtering, grouping, and projecting data:
- `.Where()`, `.Select()`, `.GroupBy()`, `.OrderBy()`, `.Sum()`, `.Average()`, `.Any()`
- `.SelectMany()` — used in shopping list consolidation to flatten a list of lists

### Nullable Reference Types
The project has `<Nullable>enable</Nullable>`. Watch for `?` on types like `Recipe?`, `string?`, and null-conditional operators like `ri.Ingredient?.CaloriesPerUnit ?? 0`.

### `IQueryable` vs `IEnumerable`
`IQueryable` builds a deferred SQL expression tree (executed at `.ToListAsync()`).  
Once `.ToListAsync()` runs, the result is `IEnumerable` / `List<T>` in memory.  
The calorie filter in `SearchRecipesAsync` correctly switches to in-memory filtering after the SQL query executes.

---

## 12. EF Core Concepts to Review

| Concept | Where Used |
|---|---|
| `DbContext` | `AppDbContext` |
| `DbSet<T>` | `Recipes`, `Ingredients`, `RecipeIngredients` |
| Composite Key | `RecipeIngredient` — `HasKey(ri => new { ri.RecipeId, ri.IngredientId })` |
| `.Include()` / `.ThenInclude()` | Eager loading in all `GetRecipes` queries |
| `.AsNoTracking()` | Read-only queries for performance |
| `OnDelete(DeleteBehavior.Cascade)` | Recipe → RecipeIngredient |
| `OnDelete(DeleteBehavior.Restrict)` | Ingredient → RecipeIngredient |
| `HasPrecision(18, 2)` | `CostPerUnit` decimal column |
| `.GroupBy()` in LINQ-to-SQL | Dashboard analytics aggregation |
| `EnsureCreated()` | Database initialization + seeding |
| Data Seeding via `HasData()` | Seed ingredients, recipes, and links |
| `EntityState.Modified` | Used in `UpdateRecipeAsync` |
| `AddScoped<>` | DbContext lifetime per request |

---

## 13. Architecture Pattern

The project follows a **3-layer architecture**:

```
UI Layer         →  Blazor .razor pages (inject IRecipeService)
Service Layer    →  RecipeService (business logic, DTO mapping)
Data Layer       →  AppDbContext + EF Core (database access)
```

The UI never touches the `DbContext` directly. It only knows about `IRecipeService`. This is **Dependency Inversion** (the D in SOLID) — the page depends on the interface, not the concrete class.

---

## 14. Quick Reference — Seeded Data

### Ingredients
| Id | Name | Calories/Unit | Unit | Cost/Unit |
|---|---|---|---|---|
| 1 | Couscous Semolina | 3.6 | g | 0.002 |
| 2 | Olive Oil | 8.84 | ml | 0.012 |
| 3 | Tomato Paste | 0.82 | g | 0.004 |
| 4 | Lamb Meat | 2.94 | g | 0.022 |
| 5 | Harissa | 1.2 | g | 0.006 |
| 6 | Butter | 7.17 | g | 0.008 |
| 7 | Eggs | 143 | piece | 0.25 |
| 8 | Heavy Cream | 3.45 | ml | 0.005 |
| 9 | Gruyere Cheese | 4.13 | g | 0.018 |

### Recipe 1 — Tunisian Lamb Couscous (Lunch, 4 servings)
500g Semolina + 50ml Olive Oil + 70g Tomato Paste + 600g Lamb + 25g Harissa

### Recipe 2 — French Quiche Lorraine (Dinner, 6 servings)
50g Butter + 4 Eggs + 250ml Heavy Cream + 150g Gruyere

---

## 15. Things Likely to Be Tested

1. **Explain the many-to-many relationship** — `Recipe ↔ RecipeIngredient ↔ Ingredient`, composite key, why a join entity is used
2. **Why is `CaloriesPerServing` filtered in-memory, not in SQL?** — It's a computed C# property, not a DB column
3. **What does `AsNoTracking()` do?** — Tells EF Core not to track entity changes, improving read performance
4. **What is DI (Dependency Injection) and how is it used here?** — `IRecipeService` registered as `Scoped`, injected via `@inject` in Blazor pages
5. **Difference between `EnsureCreated()` and Migrations** — `EnsureCreated` is simpler, creates schema once, no version history
6. **How does the weekly meal planner algorithm work?** — Picks recipe closest to caloric target using `Math.Abs`, consolidates ingredients with `GroupBy`
7. **What are Data Annotations?** — Attributes on model properties that define validation rules, used by `EditForm` in Blazor
8. **What is `@bind:after`?** — A Blazor event modifier that triggers a callback after a two-way binding value changes (used for reactive search filters)
9. **What is `OnInitializedAsync`?** — The Blazor lifecycle method called when a component is first loaded — used to fetch initial data
10. **What is a DTO?** — Data Transfer Object, a simple class used to carry data between layers without exposing the full entity