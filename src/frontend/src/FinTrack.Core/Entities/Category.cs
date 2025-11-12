using System.ComponentModel.DataAnnotations;

namespace FinTrack.Core.Entities;

/// <summary>
/// Represents a transaction category for organizing financial transactions
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Name of the category
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description of the category
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Color code for visual representation (hex format)
    /// </summary>
    [StringLength(7)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Icon name or identifier for the category
    /// </summary>
    [StringLength(50)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// Parent category ID for hierarchical categories
    /// </summary>
    public int? ParentCategoryId { get; set; }
    
    /// <summary>
    /// Indicates if the category is currently active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Indicates if this is a system-defined category that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; }
    
    /// <summary>
    /// Sort order for displaying categories
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Monthly budget limit for this category (optional)
    /// </summary>
    public decimal? BudgetLimit { get; set; }
    
    /// <summary>
    /// Type of transactions this category is used for (Income, Expense)
    /// </summary>
    [Required]
    public Core.Enums.TransactionType CategoryType { get; set; } = Core.Enums.TransactionType.Expense;
    
    // Navigation properties
    
    /// <summary>
    /// Collection of transactions associated with this category
    /// </summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    
    /// <summary>
    /// Navigation property to the parent category
    /// </summary>
    public virtual Category? ParentCategory { get; set; }
    
    /// <summary>
    /// Collection of child categories
    /// </summary>
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    
    /// <summary>
    /// Collection of budgets associated with this category
    /// </summary>
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();    
    
/// <summary>
    /// Constructor that initializes default values
    /// </summary>
    public Category()
    {
        IsActive = true;
        IsSystem = false;
        SortOrder = 0;
        Color = "#6B7280"; // Default gray color
    }
    
    /// <summary>
    /// Gets the full category path (including parent categories)
    /// </summary>
    public string FullPath
    {
        get
        {
            if (ParentCategory == null)
                return Name;
            
            return $"{ParentCategory.FullPath} > {Name}";
        }
    }
    
    /// <summary>
    /// Gets the depth level of this category in the hierarchy
    /// </summary>
    public int Level
    {
        get
        {
            int level = 0;
            var current = ParentCategory;
            while (current != null)
            {
                level++;
                current = current.ParentCategory;
            }
            return level;
        }
    }
    
    /// <summary>
    /// Indicates if this category has child categories
    /// </summary>
    public bool HasSubCategories => SubCategories.Any(c => !c.IsDeleted);
    
    /// <summary>
    /// Gets all active subcategories
    /// </summary>
    public IEnumerable<Category> ActiveSubCategories => 
        SubCategories.Where(c => c.IsActive && !c.IsDeleted);
    
    /// <summary>
    /// Gets all active transactions for this category
    /// </summary>
    public IEnumerable<Transaction> ActiveTransactions => 
        Transactions.Where(t => !t.IsDeleted);
    
    /// <summary>
    /// Validates the category data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (ParentCategoryId.HasValue && ParentCategoryId.Value == Id) return false; // Can't be parent of itself
        if (!string.IsNullOrEmpty(Color) && !IsValidHexColor(Color)) return false;
        if (BudgetLimit.HasValue && BudgetLimit.Value < 0) return false;
        
        return true;
    }
    
    /// <summary>
    /// Validates if a string is a valid hex color
    /// </summary>
    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrEmpty(color)) return false;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;
        
        return color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }
    
    /// <summary>
    /// Calculates total spending in this category for a date range
    /// </summary>
    public decimal CalculateSpending(DateTime startDate, DateTime endDate)
    {
        return ActiveTransactions
            .Where(t => t.Date >= startDate && t.Date <= endDate && t.Type == Core.Enums.TransactionType.Expense)
            .Sum(t => t.Amount);
    }
    
    /// <summary>
    /// Calculates total spending including subcategories for a date range
    /// </summary>
    public decimal CalculateSpendingWithSubCategories(DateTime startDate, DateTime endDate)
    {
        var totalSpending = CalculateSpending(startDate, endDate);
        
        foreach (var subCategory in ActiveSubCategories)
        {
            totalSpending += subCategory.CalculateSpendingWithSubCategories(startDate, endDate);
        }
        
        return totalSpending;
    }
    
    /// <summary>
    /// Gets the budget utilization percentage for the current month
    /// </summary>
    public decimal? BudgetUtilizationPercentage
    {
        get
        {
            if (!BudgetLimit.HasValue || BudgetLimit.Value == 0) return null;
            
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var spending = CalculateSpending(startOfMonth, endOfMonth);
            
            return (spending / BudgetLimit.Value) * 100;
        }
    }
}