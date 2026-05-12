# RSD — Implementation Tasks

**Version:** 1.0
**Date:** 2026-05-12
**Status:** Draft for review
**Owner:** Mark Podlyashetskyi

This document enumerates the concrete tasks required to deliver the backend and admin panel as specified across:

- [`architecture.md`](architecture.md) — high-level system view, ADRs, quality attributes.
- [`modules.md`](modules.md) — module-by-module reference with public APIs and dependency rules.
- [`../docs/superpowers/specs/2026-05-12-backend-and-admin-design.md`](../docs/superpowers/specs/2026-05-12-backend-and-admin-design.md) — implementation specification with schema, phasing, and configuration.

There are **23 tasks** grouped into the **5 phases** from the spec. Each phase ships as a single PR (Phase 4 may split into 4a / 4b). Within a phase, tasks are sequential by default; where two tasks are genuinely independent it is called out under **Dependencies**.

Every task is scoped to be reviewable in one focused sitting and ends with a verifiable demo. No task crosses phase boundaries.

---

## Phase summary

| Phase | Tasks | Outcome |
|---|---|---|
| 1 — Foundation | T01–T07 | Postgres + EF Core + Identity + admin shell, no public-facing change |
| 2 — Simple entities + Inbox | T08–T11 | 9 simple entities + Contact inbox, public site reads them from DB |
| 3 — List rows for Blog/Cases/Products/Services | T12–T14 | List + header-only edit; detail bodies still seeded |
| 4 — Rich detail bodies | T15–T18 | Typed and block-based body editors; previews |
| 5 — Operations polish | T19–T23 | Audit / Trash / Media / Users / SEO endpoints / cache rollout |

---

## Task index

