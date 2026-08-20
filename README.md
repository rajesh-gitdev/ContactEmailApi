# Enterprise Contact & Email API — .NET 10

A production-grade Web API (Clean Architecture) for handling website "Send Message",
support, careers, newsletter, feedback, callback, and transactional-email workflows.
Designed to be called securely from HTML/Bootstrap/Angular front-ends.

> **This is the complete 3-phase build.** Phase 1 delivered the secured, observable API
> host; Phase 2 added the domain model, contracts, validation, and the email pipeline;
> Phase 3 adds all ten business endpoints with their services, full spam protection,
> audit logging, integration tests, a Postman collection, and a deployment guide.

---

## What's in Phase 1

The full application skeleton and every cross-cutting concern, wired end to end:

- **Clean Architecture solution** — `Domain`, `Shared`, `Application`, `Persistence`,
  `Infrastructure`, `Api`, plus `UnitTests`. Dependencies point inward.
- **Central Package Management** — every NuGet version is pinned once in
  `Directory.Packages.props`; common build settings live in `Directory.Build.props`.
- **Authentication** — dual scheme behind a single `MultiAuth` policy scheme:
  JWT Bearer (HS256) **and** API key (`X-Api-Key`). A request is routed to the API-key
  handler when the header is present, otherwise to JWT.
- **Authorization** — role policies for `Admin`, `Website`, `Internal`, `System`.
- **JWT issuance** — `POST /api/v1/auth/token` exchanges a valid API key for a signed JWT.
- **Rate limiting** — fixed-window per-IP policies: contact 5/min, newsletter 3/min,
  OTP 3/5 min, admin 60/min. Rejections return the standard envelope + `Retry-After`.
- **Middleware pipeline** — correlation IDs (`X-Correlation-ID`), security headers,
  and a global exception handler that always emits the standard `ApiResponse` envelope.
- **Standard response envelope** — `success`, `message`, `errors`, `requestId`,
  `statusCode`, `traceId`, `timestamp` (camelCase, nulls omitted).
- **Logging** — Serilog (console + rolling file, 14-day retention) with correlation ID
  and request logging enrichment.
- **OpenAPI** — first-party .NET 10 generator (`Microsoft.AspNetCore.OpenApi`, 3.1) with
  a security-scheme transformer, surfaced through **Scalar** (`/scalar/v1`) **and**
  classic **Swagger UI** (`/swagger`).
- **Health checks** — `/health`, `/health/live`, `/health/ready` (self, memory, SMTP
  reachability, and DbContext).
- **Response compression** (Brotli/Gzip), HSTS, HTTPS redirection, forwarded headers.
- **Containerization** — multi-stage `Dockerfile` (non-root) and `docker-compose.yml`
  (API + SQL Server 2022).
- **Tests** — xUnit unit tests for the response envelope and the API-key validator.

### Endpoints available in Phase 1

| Method | Route | Auth | Notes |
|-------|-------|------|-------|
| GET | `/api/v1/diagnostics/ping` | Anonymous | Liveness sanity check |
| GET | `/api/v1/diagnostics/whoami` | JWT or API key | Echoes the caller's identity/roles |
| POST | `/api/v1/auth/token` | API key | Exchanges an API key for a JWT |
| GET | `/health`, `/health/live`, `/health/ready` | Anonymous | Health probes |
| — | `/scalar/v1`, `/swagger`, `/openapi/v1.json` | Anonymous* | API docs |

\* Lock these down in production as needed.

---

## Prerequisites

- **.NET 10 SDK** (`global.json` pins `10.0.100`, roll-forward `latestFeature`).
- Optional: **Docker** (for the compose stack) and **SQL Server** (Phase 2+ persistence).

---

## Run it

```bash
# from the solution root
dotnet restore
dotnet build
dotnet run --project src/ContactEmailApi.Api
```

Then open:

- Scalar UI — `https://localhost:<port>/scalar/v1`
- Swagger UI — `https://localhost:<port>/swagger`
- Health — `https://localhost:<port>/health`

The console prints the actual port (see `src/ContactEmailApi.Api/Properties/launchSettings.json`).

### Try the auth flow

