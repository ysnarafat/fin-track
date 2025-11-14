# Known Issues

This document tracks known issues in the codebase that need attention.

## Current Issues

### 1. Type Mismatch in CategoryEntityTests (High Priority)

**File**: `src/frontend/tests/FinTrack.Tests.Unit/Domain/CategoryEntityTests.cs`  
**Method**: `IsValid_WithDifferentBudgetLimits_ShouldReturnExpectedResult`  
**Issue**: Parameter type mismatch between test method and actual property type

#### Problem Description
The test method parameter type was changed from `decimal?` to `double?`, but the `Category.BudgetLimit` property remains `decimal?`. This creates a type mismatch that requires explicit casting and may introduce precision issues.

#### Current Code (Partially Fixed)
```csharp
[Theory]
[InlineData(-100.0, false)] // Negative budget limit - Updated with .0 suffix
[InlineData(0.0, true)] // Zero budget limit - Updated with .0 suffix
[InlineData(100.0, true)] // Positive budget limit - Updated with .0 suffix
[InlineData(null, true)] // No budget limit
public void IsValid_WithDifferentBudgetLimits_ShouldReturnExpectedResult(double? budgetLimit, bool expectedValid)
{
    // ...
    if (budgetLimit.HasValue)
        category.BudgetLimit = (decimal)budgetLimit.Value; // Explicit casting still required
    // ...
}
```

**Recent Update**: Test data literals were updated to use `.0` suffix for better type clarity, but the core type mismatch issue remains.

#### Recommended Fix
```csharp
[Theory]
[InlineData(-100m, false)] // Negative budget limit
[InlineData(0m, true)] // Zero budget limit
[InlineData(100m, true)] // Positive budget limit
[InlineData(null, true)] // No budget limit
public void IsValid_WithDifferentBudgetLimits_ShouldReturnExpectedResult(decimal? budgetLimit, bool expectedValid)
{
    // ...
    if (budgetLimit.HasValue)
        category.BudgetLimit = budgetLimit.Value; // No casting needed
    // ...
}
```

#### Impact
- **Type Safety**: Reduces type safety by requiring explicit casting
- **Precision**: Potential precision loss when converting from double to decimal
- **Maintainability**: Makes the test less clear about the actual property type being tested
- **Best Practices**: Violates the principle of matching test parameter types with actual property types

#### Resolution Steps
1. Change the parameter type from `double?` to `decimal?`
2. Update the `[InlineData]` attributes to use decimal literals with `m` suffix
3. Remove the explicit casting in the test method body
4. Verify that all tests pass after the change

#### Prevention
- Always verify that test parameter types match the actual property types being tested
- Use explicit type suffixes for numeric literals in test data (`m` for decimal, `f` for float)
- Review type changes during code reviews to catch such mismatches early

## Resolved Issues

*No resolved issues yet.*

---

## Reporting New Issues

When reporting new issues:

1. **File Location**: Specify the exact file path and method/class name
2. **Issue Type**: Categorize as Bug, Type Safety, Performance, etc.
3. **Impact Assessment**: Describe the potential impact on functionality
4. **Reproduction Steps**: Provide clear steps to reproduce the issue
5. **Recommended Fix**: Suggest a solution if possible
6. **Prevention**: Suggest ways to prevent similar issues in the future

## Issue Priorities

- **High**: Type safety issues, build failures, critical functionality bugs
- **Medium**: Performance issues, maintainability concerns, minor bugs
- **Low**: Code style issues, documentation improvements, nice-to-have features