| ID | Title | Phase |
|---|---|---|
| [T01](#t01) | Infrastructure setup: Postgres, volumes, NuGet packages, env scaffolding | 1 |
| [T02](#t02) | Persistence foundation: `AppDbContext`, base entities, Identity, initial migration | 1 |
| [T03](#t03) | Audit pipeline: interceptor + `IAuditLog` read | 1 |
| [T04](#t04) | Service abstractions: Storage, Imaging, Slugs, Cache, Email, Preview | 1 |
| [T05](#t05) | Authentication: Identity wiring, AdminBootstrapper, login + password-reset pages | 1 |
| [T06](#t06) | Admin shell: layout, sidebar, top bar, shared UI primitives | 1 |
| [T07](#t07) | Tests project: xUnit + Testcontainers + initial coverage | 1 |
| [T08](#t08) | Simple entities domain layer (9 entities) | 2 |
| [T09](#t09) | Simple entities admin CRUD pages | 2 |
| [T10](#t10) | Public site consumes simple entities from DB | 2 |
| [T11](#t11) | Contact-form pipeline: submissions, endpoint, inbox, SMTP | 2 |
| [T12](#t12) | Blog/Cases/Products/Services domain layer (header + seeded body) | 3 |
| [T13](#t13) | Blog/Cases/Products/Services admin: list + header-only edit | 3 |
| [T14](#t14) | Public list pages + detail-page header lookup | 3 |
| [T15](#t15) | Typed-body editors for Case and Product details | 4 |
| [T16](#t16) | Block-body editor for Blog and Service details | 4 |
| [T17](#t17) | `RichTextEditor`: Quill JS interop + server-side HTML sanitization | 4 |
| [T18](#t18) | Preview pipeline: HMAC tokens, `/preview/...` route, edit-page buttons | 4 |
| [T19](#t19) | Audit log viewer (`/admin/audit`) | 5 |
| [T20](#t20) | Trash UI (`/admin/trash`) | 5 |
| [T21](#t21) | Media library (`/admin/media`) with reference tracking | 5 |
| [T22](#t22) | User management (`/admin/users`): invite, disable, reset | 5 |
| [T23](#t23) | SEO and caching rollout: sitemap, robots, tag-invalidation, ResponsiveImage sweep | 5 |

---

## Phase 1 — Foundation

### T01 — Infrastructure setup: Postgres, volumes, NuGet packages, env scaffolding

<a id="t01"></a>

**Goal.** Bring the deployable shape up to what the rest of the system needs — without writing any application code yet.

**Deliverables.**
- `docker-compose.yml` adds a `postgres:16-alpine` service with healthcheck, `pgdata` named volume, environment vars from `.env`, and `web.depends_on` set to `postgres` `service_healthy`.
- `docker-compose.yml` adds an `uploads` named volume mounted on `web` at `/app/wwwroot/uploads`.
- `.gitignore` adds `RSD.Web/wwwroot/uploads/` (Phase 1 onwards files do not get committed).
- `.env.example` committed at repo root with non-secret keys (DB user, host, port, etc.). `.env` itself stays untracked.
- NuGet packages added to `RSD.Web.csproj`: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.Cookies`, `SixLabors.ImageSharp`, `Ganss.Xss`, `MailKit`.
- `appsettings.json` gets the full configuration tree from spec §8.4 with placeholders for secrets.

**Acceptance criteria.**
- `docker compose up -d` brings both containers up; `web` reports healthy.
- `dotnet build` succeeds with all new packages restored.
- No secrets are checked into the repository.

**Dependencies.** None — first task.

**References.** Spec §8 (auth / config), §9 (phasing), architecture §4 (containers), modules §6.4.

---

### T02 — Persistence foundation: `AppDbContext`, base entities, Identity, initial migration

<a id="t02"></a>

**Goal.** Establish the database layer up to the point where the schema exists in Postgres at startup.

**Deliverables.**
- `Data/AppDbContext.cs` inherits `IdentityDbContext<AdminUser>`; one `DbSet<T>` for every operational entity that exists in Phase 1: `ContactSubmissions`, `AuditLogEntries`, `UploadedFiles`. (Content `DbSet<T>` are introduced in their respective phases — keep the DbContext small now.)
- `Data/Entities/ContentEntity.cs` (abstract `record class` base with `Id`, `Slug`, `Status`, `CreatedAt`, `UpdatedAt`, `PublishedAt`, `IsDeleted`, `Seo`).
- `Data/Entities/SeoMetadata.cs`, `Data/Entities/ContactSubmission.cs`, `Data/Entities/AuditLogEntry.cs`, `Data/Entities/UploadedFile.cs`, `Data/Entities/ImageVariant.cs`.
- `Services/Auth/AdminUser.cs` (`class AdminUser : IdentityUser` with `DisplayName`. Plain `class`, not `record class`, because C# CS8864 disallows records inheriting from non-record base types like `IdentityUser`; CLAUDE.md §3 "complex inheritance" exception applies).
- `Data/Configurations/` with one configuration class per entity introduced here. `OwnsOne` for `SeoMetadata` (deferred — only used by content entities). Indexes for `AuditLogEntries` (`At desc`, `EntityType`, `UserId`).
- Initial migration `0001_Initial.cs` created via `dotnet ef migrations add`.
- `Program.cs` registers `AppDbContext` against the Postgres connection string and runs `Database.Migrate()` on startup, gated by `Database:AutoMigrate` config flag (default true).

**Acceptance criteria.**
- App boots; on first run it creates all tables; on subsequent runs it no-ops.
- `psql` into `postgres` confirms `aspnet_users`, `contact_submissions`, `audit_log_entries`, `uploaded_files` exist with expected columns.
- `ContentEntity.IsDeleted` default verified by type inspection (`bool` with no initializer is `false` by language definition). Formal unit test deferred to T07 where the test project is established — see T07 coverage list.

**Dependencies.** T01.

**References.** Spec §4 (project structure), §5.1 (base type), §5.3 (operational tables). Modules §4.1, §4.2, §4.3.

---

### T03 — Audit pipeline: interceptor + `IAuditLog` read

<a id="t03"></a>

**Goal.** Make every entity change traceable, automatically and atomically.

**Deliverables.**
- `Data/Interceptors/AuditSaveChangesInterceptor.cs` — inherits `SaveChangesInterceptor`, overrides `SavingChangesAsync`. Walks `ChangeTracker.Entries()`, derives the `Action` (Create / Update / Publish / Unpublish / Archive / Delete / Restore) from `EntityState` and `Status` transitions, builds a minimal JSON diff (changed fields only, with sensitive properties excluded by name list).
- Adds `AuditLogEntries` rows inside the same `SaveChanges` so they participate in the same transaction.
- Reads current user from injected `IHttpContextAccessor`; falls back to `"system"` for hosted-service-originated saves.
- `Services/Audit/IAuditLog.cs` + `AuditLog.cs` — read-only adapter exposing `ListAsync(AuditQuery, ct)`.
- `Services/Audit/AuditQuery.cs`, `Services/Audit/AuditAction.cs` (enum), `Services/Audit/AuditDiff.cs` (helpers).
- Interceptor registered in DI; attached to `AppDbContext` via `AddDbContext(..., options => options.AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>()))`.

**Acceptance criteria.**
- Integration test: modify a seed-row via `AppDbContext` → audit row appears with correct user, action, and diff.
- Failing audit write degrades to a warning log; the main save still commits.
- Excluded property list covers `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`.

**Dependencies.** T02.

**References.** Spec §5.6, ADR-006 in architecture §11. Modules §3.5, §4.4.

---

### T04 — Service abstractions: Storage, Imaging, Slugs, Cache, Email, Preview

<a id="t04"></a>

**Goal.** Build the cross-cutting services that every content slice will lean on. None require entity-specific knowledge; they are deliberately built before any entity work.

**Deliverables.**
- `Services/Storage/IFileStorage.cs` + `LocalDiskFileStorage.cs`. Saves under `wwwroot/uploads/{entity}/{yyyy}/{mm}/{guid}-{suffix}.{ext}`; returns `StoredFile`.
- `Services/Imaging/IImageProcessor.cs` + `ImageSharpProcessor.cs` + `SvgSanitizer.cs`. Emits WebP at 480 / 1024 / 1920 px, never upscales, preserves EXIF orientation, strips other EXIF. SVGs sanitized via `Ganss.Xss` SVG profile.
- `Services/Slugs/ISlugger.cs` + `Slugger.cs`. Pure `Slugify` for transliteration + normalization; `GenerateUniqueAsync<TEntity>` for collision resolution.
- `Services/Cache/IPublicPageCache.cs` + `OutputCacheAdapter.cs` + `CacheTags.cs` static helper.
- `Services/Email/IEmailSender.cs` + `SmtpEmailSender.cs` (MailKit) + `LoggingEmailSender.cs` (dev) + `Services/Email/EmailTemplates/` with `ForgotPasswordTemplate`, `UserInviteTemplate`, `ContactSubmissionTemplate`.
- `Services/Preview/IPreviewTokenSigner.cs` + `HmacPreviewTokenSigner.cs` + `PreviewClaims.cs`.
- `Services/Common/Result.cs` (`record Result<T>(bool Ok, T? Value, string Error)`, `Unit` sentinel).
- DI extension methods: `AddRsdStorage`, `AddRsdImaging`, `AddRsdSlugs`, `AddRsdCache`, `AddRsdEmail`, `AddRsdPreview`. Wired up in `Program.cs`.

**Acceptance criteria.**
- Unit tests cover: slug transliteration of Cyrillic input, slug collision suffixing, HMAC sign+verify (incl. tamper rejection), image variant path generation, WebP variant emission for a sample PNG, SVG sanitization stripping `<script>`.
- `LoggingEmailSender` is bound when `ASPNETCORE_ENVIRONMENT=Development`; `SmtpEmailSender` otherwise.

**Dependencies.** T02 (for `ISlugger` DB query; everything else is independent of T02).

**References.** Spec §7 (image pipeline), §8.3 (email), §8 (preview). Modules §3.2–§3.9.

---

### T05 — Authentication: Identity wiring, AdminBootstrapper, login + password-reset pages

<a id="t05"></a>

**Goal.** A real admin can log in. No content management yet — but the auth seam is fully operational.

**Deliverables.**
- `Program.cs` adds Identity (`AddIdentity<AdminUser, IdentityRole>` with default Microsoft password policy, lockout 5/15 min), cookie auth (`HttpOnly`, `Secure`, `SameSite=Lax`, sliding 30 days), authorization, `[Authorize(Roles = "Admin")]` policy.
- `Services/Auth/AdminBootstrapper.cs` — `IHostedService`; runs after migrations; idempotent (no-op if any user exists); reads `RSD_BOOTSTRAP_ADMIN_EMAIL` and `RSD_BOOTSTRAP_ADMIN_PASSWORD`; creates an admin user and adds to `Admin` role.
- `Services/Auth/AdminUserClaimsTransformer.cs` adds `DisplayName` claim.
- `Components/Admin/Pages/Login/Login.razor` (+ `.razor.cs`) — email/password form; rate-limited; redirects to `/admin` on success.
- `Components/Admin/Pages/ForgotPassword/ForgotPassword.razor` + reset email via `IEmailSender`.
- `Components/Admin/Pages/ResetPassword/ResetPassword.razor` — handles the token from the email link.
- `Components/Admin/Pages/Logout.razor` (or a `MapPost("/admin/logout")` handler).

**Acceptance criteria.**
- Start with empty DB + valid bootstrap env vars → can log in as that admin.
- Wrong password 5 times → account locked for 15 min.
- Forgot-password flow round-trips: request → email received (LoggingEmailSender in dev) → set new password → log in.
- Visiting `/admin/...` while unauthenticated redirects to `/admin/login?returnUrl=...`.

**Dependencies.** T02, T04 (email).

**References.** Spec §8.1, §8.2. Modules §3.8.

---

### T06 — Admin shell: layout, sidebar, top bar, shared UI primitives

<a id="t06"></a>

**Goal.** Land the visual chrome and the shared component library every entity page will use. Stubs in place; no entity functionality yet.

**Deliverables.**
- `Components/Admin/Layout/AdminLayout.razor` + `.razor.cs` — `@inherits LayoutComponentBase`, `@attribute [Authorize(Roles = "Admin")]`, slots in sidebar + top bar.
- `Components/Admin/Layout/AdminNavbar.razor` — top bar with user email, "View site ↗", sign-out form.
- `Components/Admin/Layout/AdminSidebar.razor` (+ `.razor.cs`) — driven by a static `IReadOnlyList<AdminNavItem>` record list. Nav items for the planned routes are placeholder stubs at this stage.
- "Coming soon" stub pages for every sidebar destination so navigation works end-to-end.
- `Components/Admin/Shared/`:
  - `AdminDataTable<T>.razor` (+ `.razor.cs`) — generic, pagination, sort, filter chips.
  - `StatusBadge.razor` (+ `.razor.cs`).
  - `SlugField.razor` (+ `.razor.cs`) — title-derived with lock toggle; async uniqueness check (against `ISlugger`).
  - `ImageUploader.razor` (+ `.razor.cs`) — drag-drop, uses `IFileStorage` + `IImageProcessor`.
  - `ResponsiveImage.razor` (+ `.razor.cs`) — `(UploadedFile file, ImageRole role)` → correct `<picture>`/`<img>`.
  - `ConfirmDialog.razor`, `Toast.razor`, `ToastHost.razor`.
- `wwwroot/js/admin/` — `image-uploader.js` for drop-handling. (Quill module deferred to T17.)

**Acceptance criteria.**
- Logged-in admin lands on `/admin`, sees the populated sidebar, can click each item and see the "Coming soon" stub.
- `ImageUploader` can upload a PNG; file appears in `wwwroot/uploads/_test/{yyyy}/{mm}/...`; three WebP variants are generated.
- `SlugField` validates collision against a manually-inserted row.
- A11y pass: every interactive control has visible label, keyboard focus, and works with VoiceOver.

**Dependencies.** T04, T05.

**References.** Spec §6.1–§6.4. Modules §1.5–§1.7.

---

### T07 — Tests project: xUnit + Testcontainers + initial coverage

<a id="t07"></a>

**Goal.** Create the home for tests and establish the integration-test pattern that future phases will extend.

**Deliverables.**
- `RSD.Web.Tests/RSD.Web.Tests.csproj` referencing `xUnit`, `Testcontainers`, `Testcontainers.PostgreSql`, `Microsoft.NET.Test.Sdk`, `FluentAssertions`, `Bunit` (Bunit suite first used in Phase 4).
- Solution `.sln` updated to include the test project.
- `RSD.Web.Tests/Unit/` with first tests: `SluggerTests`, `HmacPreviewTokenSignerTests`, `ImageSharpProcessorPathTests`, `AuditDiffTests`, `EmailTemplates_RenderTests`, and `ContentEntityDefaultsTests` (covering the deferred T02 acceptance — `IsDeleted` defaults to `false`, `Status` defaults to `Draft`, `CreatedAt`/`UpdatedAt` populate, `Seo` is non-null).
- `RSD.Web.Tests/Integration/` with `PostgresFixture` (singleton Testcontainers Postgres), `AppDbContextFixture` (provides a fresh DB transaction per test, rolled back on dispose). One sample integration test: `AdminBootstrapper_OnEmptyDb_CreatesFirstAdmin`.
- CI placeholder: a `dotnet test` invocation in a single command at the repo root; no GitHub Actions workflow yet (deferred to ops backlog).

**Acceptance criteria.**
- `dotnet test` runs locally; all tests green.
- `PostgresFixture` shares a single container across the assembly; total integration-test runtime stays under 30 s on a baseline laptop.
- Adding a new integration test only requires declaring the fixture, not bringing up Docker manually.

**Dependencies.** T02, T04, T05 (so there's real code to test).

**References.** Spec §11. Modules §5.1.

---

## Phase 2 — Simple entities + Inbox

### T08 — Simple entities domain layer (9 entities)

<a id="t08"></a>

**Goal.** Make the simplest entities first-class in the DB. These have flat schemas, no rich body, and serve as the validation of the entire entity-pattern end-to-end.

**Deliverables.**
- Entity records: `Testimonial`, `TeamMember`, `Partner`, `Value`, `MissionStat`, `TechStackItem`, `ContactPoint`, `MessengerLink`, `SocialLink`. All derive from `ContentEntity` (so they get Slug, Status, Soft-delete, Audit).
- Per-entity `IEntityTypeConfiguration` with partial unique index on `Slug WHERE NOT IsDeleted` and ordinary indexes on `DisplayOrder` where the entity has one.
- Migration `0002_SimpleEntities.cs`.
- Per-entity service interface and implementation in `Services/Content/`, each conforming to a small variant of `IContentService` (no rich body; no SEO panel — these entities don't ship with `SeoMetadata` content fields populated by default).
- Seeders for each entity in `Data/Seed/`, pulling sample data verbatim from today's hard-coded `.razor.cs` arrays. Seeder is idempotent (skips if any rows of that entity exist).
- `SeedRunner` orchestrator wired into `Program.cs` as an `IHostedService` that runs after migrations.

**Acceptance criteria.**
- Fresh DB after migration is seeded with current sample data; row counts match today's hard-coded arrays.
- Integration test per entity: list, create, update, soft-delete, restore.
- Slug uniqueness enforced — attempting to insert a duplicate non-deleted slug fails at DB level.
- Audit rows appear for every create/update.

**Dependencies.** T02, T03, T04, T07.

**References.** Spec §5.2 (table rows), §9 (Phase 2). Modules §3.1, §4.2, §4.6.

---

### T09 — Simple entities admin CRUD pages

<a id="t09"></a>

**Goal.** Editors can manage all 9 simple entities through the admin UI.

**Deliverables.**
- `Components/Admin/Pages/{Testimonials,Team,Partners,Values,Stats,Tech,Contact,Messenger,Social}/` each containing `*List.razor` (+ `.razor.cs`) and `*Edit.razor` (+ `.razor.cs`).
- List pages use `AdminDataTable<T>`, support search by primary text field, soft-delete from row actions.
- Edit pages use `ImageUploader` for image-bearing entities; entity-specific form fields are plain Blazor inputs.
- Sidebar entries in `AdminSidebar.razor.cs` updated to point at the new pages.
- Display-order management: each list page has a drag-handle column for `DisplayOrder` reorder, persisting via a single `BulkReorderAsync` service call.

**Acceptance criteria.**
- An editor can create, edit, soft-delete, and reorder each entity type.
- Toast confirms success; errors surface inline with a clear message.
- All form fields are labeled and keyboard-navigable.

**Dependencies.** T06, T08.

**References.** Spec §6.2. Modules §1.6.

---

### T10 — Public site consumes simple entities from DB

<a id="t10"></a>

**Goal.** Replace today's hard-coded sample arrays in the public Razor sections with service-backed reads.

**Deliverables.**
- `Components/Sections/Home/TestimonialsSection.razor.cs` and `TestimonialsCarouselSection.razor.cs` call `ITestimonialService` in `OnInitializedAsync`.
- Same pattern applied to: `Sections/About/TeamSection`, `ManagementSection`, `PartnersSection`, `ValuesSection`, `MissionSection`; `Sections/Services/TechStackSection`; `Sections/Contact/ContactSection` for `ContactPoint`/`MessengerLink`/`SocialLink`.
- Each section uses `ResponsiveImage` for any image rendering instead of hard-coded `<img src="...">`.
- The original sample-data record types in these `.razor.cs` files are deleted; the new types come from `Data/Entities/`.

**Acceptance criteria.**
- Each affected public page renders identically to today (visual diff is null), but its data flows from Postgres.
- Toggling a testimonial's `Status` to `Archived` in admin → it disappears from the home page within the output-cache TTL (or immediately after explicit invalidation, which T23 fully wires).

**Dependencies.** T08, T09.

**References.** Spec §9 (Phase 2 demo). Modules §1.3.

---

### T11 — Contact-form pipeline: submissions, endpoint, inbox, SMTP

<a id="t11"></a>

**Goal.** Capture leads from the public Contact form, route them to admin Inbox and to a configured email address.

**Deliverables.**
- `Endpoints/ContactSubmitEndpoint.cs` — `POST /api/contact`, validates the request DTO, runs honeypot check (`Hp` field must be empty), enforces per-IP rate limit (5 requests / 5 min via `RateLimiter`).
- `Services/Content/ContactSubmissionService.cs` writes to `ContactSubmissions` and fires the SMTP notification asynchronously (best-effort; DB record is the source of truth).
- `Services/Email/EmailTemplates/ContactSubmissionTemplate.cs` renders subject/html/text.
- `Components/Admin/Pages/Inbox/InboxList.razor` (+ `.razor.cs`) — paginated list, filter `All / Open / Handled`, click opens a detail drawer with `mailto:` reply button, mark-as-handled, soft-delete.
- `Components/Sections/Contact/ContactSection.razor.cs` wires the existing form to `POST /api/contact`; success state and error state UI added.

**Acceptance criteria.**
- Submitting the contact form on `/contact` inserts a row in `contact_submissions` and triggers the email template (visible in `LoggingEmailSender` output in dev).
- Honeypot triggered → 200 OK silently; DB unchanged.
- Rate limit exceeded → 429 with a polite retry-after.
- Inbox admin view paginates correctly with 100+ rows.

**Dependencies.** T08, T09, T04 (email).

**References.** Spec §6.5, §9 (Phase 2). Modules §2.1, §3.1.

---

## Phase 3 — List rows for Blog/Cases/Products/Services

### T12 — Blog/Cases/Products/Services domain layer (header + seeded body)

<a id="t12"></a>

**Goal.** Bring the four major content types into the DB. Header fields are first-class; body fields exist on the schema but stay frozen at seed values until Phase 4.

**Deliverables.**
- `Data/Entities/BlogPost.cs`, `Case.cs`, `Product.cs`, `Service.cs`. Header fields per spec §5.2; body fields typed (`ArticleBody` for Blog/Service, `CaseDetailFields` / `ProductDetailFields` for Case/Product) but written via seeders only.
- `Data/Entities/CaseDetailFields.cs`, `ProductDetailFields.cs`, `ArticleBody.cs`, `ArticleBlock.cs` and the seven concrete block records under `Data/Entities/ArticleBlocks/`. `[JsonPolymorphic]` + `[JsonDerivedType]` on `ArticleBlock`.
- `Data/Entities/BadgePill.cs`, `MetaItem.cs`, `ChallengeHurdle.cs`, `MetricCallout.cs`, `EmbeddedTestimonial.cs`, `TwoColumnText.cs`, `SubsectionItem.cs`, `StatRowItem.cs`, `GalleryImage.cs`.
- Configurations with `HasConversion` for `jsonb` body fields using a shared `JsonbValueConverter<T>`; `OwnsOne(e => e.Seo)`; partial unique index on `Slug`.
- Migration `0003_MainContent.cs`.
- Services in `Services/Content/`: `BlogService`, `CaseService`, `ProductService`, `ServiceService`. Implement the full `IContentService<TListItem, TDetail, TUpsert>` contract.
- Seeders in `Data/Seed/`: extract today's hard-coded list + detail content verbatim from `PostsGridSection.razor.cs`, `CasesGridSection.razor.cs` + `CaseDetail.razor.cs`, `ProductsListSection.razor.cs` + `ProductDetail.razor.cs`, `FeaturesSection.razor.cs` + `ServiceDetail.razor.cs`.

**Acceptance criteria.**
- Seeded DB has the same 9 blog posts, 6 cases, 3 products, 6 services as today's hard-coded arrays.
- Round-trip serialization test: `CaseDetailFields` and `ArticleBody` survive write→read with all data preserved.
- Integration test for each service covers all CRUD methods.

**Dependencies.** T08.

**References.** Spec §5.2, §5.4. Modules §3.1, §4.2.

---

### T13 — Blog/Cases/Products/Services admin: list + header-only edit

<a id="t13"></a>

**Goal.** Editors can manage the list-level metadata (title, slug, cover, description, tags, SEO, status). Body editing comes in Phase 4.

**Deliverables.**
- `Components/Admin/Pages/{Blog,Cases,Products,Services}/` each with `*List.razor` and `*Edit.razor`.
- Edit pages use `SlugField`, `ImageUploader` for cover, `SeoMetaPanel` for SEO, status switcher; body fields are non-editable in this phase (placeholder note: "Body content editor coming in Phase 4").
- `Components/Admin/Shared/SeoMetaPanel.razor` (+ `.razor.cs`) introduced here, plugging into `Seo` owned type.
- Sidebar entries updated.
- Tag input control for `Tags string[]` on blog posts and `TechTags string[]` on cases.

**Acceptance criteria.**
- An editor can create a new blog post with title + slug + cover + SEO + tags; saving makes it visible on the public list page after cache invalidation.
- Duplicate slug attempt is rejected by `SlugField` before submit.
- SEO meta panel persists meta-title, meta-description, OG image path.

**Dependencies.** T06, T12.

**References.** Spec §6.2 (per-entity pattern). Modules §1.6.

---

### T14 — Public list pages + detail-page header lookup

<a id="t14"></a>

**Goal.** Public list pages render from DB. Detail pages render header from DB; body still comes from seeded jsonb (no editing yet).

**Deliverables.**
- `Components/Pages/Blog.razor` / `Cases.razor` / `Products.razor` / `Services.razor` query the relevant service for published rows in display order; replace hard-coded arrays.
- `Components/Pages/BlogDetail.razor` / `CaseDetail.razor` / `ProductDetail.razor` / `ServiceDetail.razor` (+ their `.razor.cs`) load via `GetBySlugAsync(slug, includeDrafts: false)`. 404 for unknown / unpublished slugs (`HttpContext` → `Response.StatusCode = 404`).
- All `<img>` paths in these public pages flow through `ResponsiveImage`.

**Acceptance criteria.**
- `/blog`, `/cases`, `/products`, `/services` visually identical to today.
- `/blog/cloud-infrastructure-scaling-2026`, `/cases/healthcare-plus`, `/products/nexacrm`, `/services/cloud-solutions` render as before (header from DB, body still from seeded jsonb).
- Creating a new published row in admin makes it appear on the list and detail.

**Dependencies.** T13.

**References.** Spec §9 (Phase 3 demo). Modules §1.2.

---

## Phase 4 — Rich detail bodies

### T15 — Typed-body editors for Case and Product details

<a id="t15"></a>

**Goal.** Editors can compose Case and Product detail bodies entirely through the admin, with no body-content drift possible from the design.

**Deliverables.**
- `Components/Admin/Shared/RepeaterField.razor` (+ `.razor.cs`) — generic, takes a `List<TRow>`, renders one sub-form per row using a `RenderFragment<TRow>`, supports add/remove/reorder via drag handle.
- `Components/Admin/Pages/Cases/CaseBodyEditor.razor` (+ `.razor.cs`) — fieldset groups for Badges / MetaTags / Meta / Hurdles / Results / TechPills / Metrics / EmbeddedTestimonial / Conclusion. Each list uses `RepeaterField<TRow>`.
- `Components/Admin/Pages/Products/ProductBodyEditor.razor` — same shape over `ProductDetailFields`.
- `CaseEdit.razor` and `ProductEdit.razor` now embed the body editor below the header section.

**Acceptance criteria.**
- Editor creates a Case with 3 Hurdles, 5 Results, 2 Metrics, an EmbeddedTestimonial; saves; reopens; data round-trips.
- Reorder of a Hurdle persists.
- Form is keyboard-navigable; tab order is logical.

**Dependencies.** T13.

**References.** Spec §6.3 (typed body editor). Modules §1.7.

---

### T16 — Block-body editor for Blog and Service details

<a id="t16"></a>

**Goal.** Editors can compose Blog and Service detail bodies as ordered, typed block lists.

**Deliverables.**
- `Components/Admin/Shared/BlockListEditor.razor` (+ `.razor.cs`) — vertical list of block cards, each with: type label, compact preview, drag-handle reorder, expand-to-edit, delete. `+ Add block` opens a typed palette (Subsection / StatsRow / Gallery / BulletList / Quote / Image / RichText).
- One sub-editor per block type as a small partial component inside `BlockListEditor` (`SubsectionBlockEditor`, `StatsRowBlockEditor`, `GalleryBlockEditor`, `BulletListBlockEditor`, `QuoteBlockEditor`, `ImageBlockEditor`, `RichTextBlockEditor` — the last consumes the `RichTextEditor` from T17).
- `Components/Admin/Pages/Blog/BlogBodyEditor.razor` and `Services/ServiceBodyEditor.razor` embed `BlockListEditor` plus an `Intro` `RichTextEditor`.
- Public-side block rendering already exists in `Components/Sections/Article/`; wire it to render from `ArticleBody.Blocks` via a dispatcher component (`ArticleBodyRenderer.razor`).

**Acceptance criteria.**
- Editor creates a new blog post from scratch with every block type at least once; saves; previews; published page matches the design quality of the seeded example.
- Reordering blocks persists.
- Removing a block does not orphan its `ImagePath`/`AvatarPath` refcounts (T21 enforces, T16 keeps things consistent at the service layer).

**Dependencies.** T13, T17.

**References.** Spec §6.3 (block-list body editor). Modules §1.7.

---

### T17 — `RichTextEditor`: Quill JS interop + server-side HTML sanitization

<a id="t17"></a>

**Goal.** A reusable WYSIWYG editor with a strict trust boundary: HTML out of the editor is always sanitized server-side before persistence.

**Deliverables.**
- `Components/Admin/Shared/RichTextEditor.razor` (+ `.razor.cs`) — wraps a `<div>` initialized by Quill via JS interop; emits `ValueChanged(string)` on debounce.
- `wwwroot/js/admin/quill-interop.js` — initializes Quill with a configured toolbar (`bold`, `italic`, `underline`, `link`, `header 2/3`, `bullet list`, `ordered list`, `clean`). No image-in-content for v1 — images go through `ImageBlock` in `BlockListEditor`.
- Server-side: `Services/Content/HtmlSanitizer.cs` (or in `Services/Common/`) wrapping `Ganss.Xss` with a configured whitelist; every content service that accepts HTML fields routes through it before persistence.
- A single Bunit test confirms `RichTextEditor` reacts to server-side value updates (component-level only; the Quill side is covered by visual review).

**Acceptance criteria.**
- Pasting `<script>alert('x')</script>` into the editor never persists the script tag — sanitizer strips it server-side.
- Toolbar formatting (bold, link, bullet list) round-trips.
- Field is keyboard-accessible; tab moves focus, shift-tab moves it back; toolbar buttons are reachable.

**Dependencies.** T06.

**References.** Spec §4 (Quill), §9.4 (sanitization). Modules §1.7.

---

### T18 — Preview pipeline: HMAC tokens, `/preview/...` route, edit-page buttons

<a id="t18"></a>

**Goal.** Editors can preview Draft entries exactly as they will render when published, via a signed URL that bypasses the `Status == Published` filter.

**Deliverables.**
- `Endpoints/PreviewEndpoint.cs` — `GET /preview/{type}/{slug}` accepts `?token=...`. Verifies the token via `IPreviewTokenSigner`; on success, sets a request-scoped flag and delegates to the public detail rendering for that entity type; on failure, 404.
- A small `IPreviewContext` scoped service that the relevant content services check (`GetBySlugAsync(slug, includeDrafts: ctx.IsPreview, ct)`).
- Edit-page "Preview ↗" button on `BlogEdit`, `CaseEdit`, `ProductEdit`, `ServiceEdit`. Button calls `IPreviewTokenSigner.Sign(...)` and opens the URL in a new tab.
- Configuration: `Preview:SigningKey` (env-driven), `Preview:TtlMinutes` (default 60).

**Acceptance criteria.**
- Click "Preview" on a Draft blog post → opens `/preview/blog/{slug}?token=...` → renders as if published.
- Expired token → 404.
- Tampered token → 404.
- Rotating `Preview:SigningKey` immediately invalidates outstanding preview URLs.

**Dependencies.** T13, T14.

**References.** Spec §6.6, §7.4 (sequence). Modules §3.9.

---

## Phase 5 — Operations polish

### T19 — Audit log viewer (`/admin/audit`)

<a id="t19"></a>

**Goal.** Make the audit data, which has been accumulating since Phase 1, actually visible and useful.

**Deliverables.**
- `Components/Admin/Pages/Audit/AuditList.razor` (+ `.razor.cs`) — paginated; filters: user, entity type, action, date range; row expands to show the JSON diff via `AuditDiffViewer`.
- `Components/Admin/Shared/AuditDiffViewer.razor` — pretty-prints a JSON diff with field-level red/green highlighting.
- Sidebar entry.

**Acceptance criteria.**
- 1000-row audit table renders in under 200 ms.
- Filters compose (user + action + date range).
- Diff viewer correctly displays nested `jsonb` body changes (e.g. an `ArticleBody.Blocks` reorder).

**Dependencies.** T03.

**References.** Spec §6.5. Modules §1.6.

---

### T20 — Trash UI (`/admin/trash`)

<a id="t20"></a>

**Goal.** Make soft-deleted entities recoverable; allow permanent deletion when desired.

**Deliverables.**
- `Components/Admin/Pages/Trash/TrashList.razor` (+ `.razor.cs`) — combined view of soft-deleted rows across all entity types (`UNION` via a small `TrashService` that queries each entity with `IgnoreQueryFilters`).
- Row actions: Restore (clears `IsDeleted`, status restored to `Draft`), Hard Delete (with `ConfirmDialog` requiring typed confirmation).
- Sidebar entry.

**Acceptance criteria.**
- Soft-deleted blog post appears in Trash within a single round-trip after deletion.
- Restore makes the row visible again in `BlogList` as Draft.
- Hard delete removes the row and (T21-aware) decrements file refcounts.

**Dependencies.** T08, T12.

**References.** Spec §6.5. Modules §1.6.

---

### T21 — Media library (`/admin/media`) with reference tracking

<a id="t21"></a>

**Goal.** Editors can audit uploaded files, see where each is used, and safely remove orphans.

**Deliverables.**
- `Components/Admin/Pages/Media/MediaGrid.razor` (+ `.razor.cs`) — paginated grid of `UploadedFiles`; search by original name and content type; click opens a detail panel.
- Detail panel shows: image preview (large variant), original metadata, `RefCount`, "Used by" list (queries every content service for entities that reference this `Path`).
- Hard delete blocked while `RefCount > 0`; soft delete not applicable here (files are operational data, not content entities).
- Refcount maintenance: every content service that takes a `*Path` increments on save, decrements on remove/replace. A nightly recomputation job is left to ops (`Services/Storage/RefCountAuditor.cs` as a CLI-style hosted task disabled by default).

**Acceptance criteria.**
- Uploading a cover and removing it from a blog post within one session leaves `RefCount = 0` and the file deletable.
- Two blog posts sharing one cover image both contribute to `RefCount = 2`; removing one drops it to 1, file still undeletable.
- Search by `original_name LIKE` works in under 100 ms with 10k rows.

**Dependencies.** T08, T12, T13.

**References.** Spec §5.8. Modules §3.2.

---

### T22 — User management (`/admin/users`): invite, disable, reset

<a id="t22"></a>

**Goal.** New admins can be added by existing admins; no ops shell access required.

**Deliverables.**
- `Components/Admin/Pages/Users/UsersList.razor` (+ `.razor.cs`) — list with email, display name, last login, status (active/disabled), row actions.
- `Components/Admin/Pages/Users/InviteUser.razor` — form to invite a new admin; creates an `AdminUser` with a random reset token; emails the invite link.
- Reset-password flow reused from T05.
- Disable user (cannot log in until re-enabled). Disabled-while-current-user check (cannot disable yourself).
- Sidebar entry.

**Acceptance criteria.**
- Existing admin invites a teammate's email → teammate gets the invite email → sets a password → can log in.
- Disabling a user revokes their cookie on next request.
- Cannot disable the last active admin (system check).

**Dependencies.** T05.

**References.** Spec §6.5. Modules §3.8.

---

### T23 — SEO and caching rollout: sitemap, robots, tag-invalidation, ResponsiveImage sweep

<a id="t23"></a>

**Goal.** Land the final SEO + performance pieces and verify cache-invalidation works end-to-end across every entity.

**Deliverables.**
- `Endpoints/SitemapEndpoint.cs` — `GET /sitemap.xml`, served by `ISitemapBuilder` aggregating published Blog, Cases, Products, Services with `lastmod` from `UpdatedAt`. Output cached at the endpoint level with a 60-minute TTL.
- `Endpoints/RobotsEndpoint.cs` — `GET /robots.txt`, served from `IRobotsTxtProvider` (config-driven content).
- `Services/Seo/SitemapBuilder.cs` + `RobotsTxtProvider.cs`.
- Full audit + completion of output-cache tag invalidation: every `IContentService` save/publish/unpublish/delete calls `IPublicPageCache.EvictForAsync<T>(id)` and `EvictListAsync<T>()`. Tag set is documented in `CacheTags.cs`.
- Final sweep across `Components/Sections/` and `Components/Pages/` to ensure every `<img>` uses `ResponsiveImage` (including brand marks, decorative illustrations, and Article-section images).
- README section documenting backup recipes for `pgdata` and `uploads` volumes.

**Acceptance criteria.**
- `GET /sitemap.xml` lists every Published entity with valid `loc` and `lastmod`; updates within 60 min of a publish.
- `GET /robots.txt` returns the configured content.
- Saving any entity in admin → cache invalidates → next public visit renders the new content.
- Lighthouse score on `/`, `/blog`, `/cases`, `/products`, `/services` is ≥ 90 in all categories.

**Dependencies.** T10, T14, T16.

**References.** Spec §6.5 (sitemap, robots), §5.7 (cache). Modules §2.1, §3.7, §3.10.

---

## Out of scope (recap)

The following are explicitly **not** in any of these tasks:

- Multi-language / i18n.
- Scheduled publishing (`PublishAt`).
- Slug redirects on rename.
- Per-entity granular permissions.
- SSO / external IdP.
- Public REST/GraphQL API.
- Horizontal scaling (distributed cache, sticky sessions).
- Automated DB / uploads backups (documented manually in T23).
- Playwright end-to-end UI tests.

See architecture §13 for how each of these slots in later without breaking the v1 design.

---

## Working agreement

- **One PR per phase** (Phase 4 may split into 4a Case+Product, 4b Blog+Service+Preview).
- **Tasks within a phase are sequential** unless explicitly marked otherwise.
- **Each task ends with a verifiable demo** — the acceptance criteria above.
- **No task crosses phase boundaries** — if scope creep tempts you across, stop and update this document first.
- **Every PR honors `CLAUDE.md` conventions**: .NET 9 syntax, records, primary-constructor DI, no nulls in business logic, no primitive obsession, CC ≤ 4, split `.razor`/`.razor.cs`, semantic HTML + WAI-ARIA.
- **Stop after every task and report** — see `CLAUDE.md` §8. When a task's acceptance criteria are met, do not start the next task. Report what was delivered and wait for explicit approval to continue. This applies to every task in this document, no exceptions for "small" or "obvious" next tasks.
