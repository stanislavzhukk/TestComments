# Boilerplate: API + PostgreSQL + PGAdmin

This repository provides a ready-to-use local development environment with:

- ASP.NET Core API  
- PostgreSQL running in Docker  
- PGAdmin web UI for database management  
- Swagger for API exploration and testing  

Everything is wired together with Docker Compose for easy setup.

---

## Tech Stack

- .NET 8 (ASP.NET Core)
- PostgreSQL 16
- PGAdmin 4
- Docker & Docker Compose
- Entity Framework Core

---

## Getting Started

### 1 Prerequisites

Make sure you have installed:

- Docker  
- Docker Compose  

---

### 2 Environment variables

Create a `.env` file from the example:

```bash
cp .env.example .env
```

Build and start all services:

```bash
docker compose up --build
```

 Services & URLs

- PGAdmin (PostgreSQL UI)
http://localhost:5050/

- Swagger (API docs & testing)
http://localhost:8080/swagger/index.html