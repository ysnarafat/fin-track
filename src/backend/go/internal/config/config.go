package config

import (
	"encoding/json"
	"log"
	"os"
	"path/filepath"
)

type Config struct {
	Port  string `json:"port"`
	DBUrl string `json:"database_url"`
}

func Load() Config {
	dir, _ := os.Getwd()
	log.Println("Working dir:", dir)
	dir = filepath.Join(dir, "..", "..", "internal", "config", "config.json")
	file, err := os.Open(dir)
	if err != nil {
		log.Printf("⚠️ Could not open config file, using env/defaults: %v", err)
		return loadFromEnvOrDefault()
	}
	defer file.Close()

	var cfg Config
	if err := json.NewDecoder(file).Decode(&cfg); err != nil {
		log.Fatalf("❌ Failed to decode config file: %v", err)
	}

	if val := os.Getenv("port"); val != "" {
		cfg.Port = val
	}
	if val := os.Getenv("database_url"); val != "" {
		cfg.DBUrl = val
	}

	return cfg
}

func loadFromEnvOrDefault() Config {
	defaultConfig := Config{
		Port:  getEnv("port", "8080"),
		DBUrl: getEnv("database_url", ""),
	}

	return defaultConfig
}

func getEnv(key, fallback string) string {
	if val := os.Getenv(key); val != "" {
		return val
	}
	return fallback
}
