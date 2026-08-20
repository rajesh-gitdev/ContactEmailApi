# Deployment Guide

This guide covers configuring, building, and deploying the Enterprise Contact & Email API.

## 1. Prerequisites

- **.NET 10 SDK** (`global.json` pins `10.0.100`).
- **SQL Server** 2019+ (or Azure SQL) reachable from the API.
- An **SMTP** account/relay for outbound mail.
- Optional: **Docker** / a container runtime, and a **Google reCAPTCHA** key pair.

## 2. Configuration

All settings live in `appsettings.json` with `appsettings.{Environment}.json` overlays.
**Never commit real secrets.** In production, supply them via environment variables or a
secret store (Azure Key Vault, AWS Secrets Manager, Kubernetes secrets, Docker secrets).

Environment variables use `__` (double underscore) as the section separator:

```
ConnectionStrings__SqlServer="Server=db;Database=ContactEmailApi;User Id=app;Password=...;TrustServerCertificate=True"
Jwt__Issuer="https://api.yourdomain.com"
Jwt__Audience="contact-email-api-clients"
Jwt__SigningKey="<a random string of at least 32 characters>"
ApiKeys__Keys__0__Key="<website-key>"
ApiKeys__Keys__0__Owner="MarketingSite"
ApiKeys__Keys__0__Role="Website"
ApiKeys__Keys__0__Enabled="true"
Smtp__Host="smtp.yourprovider.com"
Smtp__Port="587"
Smtp__Username="<smtp-user>"
Smtp__Password="<smtp-pass>"
Smtp__UseSsl="true"
Smtp__SenderEmail="no-reply@yourdomain.com"
Smtp__SenderName="Your Company"
Smtp__BusinessEmail="contact@yourdomain.com"
Cors__AllowedOrigins__0="https://www.yourdomain.com"
Recaptcha__Enabled="true"
Recaptcha__SecretKey="<recaptcha-secret>"
```

Key sections:

- **ConnectionStrings:SqlServer** — the database connection.
- **Jwt** — issuer, audience, and a signing key of **at least 32 characters** (validated at
  startup; the app refuses to start otherwise).
- **ApiKeys** — issued keys with owner + role (`Admin`/`Website`/`Internal`/`System`).
- **Smtp** — mail server plus the `BusinessEmail` inbox that form notifications go to.
- **Cors:AllowedOrigins** — your front-end origins. In **Production** the app refuses to
  start with an empty or wildcard list.
- **SpamProtection** — honeypot/timestamp/duplicate tuning.
- **Recaptcha** — set `Enabled=true` and provide the secret to require reCAPTCHA.
- **Otp** — one-time-password length and expiry.

## 3. Database

Create the schema with EF Core migrations:

```bash
dotnet tool install --global dotnet-ef            # once
dotnet ef migrations add InitialCreate -p src/ContactEmailApi.Persistence -s src/ContactEmailApi.Api
dotnet ef database update -p src/ContactEmailApi.Persistence -s src/ContactEmailApi.Api
```

For containers/CI, generate an idempotent SQL script and apply it during release:

```bash
dotnet ef migrations script --idempotent -p src/ContactEmailApi.Persistence -s src/ContactEmailApi.Api -o migrate.sql
```

## 4. Build, test, run

```bash
dotnet restore
dotnet build -c Release
dotnet test                                        # unit + integration tests
dotnet run -c Release --project src/ContactEmailApi.Api
```

Docs at `/scalar/v1` and `/swagger`; health at `/health`, `/health/live`, `/health/ready`.

## 5. Docker

```bash
docker build -t contact-email-api -f src/ContactEmailApi.Api/Dockerfile .
docker run -p 8080:8080 --env-file ./.env contact-email-api
```

Or the full stack (API + SQL Server) with Compose:

```bash
docker compose up --build
```

The image runs as a non-root user and listens on port 8080.

## 6. Reverse proxy / TLS

Terminate TLS at your ingress (nginx, Caddy, Azure App Gateway, ALB). The app already
honors forwarded headers, so `X-Forwarded-For`/`X-Forwarded-Proto` drive client IP
(used by rate limiting and spam checks) and HTTPS detection. Restrict the docs endpoints
(`/scalar`, `/swagger`, `/openapi`) at the proxy if they shouldn't be public.

## 7. Health checks & monitoring

- **Liveness**: `GET /health/live` (process up).
- **Readiness**: `GET /health/ready` (memory, SMTP reachability, database).

Point your orchestrator's liveness probe at `/health/live` and readiness probe at
`/health/ready`. Logs are structured (Serilog) to console and a daily rolling file
(`logs/`), each line carrying the correlation ID.

## 8. Operational notes

- **Email delivery** runs on a background queue, so a slow/failing SMTP server never
  blocks API responses. Delivery outcomes (including failures and attempt counts) are
  recorded in the `EmailLogs` table.
- **Rate limits**: contact/support/career/feedback/callback/business 5/min, newsletter
  3/min, OTP 3/5 min, admin/email-send 60/min — all per client IP. Tune in
  `RateLimitingExtensions`.
- **Scaling out**: the email queue and the duplicate-detection cache are in-process. For
  multiple instances, each node has its own queue (fine) and its own dedupe cache
  (duplicates are only detected per-node) — move dedupe to a shared store (e.g. Redis)
  and consider an external queue if you need cross-node delivery guarantees.
- **Secrets rotation**: API keys and the JWT signing key are read from configuration;
  rotate by updating configuration and recycling the app.