1. Grab a sample API key from `src/ContactEmailApi.Api/appsettings.json` (`ApiKeys` section —
   **replace the placeholders before any real use**).
2. `POST /api/v1/auth/token` with header `X-Api-Key: <key>` to receive a JWT.
3. Call `GET /api/v1/diagnostics/whoami` with either `X-Api-Key` or `Authorization: Bearer <jwt>`.

### Docker

```bash
docker compose up --build
```

---

## Troubleshooting: "The SDK 'Microsoft.NET.Sdk' could not be found"

If every project fails with this at build time, it's SDK resolution failing at
`global.json`, not a code problem. These projects target **net10.0**, so a **.NET 10 SDK**
is required.

1. Check what's installed: `dotnet --list-sdks`
2. If no `10.x.xxx` line appears, install the .NET 10 SDK from
   <https://dotnet.microsoft.com/download/dotnet/10.0> (Windows: `winget install Microsoft.DotNet.SDK.10`),
   then reopen your terminal/IDE.
3. `global.json` now rolls forward to any 10.x-or-newer SDK and accepts previews, so any
   recent .NET 10 SDK works. You can also delete `global.json` to use your latest SDK — but
   the build only succeeds on a .NET 10 SDK.
4. In Visual Studio, use a version that bundles the .NET 10 SDK (VS 2022 17.12+ or newer),
   since VS builds with its own SDK.

## Configuration

All settings live in `appsettings.json` (+ `Development`/`Production` overlays):

- `ConnectionStrings:SqlServer` — database connection (used from Phase 2).
- `Jwt` — issuer, audience, signing key (**min 32 chars**), token lifetimes, clock skew.
- `ApiKeys` — issued keys with owner + role. **Placeholders only; replace them.**
- `Smtp` — mail server settings and the destination business inbox (used from Phase 2).
- `Cors:AllowedOrigins` — allowed front-end origins. In **Production** the app refuses to
  start with an empty or wildcard origins list.

Prefer environment variables or a secret manager for secrets in real deployments.

---

## What's in Phase 2

The domain and the email engine that Phase 3's endpoints will orchestrate:

- **Domain entities** — `ContactSubmission`, `BusinessInquiry`, `SupportTicket`,
  `CareerApplication`, `NewsletterSubscriber`, `FeedbackEntry`, `CallbackRequest`,
  `OtpCode` (stores only a hash of the code), and `EmailLog` (send audit), plus the
  supporting enums (submission status, inquiry type, support category/priority, OTP
  purpose, template type, delivery status).
- **Request/response contracts** — DTOs for all 10 endpoints under
  `Application/Contracts`, including a shared `ISpamProtectedRequest` carrying the
  honeypot and form-timestamp fields so the client contract is stable now.
- **FluentValidation validators** — one per request, auto-registered via
  `AddApplication()`, with a shared honeypot rule. (Timestamp/duplicate/reCAPTCHA
  enforcement is wired in Phase 3.)
- **Email abstractions** — `IEmailService`, `IEmailQueue`, `IEmailTemplateRenderer`
  plus provider-agnostic `EmailMessage`/`EmailAddress`/`EmailAttachment` models.
- **MailKit email service** — builds a MIME message (HTML + optional plain text,
  Cc/Bcc, reply-to, priority, attachments) and sends it over SMTP, choosing
  SSL-on-connect vs STARTTLS from the configured port/SSL flag.
- **Background email queue** — a bounded `System.Threading.Channels` queue
  (`ChannelEmailQueue`) drained by a hosted `EmailQueueProcessor` that sends off the
  request thread, retries with exponential backoff, and writes an `EmailLog` audit row.
- **HTML email templates** — 10 responsive, inline-styled templates (contact, business,
  support, career, feedback, callback, OTP, password reset, welcome/newsletter, internal
  notification) shipped as embedded resources. The renderer does `{{token}}` substitution
  and **HTML-encodes every value** to prevent injection from user content.
- **Persistence** — EF Core `IEntityTypeConfiguration` for every entity (lengths,
  unique indexes on reference/ticket numbers and subscriber email, enum-to-string
  conversions) and matching `DbSet`s on `ApplicationDbContext`.
- **Tests** — validator tests (contact, newsletter/consent, honeypot) and renderer
  tests (token substitution + HTML-encoding).

