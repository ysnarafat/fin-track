package service

import (
	"github.com/ysnarafat/fin-track/internal/domain"
	"github.com/ysnarafat/fin-track/internal/repository"
)

type ExpenseService struct {
	Repo repository.ExpenseRepository
}

func NewExpenseService(repo repository.ExpenseRepository) *ExpenseService {
	return &ExpenseService{Repo: repo}
}

func (s *ExpenseService) AddExpense(e domain.Expense) error {
	return s.Repo.Create(e)
}

func (s *ExpenseService) GetExpense() ([]domain.Expense, error) {
	return s.Repo.Get()
}
