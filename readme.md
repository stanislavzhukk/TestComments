# TestComments

TestComments is a web application for publishing and discussing comments. It
contains an Angular frontend, an ASP.NET Core Web API, and PostgreSQL storage.
The whole application is prepared to run with Docker Compose.

## Live demo

- Frontend: <FRONTEND_URL>
- API / Swagger: <API_URL>/swagger/index.html

> The test stand uses free hosting, so the API and the frontend may be asleep
> after a period of inactivity. On the first visit, allow 30-60 seconds for both
> services to wake up and warm up before using the application.

## Features

- Creating comments with a name, e-mail address, optional website, and text.
- CAPTCHA protection when creating a comment.
- Nested replies to comments.
- Formatting comment text with italic, bold, code, and links.
- File attachment:
  - image in JPG/JPEG, GIF, and PNG formats;
  - text file in TXT format up to 100 KB;
  - file type and size validation on the frontend and API;
  - drag-and-drop upload in the comment form;
  - image preview and lightbox.
- Server-side pagination for the comments list.
- Sorting by date, user name, or e-mail in ascending or descending order.
- Short polling of the comments list every 5 seconds.
- Storage of the author's IP address and user-agent with each comment.
- Server-side validation, HTML sanitization, and persistent file storage.
- Swagger/OpenAPI support for the API.
- pgAdmin included in the Docker Compose environment.

## Technology stack

- **Frontend:** Angular 22, TypeScript, RxJS, Nginx.
- **Backend:** ASP.NET Core / .NET 10 Web API.
- **Database:** PostgreSQL 16 with Entity Framework Core and automatic migrations.
- **Image processing:** SkiaSharp.
- **Infrastructure:** Docker Compose with persistent volumes for PostgreSQL,
  pgAdmin, and uploaded files.

## Quick start

### Requirements

- Docker Desktop or Docker Engine with the Docker Compose plugin.

### Start the application

From the repository root, execute:

```bash
cp .env.example .env
docker compose up --build
```

No manual steps are needed: the database is created and EF Core migrations are
applied automatically on API startup.

Services and URLs:

| Service  | URL                                      |
|----------|------------------------------------------|
| Frontend | <http://localhost>                       |
| API      | <http://localhost:8080>                  |
| Swagger  | <http://localhost:8080/swagger/index.html> |
| pgAdmin  | <http://localhost:5050>                  |

To stop the environment: `docker compose down` (add `-v` to also remove
volumes, i.e. the database and uploaded files).

## Configuration

The default local configuration is stored in `.env.example`. The `.env` file
controls the PostgreSQL connection, pgAdmin credentials, and allowed frontend
origins.

| Variable            | Description                          | Default                                  |
|---------------------|--------------------------------------|------------------------------------------|
| `POSTGRES_DB`       | database name                        | see `.env.example`                       |
| `POSTGRES_USER`     | database user                        | see `.env.example`                       |
| `POSTGRES_PASSWORD` | database password                    | see `.env.example`                       |
| `ALLOWED_ORIGINS`   | allowed CORS origins for the API     | `http://localhost,http://localhost:4200` |

Notes:

- On the test stand `ASPNETCORE_ENVIRONMENT` is intentionally set to
  `Development` so that Swagger is available.
- pgAdmin (port 5050) is intended for local use only and should not be exposed
  in production.

## Database

PostgreSQL 16 is used (the task allows any DBMS). The schema is created by EF
Core migrations, which are applied automatically when the API starts.

- `docs/schema-postgres.sql`: DDL of the actual schema
  (`pg_dump --schema-only --no-owner --no-privileges`).
- `docs/er-diagram.png`: ER diagram of the tables and their relations.

A MySQL Workbench file (`.mwb`) is not included, because MySQL Workbench works
only with MySQL. The SQL dump and the ER diagram are provided instead in the /docs folder.

## API endpoints

```text
GET  /api/comments
GET  /api/comments/{id}
POST /api/comments
GET  /api/captcha/generate
```

The `POST /api/comments` endpoint accepts `multipart/form-data`, including an
optional attachment. Full request and response schemas are available in Swagger.

## Architecture and decisions

- **Clean Architecture.** Dependencies point inward only:
  `API -> Application -> Domain`, and `Infrastructure -> Application/Domain`.
  Business rules and constants live in `Domain`, use cases and validators in
  `Application`, and EF Core, CAPTCHA, and file storage in `Infrastructure`.
- **Nested replies.** Each comment keeps a reference to its parent comment, which
  allows unlimited nesting and a simple tree on the client.
- **Server-side pagination and sorting.** The list is paged and sorted in the
  database query, so the response size and load stay constant as the number of
  comments grows.
- **Short polling every 5 seconds.** Chosen over WebSockets/SignalR for
  simplicity and compatibility with free hosting; new comments appear without a
  page reload.
- **HTML sanitization.** Comment text is sanitized on the server; only a small
  whitelist of tags (italic, bold, code, links) is allowed, which protects
  against XSS.
- **File validation.** Type and size are checked on both frontend and API.
  Images are processed with SkiaSharp, text files are limited to 100 KB, and
  uploaded files are stored on a persistent Docker volume.
- **CAPTCHA.** Generated and verified on the server, so it cannot be bypassed
  by modifying the client.

## Project structure

```text
API/            ASP.NET Core entry point, controllers, middleware, Dockerfile
Application/    DTOs, validators, application services, and use cases
Domain/         Entities, enums, shared errors, and business constants
Infrastructure/ EF Core persistence, migrations, CAPTCHA, and file storage
frontend/       Angular application and Nginx configuration
docs/           Database schema (SQL) and ER diagram
compose.yaml    Local multi-container environment
.env.example    Example Docker Compose configuration
```