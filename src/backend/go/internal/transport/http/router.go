package http

import (
	"database/sql"
	"fmt"
	"net/http"

	"github.com/go-chi/chi/v5"
	_ "github.com/lib/pq"
	"github.com/ysnarafat/fin-track/internal/config"
	"github.com/ysnarafat/fin-track/internal/repository/postgres"
	"github.com/ysnarafat/fin-track/internal/service"
	"github.com/ysnarafat/fin-track/internal/transport/http/handler"
)

func NewRouter(cfg *config.Config) *http.Server {
    db, _ := sql.Open("postgres", cfg.DBUrl)

    expRepo := postgres.NewExpenseRepo(db)
    expService := service.NewExpenseService(expRepo)
    expHandler := handler.NewExpenseHandler(expService)

    r := chi.NewRouter()
    r.Post("/expenses", expHandler.Create)
    r.Get("/", expHandler.Get)

    return &http.Server{
        Addr: fmt.Sprintf(":%s", cfg.Port),
        Handler: r,
    }
}
