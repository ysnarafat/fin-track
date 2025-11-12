using FinTrack.Core.Enums;

namespace FinTrack.Shared.DTOs;

/// <summary>
/// Data transfer object for Category entity
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }
    public decimal? BudgetLimit { get; set; }
    public TransactionType CategoryType { get; set; }
    
    // Navigation properties for display
    public string? ParentCategoryName { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
}

/// <summary>
/// DTO for creating a new category
/// </summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
    public decimal? BudgetLimit { get; set; }
    public TransactionType CategoryType { get; set; } = TransactionType.Expense;
}

/// <summary>
/// DTO for updating an existing category
/// </summary>
public class UpdateCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public decimal? BudgetLimit { get; set; }
    public TransactionType CategoryType { get; set; }
}