> Phase 2 wires services and the pipeline; it does **not** add controllers yet, so no
> new HTTP routes appear until Phase 3. Generate an EF migration when you're ready to
> create the schema: `dotnet ef migrations add InitialCreate -p src/ContactEmailApi.Persistence -s src/ContactEmailApi.Api`.

---

## What's in Phase 3

The full public surface and the logic behind it:

- **All 10 business endpoints** with controllers, orchestrating services, and the standard
  response envelope (see the table below).
- **Full spam protection** (`SpamGuard`) layering a honeypot, a "filled too fast / too
  stale" timestamp check, in-memory duplicate detection, and optional Google reCAPTCHA
  verification (`GoogleRecaptchaVerifier`, off by default).
- **Request validation** — the FluentValidation validators now actually run, via an MVC
  action filter that throws a 422 with the standard envelope; automatic model-binding
  errors are wrapped in the same envelope.
- **Audit logging** — every submission is persisted and logged with a structured "Audit"
  event (reference/ticket number, never secrets); every outbound email is recorded in
  `EmailLogs`.
- **Reference/ticket numbers** — human-friendly identifiers (CT-, BIZ-, JOB-, CB-, SUP-…).
- **OTP issuance** — 6-digit codes, emailed to the recipient; only a salted hash is stored.
- **Integration tests** — a `WebApplicationFactory` harness (in-memory DB, faked SMTP,
  injected test key) covering health, anonymous access, auth enforcement, a happy-path
  submission, and validation failures.
- **Postman collection + environment** under `postman/`, and a **deployment guide** at
  `docs/DEPLOYMENT.md`.

### The 10 business endpoints

| Method | Route | Policy | Rate limit |
|-------|-------|--------|-----------|
| POST | `/api/v1/contact/send` | Website | contact |
| POST | `/api/v1/contact/business` | Website | contact |
| POST | `/api/v1/support/create` | Website | contact |
| POST | `/api/v1/career/apply` | Website | contact |
| POST | `/api/v1/newsletter/subscribe` | Website | newsletter |
| POST | `/api/v1/feedback/send` | Website | contact |
| POST | `/api/v1/callback/request` | Website | contact |
| POST | `/api/v1/email/send-otp` | Website | otp |
| POST | `/api/v1/email/send` | Internal/Admin | admin |
| POST | `/api/v1/email/test` | Admin | admin |

All public-form endpoints authenticate with a Website (or Admin) API key or JWT; the
`email/send` and `email/test` endpoints require Internal/Admin.

---

## Testing

```bash
dotnet test        # runs both the unit and integration test projects
```

The integration tests boot the real host with an in-memory database and a no-op SMTP
sender, so they need no external SQL Server or mail server.

---

## Notes & caveats

- **Build environment.** This solution was authored in a sandbox **without** a .NET SDK
  or access to NuGet, so it was **not** compiled here. Run `dotnet restore` / `dotnet build`
  locally. If a pinned patch version doesn't resolve on your feed, adjust it in
  `Directory.Packages.props` (versions are centralized there).
- **OpenAPI security transformer.** `SecuritySchemeDocumentTransformer.cs` targets the
  Microsoft.OpenApi **v2** object model that ships with .NET 10 (types live in the
  `Microsoft.OpenApi` namespace; references use `OpenApiSecuritySchemeReference`). This is
  the one file most sensitive to package revisions — if OpenAPI generation ever fails to
  build, check it first.
- **Response compression over HTTPS.** Enabled for HTTPS. Compressing responses that reflect
  secret-bearing input can expose you to BREACH-style attacks; keep sensitive values out of
  compressible response bodies, or disable `EnableForHttps` if that matches your threat model.
- **Docs endpoints** are open by default for convenience — restrict `/scalar`, `/swagger`,
  and `/openapi` in production if you don't want them public.
- **MailKit version.** The email pipeline pins MailKit/MimeKit `4.8.0` in
  `Directory.Packages.props`; if that exact patch doesn't resolve on your feed, bump it
  there. The `EmailQueueProcessor` is a hosted service, so email sends happen in the
  background — a failing SMTP server won't block API responses, but check `EmailLogs`
  (and the app logs) for delivery failures.
