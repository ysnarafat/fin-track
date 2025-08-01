package repository

import "github.com/ysnarafat/fin-track/internal/domain"

type ExpenseRepository interface {
    Create(exp domain.Expense) error
    Get() ([]domain.Expense, error)
}
