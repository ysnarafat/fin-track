package main

import (
    "log"
    "github.com/ysnarafat/fin-track/internal/config"
    "github.com/ysnarafat/fin-track/internal/transport/http"
)

func main() {
    cfg := config.Load()
    r := http.NewRouter(&cfg)

    log.Printf("Server starting at :%s...\n", cfg.Port)
    log.Fatal(r.ListenAndServe())
}
