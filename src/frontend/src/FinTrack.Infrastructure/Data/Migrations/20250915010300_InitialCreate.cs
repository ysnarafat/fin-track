using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinTrack.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AccountNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    InitialBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SyncId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    ParentCategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CategoryType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SyncId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TargetAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CurrentAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TargetDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LinkedAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SyncId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goals_Accounts_LinkedAccountId",
                        column: x => x.LinkedAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Period = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    SpentAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AlertThreshold = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SyncId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SyncId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalMilestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GoalId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TargetDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SyncId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    SyncStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalMilestones_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsActive",
                table: "Accounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsDeleted",
                table: "Accounts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SyncId",
                table: "Accounts",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SyncStatus",
                table: "Accounts",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Type",
                table: "Accounts",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CategoryId",
                table: "Budgets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_IsActive",
                table: "Budgets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_IsDeleted",
                table: "Budgets",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_Period",
                table: "Budgets",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_StartDate_EndDate",
                table: "Budgets",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_SyncId",
                table: "Budgets",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_SyncStatus",
                table: "Budgets",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryType",
                table: "Categories",
                column: "CategoryType");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsSystem",
                table: "Categories",
                column: "IsSystem");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SortOrder",
                table: "Categories",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SyncId",
                table: "Categories",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SyncStatus",
                table: "Categories",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_GoalId",
                table: "GoalMilestones",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_IsCompleted",
                table: "GoalMilestones",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_IsDeleted",
                table: "GoalMilestones",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_SortOrder",
                table: "GoalMilestones",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_SyncId",
                table: "GoalMilestones",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_SyncStatus",
                table: "GoalMilestones",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_TargetDate",
                table: "GoalMilestones",
                column: "TargetDate");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_IsActive",
                table: "Goals",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_IsCompleted",
                table: "Goals",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_IsDeleted",
                table: "Goals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_LinkedAccountId",
                table: "Goals",
                column: "LinkedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_Priority",
                table: "Goals",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_SyncId",
                table: "Goals",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_SyncStatus",
                table: "Goals",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_TargetDate",
                table: "Goals",
                column: "TargetDate");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_Type",
                table: "Goals",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date",
                table: "Transactions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date_AccountId",
                table: "Transactions",
                columns: new[] { "Date", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IsDeleted",
                table: "Transactions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SyncId",
                table: "Transactions",
                column: "SyncId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SyncStatus",
                table: "Transactions",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Type",
                table: "Transactions",
                column: "Type");

            // Seed default data
            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Name", "Balance", "Type", "Currency", "Description", "IsActive", "InitialBalance", "CreatedAt", "UpdatedAt", "SyncId", "SyncStatus", "Version" },
                values: new object[] { 1, "Primary Checking", 0m, 0, "USD", "Default checking account", true, 0m, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "default-account-1", 0, 1L });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "CategoryType", "Icon", "Color", "IsSystem", "IsActive", "SortOrder", "CreatedAt", "UpdatedAt", "SyncId", "SyncStatus", "Version" },
                values: new object[,]
                {
                    { 1, "Food & Dining", 1, "restaurant", "#FF6B6B", true, true, 1, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-1", 0, 1L },
                    { 2, "Transportation", 1, "car", "#4ECDC4", true, true, 2, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-2", 0, 1L },
                    { 3, "Shopping", 1, "shopping_cart", "#45B7D1", true, true, 3, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-3", 0, 1L },
                    { 4, "Entertainment", 1, "movie", "#96CEB4", true, true, 4, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-4", 0, 1L },
                    { 5, "Bills & Utilities", 1, "receipt", "#FFEAA7", true, true, 5, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-5", 0, 1L },
                    { 6, "Healthcare", 1, "medical_services", "#DDA0DD", true, true, 6, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-6", 0, 1L },
                    { 7, "Education", 1, "school", "#98D8C8", true, true, 7, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-7", 0, 1L },
                    { 8, "Travel", 1, "flight", "#F7DC6F", true, true, 8, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-8", 0, 1L },
                    { 9, "Personal Care", 1, "spa", "#BB8FCE", true, true, 9, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-9", 0, 1L },
                    { 10, "Other Expenses", 1, "more_horiz", "#BDC3C7", true, true, 10, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-expense-10", 0, 1L },
                    { 11, "Salary", 0, "work", "#2ECC71", true, true, 1, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-income-1", 0, 1L },
                    { 12, "Freelance", 0, "laptop", "#27AE60", true, true, 2, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-income-2", 0, 1L },
                    { 13, "Investment Returns", 0, "trending_up", "#16A085", true, true, 3, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-income-3", 0, 1L },
                    { 14, "Business Income", 0, "business", "#1ABC9C", true, true, 4, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-income-4", 0, 1L },
                    { 15, "Gifts & Bonuses", 0, "card_giftcard", "#58D68D", true, true, 5, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-income-5", 0, 1L },
                    { 16, "Other Income", 0, "attach_money", "#82E0AA", true, true, 6, new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 15, 1, 3, 0, 0, DateTimeKind.Utc), "category-income-6", 0, 1L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalMilestones");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}