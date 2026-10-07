# TestComments

TestComments is a web application for publishing and discussing comments. It
contains an Angular frontend, an ASP.NET Core Web API, and PostgreSQL storage.
The whole application is prepared to run with Docker Compose.
The deployed test stand uses free hosting, so the API and frontend may be
asleep after a period of inactivity. On the first visit, allow some time for
both services to wake up and warm up before using the application.

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
  - image preview and lightbox;
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

Services & URLs:

- Frontend: <http://localhost>
- API: <http://localhost:8080>
- Swagger: <http://localhost:8080/swagger/index.html>
- pgAdmin: <http://localhost:5050>

## Configuration

The default local configuration is stored in `.env.example`. The `.env` file
controls the PostgreSQL connection and allowed frontend origins. By default localhost and localhost:4200

## API endpoints

```text
GET  /api/comments
GET  /api/comments/{id}
POST /api/comments
GET  /api/captcha/generate
```

The `POST /api/comments` endpoint accepts `multipart/form-data`, including an
optional attachment. 
On the test stand, `ASPNETCORE_ENVIRONMENT` is intentionally set to `Development` so that Swagger is available.

## Project structure

```text
API/            ASP.NET Core entry point, controllers, middleware, Dockerfile
Application/    DTOs, validators, application services, use cases(services)
Domain/         Entities, enums, shared errors, and business constants
Infrastructure/ EF Core persistence, migrations, CAPTCHA, and file storage
frontend/       Angular application and Nginx configuration
compose.yaml    Local multi-container environment
.env.example    Example Docker Compose configuration
```