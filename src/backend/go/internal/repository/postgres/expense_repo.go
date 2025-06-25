package postgres

import (
	"context"
	"database/sql"
	"fmt"

	"github.com/ysnarafat/fin-track/internal/domain"
	"github.com/ysnarafat/fin-track/internal/repository"
)

type PostgresExpenseRepo struct {
	DB *sql.DB
}

func NewExpenseRepo(db *sql.DB) repository.ExpenseRepository {
	return &PostgresExpenseRepo{DB: db}
}

func (r *PostgresExpenseRepo) Create(exp domain.Expense) error {
	_, err := r.DB.Exec("INSERT INTO expenses (amount, note) VALUES ($1, $2)", exp.Amount, exp.Note)
	return err
}

func (r *PostgresExpenseRepo) Get() ([]domain.Expense, error) {
	var expenses []domain.Expense

	fmt.Println("Enterd into Get")
	ctx := context.Background()
	rows, err := r.DB.QueryContext(ctx, "SELECT id, amount, description, created_at FROM expenses")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	for rows.Next() {
		var expense domain.Expense
		err := rows.Scan(&expense.ID, &expense.Amount, &expense.ID, &expense.Note)
		if err != nil {
			return nil, err
		}
		expenses = append(expenses, expense)
	}

	return expenses, nil
}
