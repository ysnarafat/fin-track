package handler

import (
    "encoding/json"
    "net/http"
    "github.com/ysnarafat/fin-track/internal/domain"
    "github.com/ysnarafat/fin-track/internal/service"
)

type ExpenseHandler struct {
    Svc *service.ExpenseService
}

func NewExpenseHandler(svc *service.ExpenseService) *ExpenseHandler {
    return &ExpenseHandler{Svc: svc}
}

func (h *ExpenseHandler) Create(w http.ResponseWriter, r *http.Request) {
    var exp domain.Expense
    if err := json.NewDecoder(r.Body).Decode(&exp); err != nil {
        http.Error(w, "invalid input", http.StatusBadRequest)
        return
    }

    if err := h.Svc.AddExpense(exp); err != nil {
        http.Error(w, "could not save", http.StatusInternalServerError)
        return
    }

    w.WriteHeader(http.StatusCreated)
}


func (h *ExpenseHandler) Get(w http.ResponseWriter, r *http.Request) {

    if _, err := h.Svc.GetExpense(); err != nil {
        http.Error(w, "could not get", http.StatusInternalServerError)
        return
    }

    w.WriteHeader(http.StatusOK)
}
