# Backend & Admin Panel — Design Spec

**Date:** 2026-05-12
**Author:** Mark Podlyashetskyi (with Claude)
**Scope:** Add a Postgres-backed content store and an authenticated `/admin/*` panel to the existing RSD marketing site so a small team can CRUD every entity that today exists as hard-coded sample data inside Razor section components.

---

## 1. Background

`RSD.Web` is a Blazor Server site on .NET 9 with no database, no API, and no authentication. All public-facing copy, images, and structured content (blog posts, case studies, products, services, testimonials, team, partners, values, etc.) is hard-coded as C# `record`-typed sample arrays inside `.razor.cs` section components. Detail pages exist for exactly one example slug per type (`/cases/healthcare-plus`, `/blog/cloud-infrastructure-scaling-2026`, etc.); all other slugs 404.

The site needs an admin so that:
- Non-developers can publish, edit, archive, and delete content without touching code.
- Detail pages stop being one-off coded examples and become real `{slug}` lookups against a content store.
- Marketing assets (cover images, avatars, logos, icons) can be uploaded and replaced by editors.
- Contact-form submissions are captured and routed to a configured email.

This spec covers the full design. Implementation is broken into five phases — each phase is a mergeable PR that leaves the site working end-to-end.

## 2. Goals and non-goals

**Goals**
- One Docker container deploy (web), plus a Postgres container — no separate API process.
- Full CRUD for 13 entity types covering every piece of currently-hard-coded content.
- Draft / Published / Archived workflow with a signed preview URL for drafts.
- Local-disk file uploads behind an `IFileStorage` abstraction, with ImageSharp generating three WebP variants (small/medium/large) on upload.
- ASP.NET Identity email/password auth, single `Admin` role, default Microsoft password policy.
- Soft delete + audit log across all content entities.
- Auto sitemap.xml, robots.txt, per-entity SEO meta, output cache with tag invalidation, slug uniqueness, contact-form inbox + SMTP notification.
- Seed today's hard-coded sample data on first run so the public site never goes blank.
- All conventions from `CLAUDE.md` (records, primary-constructor DI, no nulls in business logic, no primitive obsession, CC ≤ 4, split .razor/.razor.cs) honored throughout.

**Non-goals (deliberately deferred)**
- Multi-language / i18n. Public site stays English-only; we don't model `ITranslatable<>` up front.
- Scheduled publishing (`PublishAt` + background job).
- `SlugRedirects` for renamed slugs.
- Per-entity granular permissions; everyone with an account has full edit rights.
- SSO / external identity provider.
- Headless API for third-party consumers.
- Horizontal scaling concerns (the output cache is in-memory; revisit if we ever run more than one container).
- Automated DB / uploads backups (operational concern, documented as a manual recipe only).

## 3. Decisions

| Topic | Decision |
|---|---|
| Auth | ASP.NET Identity, email/password, single shared `Admin` role. Default Microsoft password policy. Lockout after 5 failures in 15 min. |
| Topology | Same `RSD.Web` project, `/admin/*` routes. |
| Database | PostgreSQL via EF Core (`Npgsql.EntityFrameworkCore.PostgreSQL`). |
| File storage | Local disk under `wwwroot/uploads/`, mounted as a Docker volume. `IFileStorage` abstraction so we can swap to S3/Azure Blob later. |
| Scope of v1 | All 13 entity types; shipped simple-first. |
| Content model | Hybrid: typed fields where templates are fixed (Case / Product detail); typed header + ordered block list where bodies are article-like (Blog / Service detail). |
| Publish flow | Draft → Published → Archived, plus a signed Preview URL for Draft entries. |
| Localization | English only. |
| Contact form | DB + SMTP notification (admin Inbox view). |
| Rich text | Quill-based `RichTextEditor` Blazor component, JS interop, server-side HTML sanitization via `Ganss.Xss`. |
| Admin shell | Tailwind + Flowbite, denser admin layout. No new Blazor UI library. |
| SMTP provider | Code against `IEmailSender` only; pick concrete provider at deploy. Dev uses `LoggingEmailSender`. |
| First admin | Bootstrapped on empty DB from `RSD_BOOTSTRAP_ADMIN_EMAIL` + `RSD_BOOTSTRAP_ADMIN_PASSWORD` env vars (idempotent). |
| Default email-from / contact-to | `mark.podlyashetskyi@remsoft.dev` in appsettings; overridable per environment. |
| Tests | `RSD.Web.Tests` xUnit project added in Phase 1, grows with each phase. |
| Image variants | ImageSharp emits WebP at 480 / 1024 / 1920 px max-width (small / medium / large). Originals preserved for reprocessing. SVG passes through a sanitizer unchanged. |
| Audit, soft delete, output cache, sitemap, slug uniqueness, SEO meta, image processing, seed data | All included in v1. |

## 4. Project structure

Everything stays inside `RSD.Web`:

```
RSD.Web/
├── Components/
│   ├── Admin/                  NEW — admin pages, layout, shared admin components
│   │   ├── Layout/             AdminLayout, AdminNavbar, AdminSidebar
│   │   ├── Pages/              One folder per entity: Blog/, Cases/, Products/, …
│   │   │                       Plus: Login/, Inbox/, Media/, Audit/, Trash/, Users/
│   │   └── Shared/             AdminDataTable<T>, FormField, ImageUploader,
│   │                           RichTextEditor, BlockListEditor, RepeaterField<T>,
│   │                           SeoMetaPanel, SlugField, StatusBadge, ConfirmDialog,
│   │                           Toast, ToastHost, ResponsiveImage (used on public side too)
│   └── (existing public Pages, Sections, Layout, Shared)
├── Data/                       NEW
│   ├── AppDbContext.cs
│   ├── Entities/               One file per entity: BlogPost.cs, Case.cs, Product.cs, …
│   │                           Plus base ContentEntity, owned SeoMetadata, ArticleBody/
│   │                           ArticleBlock derived types, CaseDetailFields,
│   │                           ProductDetailFields.
│   ├── Configurations/         IEntityTypeConfiguration<T> per entity.
│   ├── Interceptors/           AuditSaveChangesInterceptor.
│   ├── Migrations/             EF Core code-first migrations.
│   └── Seed/                   One idempotent seeder per entity; SeedRunner orchestrator.
├── Services/                   NEW — capability-grouped, primary-constructor DI
│   ├── Content/                BlogService, CaseService, ProductService, ServiceService,
│   │                           TestimonialService, TeamMemberService, PartnerService,
│   │                           ValueService, MissionStatService, TechStackService,
│   │                           ContactPointService, MessengerLinkService, SocialLinkService.
│   │                           Each implements IContentService<TListItem, TDetail>
│   │                           where the entity has both shapes; flat ones implement
│   │                           a simpler ISimpleContentService<T>.
│   ├── Storage/                IFileStorage, LocalDiskFileStorage.
│   ├── Imaging/                IImageProcessor, ImageSharpProcessor, ImageVariant record.
│   ├── Slugs/                  ISlugger, Slugger.
│   ├── Audit/                  IAuditLog, audit diff helpers.
│   ├── Email/                  IEmailSender, SmtpEmailSender, LoggingEmailSender (dev),
│   │                           EmailTemplates (forgot-password, contact-notification,
│   │                           user-invite).
│   ├── Cache/                  IPublicPageCache wrapping IOutputCacheStore.
│   ├── Auth/                   AdminUser : IdentityUser, AdminUserClaimsTransformer,
│   │                           AdminBootstrapper.
│   ├── Preview/                IPreviewTokenSigner (HMAC, short TTL).
│   └── Seo/                    ISitemapBuilder, IRobotsTxtProvider.
├── Endpoints/                  NEW — minimal API endpoints not served as Blazor
│   ├── SitemapEndpoint.cs      GET /sitemap.xml
│   ├── RobotsEndpoint.cs       GET /robots.txt
│   └── ContactSubmitEndpoint.cs POST /api/contact (honeypot + rate-limit)
└── (existing Pages, Sections, Layout, Styles, wwwroot, Program.cs, etc.)

RSD.Web.Tests/                  NEW solution project — xUnit + Testcontainers
├── Unit/
└── Integration/
```

`docker-compose.yml` grows from one service to two:
- `web` — existing build, now also mounts `uploads:/app/wwwroot/uploads`.
- `postgres` — official `postgres:16-alpine`, named volume `pgdata`, healthcheck, env vars from `.env`. `web` `depends_on: { postgres: { condition: service_healthy } }`.

## 5. Data model

### 5.1 Base type and owned SEO

```csharp
public abstract record class ContentEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Slug { get; set; }            // unique per concrete type
    public ContentStatus Status { get; set; }            // Draft / Published / Archived
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public bool IsDeleted { get; set; }                  // soft delete
    public SeoMetadata Seo { get; set; } = new();        // owned type
}

public enum ContentStatus { Draft, Published, Archived }

public record class SeoMetadata
{
    public string MetaTitle { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public string OgImagePath { get; set; } = "";   // "" means "no OG image; use default"
}
```

Soft delete is enforced globally via an EF query filter `where !e.IsDeleted`; admin Trash view bypasses with `IgnoreQueryFilters()`.

**Nullable convention.** Per CLAUDE.md §5, nullable strings are avoided in business logic. FK-like ID columns (`OgImagePath`, `HandledByUserId`, `UploadedByUserId`, `AuditLogEntry.UserId`) use empty-string sentinels — `""` means "no reference". `DateTime?` is retained where there is no natural sentinel for "this never happened yet" (`PublishedAt`, `HandledAt`); these are domain-semantic nullables and are an accepted extension of CLAUDE.md §5's allowed-nullable list.

### 5.2 Tables — content entities

| Table | Fields (beyond ContentEntity base where applicable) |
|---|---|
| `BlogPosts` | `Title, Description, Category, AuthorId (FK → TeamMembers, nullable), CoverImagePath, ReadTimeMinutes, Tags string[], Intro string, BodyBlocks jsonb (ArticleBody)` |
| `Cases` | `Name, Industry, Description, CoverImagePath, TechTags string[], DetailFields jsonb (CaseDetailFields)` |
| `Products` | `Name, Subtitle, Price, Description, BulletPoints string[], CoverImagePath, TryForFreeHref, LearnMoreHref, DetailFields jsonb (ProductDetailFields)` |
| `Services` | `Title, Description, BulletPoints string[], CoverImagePath, DetailsHref, Intro string, BodyBlocks jsonb (ArticleBody)` |
| `Testimonials` | `Title, Quote, AvatarPath, AuthorName, AuthorRole, DisplayOnHome bool, DisplayOrder int` |
| `TeamMembers` | `Name, Role, AvatarPath, DisplayOrder, IsManagement bool` (single table covers Team + Management views) |
| `Partners` | `Name, Role, PhotoPath, ContactHref, DisplayOrder` |
| `Values` | `Title, Description, IconPath, DisplayOrder` |
| `MissionStats` | `Number, Symbol, Label, DisplayOrder` |
| `TechStackItems` | `Label, LogoPath, DisplayOrder` |
| `ContactPoints` | `Label, Lines string[], IsLink bool, DisplayOrder` |
| `MessengerLinks` | `Label, LargeIconPath, SmallIconPath, BgColor, Href, DisplayOrder` |
| `SocialLinks` | `Label, IconPath, Href, Scope` (scope: `Footer` / `Contact` / `Management`) |

The "non-content" entities (`Testimonials` through `SocialLinks`) inherit from `ContentEntity` for consistency (slug, status, soft-delete, audit), but their admin forms default `Status = Published` since these are typically always live.

### 5.3 Tables — operational

| Table | Purpose |
|---|---|
| `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc. | ASP.NET Identity. |
| `ContactSubmissions` | `Id, Name, Email, Subject, Message, SubmittedAt, IsHandled, HandledByUserId (string, "" = unhandled), HandledAt (DateTime?)` |
| `AuditLogEntries` | `Id, UserId (string, "" = system), UserEmail, EntityType, EntityId, Action (Create/Update/Publish/Unpublish/Archive/Delete/Restore), Diff jsonb, At` |
| `UploadedFiles` | `Id, Path, OriginalName, ContentType, Bytes, UploadedByUserId (string, "" = system), UploadedAt, Variants jsonb, RefCount int` |

### 5.4 The jsonb-backed bodies

Case and Product detail templates are visually fixed — admins fill out a known set of typed groups. We model them as plain C# records serialized to a single `jsonb` column on the row. Editing happens in a single transaction; no join-table sprawl.

```csharp
public record class CaseDetailFields
{
    public List<BadgePill> Badges { get; set; } = [];
    public List<string> MetaTags { get; set; } = [];
    public List<MetaItem> Meta { get; set; } = [];          // year, duration, team size...
    public List<ChallengeHurdle> Hurdles { get; set; } = [];
    public List<string> Results { get; set; } = [];
    public List<string> TechPills { get; set; } = [];
    public List<MetricCallout> Metrics { get; set; } = [];
    public EmbeddedTestimonial? Testimonial { get; set; }   // null = no testimonial card
    public TwoColumnText? Conclusion { get; set; }
}

public record BadgePill(string Text, string BgClass, string TextClass);
public record MetaItem(string Label, string Value);
public record ChallengeHurdle(string Heading, string Body);
public record MetricCallout(string Headline, string Description);
public record EmbeddedTestimonial(string Quote, string AuthorName, string AuthorRole, string AvatarPath);
public record TwoColumnText(string Left, string Right);  // both fields HTML (sanitized)

public record class ProductDetailFields
{
    public List<BadgePill> Badges { get; set; } = [];
    public List<string> Features { get; set; } = [];
    public List<MetaItem> ChallengeMeta { get; set; } = [];
    public List<ChallengeHurdle> Hurdles { get; set; } = [];
    public List<string> Results { get; set; } = [];
    public List<MetricCallout> Metrics { get; set; } = [];
    public List<string> TechPills { get; set; } = [];
}
```

Blog and Service detail bodies are article-like. We model them as an ordered list of polymorphic blocks:

```csharp
public record class ArticleBody
{
    public string Intro { get; set; } = "";          // sanitized HTML
    public List<ArticleBlock> Blocks { get; set; } = [];
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SubsectionBlock),  "subsection")]
[JsonDerivedType(typeof(StatsRowBlock),    "stats")]
[JsonDerivedType(typeof(GalleryBlock),     "gallery")]
[JsonDerivedType(typeof(BulletListBlock),  "bullets")]
[JsonDerivedType(typeof(QuoteBlock),       "quote")]
[JsonDerivedType(typeof(ImageBlock),       "image")]
[JsonDerivedType(typeof(RichTextBlock),    "richtext")]
public abstract record class ArticleBlock { public required string Id { get; init; } }

public record class SubsectionBlock(string Heading, string Subheading, string Body, List<SubsectionItem> Items) : ArticleBlock;
public record class StatsRowBlock(string Heading, List<StatRowItem> Items) : ArticleBlock;
public record class GalleryBlock(string Heading, string Description, List<GalleryImage> Images, List<string> Tags) : ArticleBlock;
public record class BulletListBlock(string Heading, List<string> Items) : ArticleBlock;
public record class QuoteBlock(string Quote, string Attribution) : ArticleBlock;
public record class ImageBlock(string ImagePath, string Caption, string Alt) : ArticleBlock;
public record class RichTextBlock(string Html) : ArticleBlock;

public record SubsectionItem(string Label, string Body);
public record StatRowItem(string Number, string Label);
public record GalleryImage(string Src, string Alt);
```

All HTML-containing fields (`Intro`, `RichTextBlock.Html`, `SubsectionBlock.Body`, `TwoColumnText.Left/Right`) are sanitized server-side with `Ganss.Xss` before persistence.

### 5.5 Slugs

`ContentEntity` is an abstract base shared by convention; each concrete entity maps to its own table (no table-per-hierarchy discriminator). Slugs are unique among non-deleted rows within a single table. Enforced by:
1. A unique index per table on `Slug`, filtered `WHERE "IsDeleted" = false` (Postgres partial index).
2. `ISlugger.GenerateAsync(title, entityType, currentId?)` checks for collisions and suffixes `-2`, `-3`, … as needed.
3. Admin `SlugField` validates on blur via a server callback and shows inline error on collision.

### 5.6 Audit

`AuditSaveChangesInterceptor` runs in `AppDbContext.SaveChangesAsync`:
- Snapshots the original entity state via `EntityEntry.OriginalValues` / `CurrentValues`.
- Emits a per-entity audit row with `Action` derived from `EntityState` (Added → Create, Modified → Update or Publish if `Status` flipped, Deleted → Delete, etc.).
- `Diff jsonb` is a minimal JSON patch — only properties that changed, with `{ before, after }`.
- Services never write audit rows by hand.

### 5.7 Output cache

Public-page handlers tag rendered output:
- `entity:{type}:{id}` — invalidated when one entity changes.
- `list:{type}` — invalidated when any entity of that type is created/published/unpublished/deleted.

`IPublicPageCache.EvictForAsync<T>(entity)` is called from each `ContentService.SaveAsync` after a successful save. Default TTL: 10 minutes (configurable).

### 5.8 Uploaded files

```csharp
public record class UploadedFile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Path { get; init; }              // relative to wwwroot, e.g. uploads/blog/2026/05/{guid}-original.png
    public required string OriginalName { get; init; }
    public required string ContentType { get; init; }
    public long Bytes { get; init; }
    public string UploadedByUserId { get; init; } = "";    // "" = system / unknown uploader
    public DateTime UploadedAt { get; init; } = DateTime.UtcNow;
    public List<ImageVariant> Variants { get; set; } = []; // [] for SVG; small/medium/large for rasters
    public int RefCount { get; set; }                       // incremented by entity FK usage
}

public record ImageVariant(string Size, string Path, int Width, int Height, long Bytes);
// Size ∈ { "small", "medium", "large", "original" }
```

Reference tracking is enforced at the service layer: every content service that takes an image path increments `RefCount` on save and decrements on remove/replace. Hard delete from the media library is blocked when `RefCount > 0`.

## 6. Admin UX

### 6.1 Shell

`AdminLayout.razor` wraps every `/admin/*` route, enforces `[Authorize(Roles = "Admin")]`, and lays out:

```
┌────────────────────────────────────────────────────────────────────┐
│  RSD Admin · {user.email}        [View site ↗]     [Sign out]      │  top bar
├──────────┬─────────────────────────────────────────────────────────┤
│ Content  │                                                         │
│  Blog    │                                                         │
│  Cases   │            (page-specific content)                      │
│  Products│                                                         │
│  Services│                                                         │
│  Team    │                                                         │
│  Partners│                                                         │
│  Values  │                                                         │
│  Stats   │                                                         │
│  Tech    │                                                         │
│  Contact │                                                         │
│ ─────    │                                                         │
│ Inbox    │                                                         │
│ Media    │                                                         │
│ Audit    │                                                         │
│ Trash    │                                                         │
│ Users    │                                                         │
└──────────┴─────────────────────────────────────────────────────────┘
```

### 6.2 Per-entity pages

Two pages per entity, parameterized over the entity type:

- **List page** (`/admin/blog`, `/admin/cases`, …): status filter chips (All / Draft / Published / Archived), search box, data table (thumbnail, title, status, updated-at, row actions Edit · Preview · Publish/Unpublish · Delete), bulk select, `+ New {entity}` button.
- **Edit page** (`/admin/{entity}/{id}` or `/new`): two-column layout — entity form on the left, sticky right rail with Status / Slug (`SlugField` with derive-from-title lock toggle) / SEO meta panel / audit info / action buttons (Save draft · Save & publish · Preview ↗).

### 6.3 Body editors

- **Case / Product detail edit (typed)** — a series of clearly-labeled `<fieldset>` groups, each driven by `RepeaterField<TRow>` over its row record: Badges, Meta, Hurdles, Results, TechPills, Metrics, EmbeddedTestimonial, Conclusion. No "pick a block" UX — the form mirrors the schema.
- **Blog / Service detail edit (block list)** — an `Intro` `RichTextEditor`, then a vertical list of block cards rendered by `BlockListEditor`. Each card shows the block-type label + compact preview; drag handle for reorder; `+ Add block` opens a palette: Subsection / StatsRow / Gallery / BulletList / Quote / Image / RichText. Clicking expands inline to the block's sub-editor.

### 6.4 Shared admin components

| Component | Used by |
|---|---|
| `AdminDataTable<T>` | every list page |
| `StatusBadge` | list rows, edit-page status chip |
| `SlugField` | every edit page |
| `ImageUploader` | every image field (cover, avatar, logo, icon) |
| `ResponsiveImage` | public components AND admin previews |
| `RichTextEditor` | Quill JS interop, returns sanitized HTML |
| `BlockListEditor` | Blog/Service body editor |
| `RepeaterField<TRow>` | typed lists inside Case/Product |
| `SeoMetaPanel` | edit-page right rail |
| `ConfirmDialog` | destructive actions |
| `Toast` / `ToastHost` | success/error feedback |

### 6.5 Special admin pages

- `/admin/login` — email/password (Identity), forgot-password.
- `/admin/inbox` — `ContactSubmissions` list, mark-as-handled, delete, `mailto:` reply.
- `/admin/media` — paginated grid of `UploadedFiles`, search, "Used by" reference list, delete blocked when `RefCount > 0`.
- `/admin/audit` — filterable audit log; row expands to show JSON diff.
- `/admin/trash` — soft-deleted entities across all types; restore or hard delete.
- `/admin/users` — list admin users, invite (email link to set password), disable, password reset.

### 6.6 Preview URLs

Draft entities link to `/preview/{type}/{slug}?token=…`. The preview route renders the public detail page bypassing the `Status == Published` filter when the token validates. Tokens are HMAC-signed (`IPreviewTokenSigner`) with a 1-hour TTL; rotating `Preview:SigningKey` in config invalidates outstanding previews.

## 7. Image pipeline

On upload, raster images pass through `ImageSharpProcessor`:

| Variant | Max width | Used for |
|---|---|---|
| `small` | 480 px | Avatars, thumbnails, mobile cards |
| `medium` | 1024 px | Desktop cards, list-item images |
| `large` | 1920 px | Hero / cover full-bleed, OG image |
| `original` | — | Preserved for reprocessing if we change sizes later |

Width-only resize; height auto; no upscaling. Output is WebP at quality 82. Stored at `wwwroot/uploads/{entity}/{yyyy}/{mm}/{guid}-{size}.webp` and `…-original.{ext}`. Each variant's actual dimensions + byte size recorded in `UploadedFiles.Variants`.

SVGs are sanitized (`Ganss.Xss` SVG profile — strip scripts, foreign objects, external refs) and stored unchanged; `Variants` stays empty.

The public `ResponsiveImage` component takes `(UploadedFile file, ImageRole role)`:

| `role` | Variant emitted |
|---|---|
| `Avatar` | `small` |
| `Card` | `medium` |
| `Hero` | `large` |
| `Og` | `large` |
| `Icon` | original (SVG passthrough) |
| `Responsive` | `<picture>` with `srcset` — `medium` for narrow viewports, `large` ≥ md |

All `<img>` tags currently using hard-coded paths in section components switch to `ResponsiveImage` during the phase that wires that entity to the DB — i.e. Phase 2 for the simple entities, Phase 3 for the list rows of Blog/Cases/Products/Services, and Phase 4 for the detail-page body content. Phase 5 mops up any remaining hard-coded `<img>` tags for non-entity assets (brand marks, decorative illustrations, etc.).

## 8. Auth, email, configuration

### 8.1 ASP.NET Identity

- `class AdminUser : IdentityUser` adds `DisplayName`. (Plain `class`, not `record class`, because C# CS8864 disallows records inheriting from non-record base types like `IdentityUser`. Covered by the CLAUDE.md §3 "complex inheritance" exception.)
- EF Core store with cookie auth: `HttpOnly`, `Secure`, `SameSite=Lax`, sliding expiration 30 days.
- Default Microsoft password policy (no overrides): min 6, requires digit + lowercase + uppercase + non-alphanumeric.
- Lockout: 5 failures → 15 min lockout.
- Single role: `Admin`. `[Authorize(Roles = "Admin")]` on `AdminLayout`.
- Antiforgery already wired (`app.UseAntiforgery()`); Blazor Server adds its own checks per interactive call.

### 8.2 First-admin bootstrap

`AdminBootstrapper` runs on app start, after migrations:
1. If `AspNetUsers` has any rows → no-op.
2. Otherwise reads `RSD_BOOTSTRAP_ADMIN_EMAIL` + `RSD_BOOTSTRAP_ADMIN_PASSWORD` from env, creates one admin, throws if env vars are missing.

### 8.3 Email

`IEmailSender` consumed by: forgot-password, user-invite, contact-form notification. Production binding: `SmtpEmailSender` reading `Email__Smtp__Host` / `Port` / `User` / `Password` / `From` from config + env. Development binding: `LoggingEmailSender` writes the message to the log + an audit row; no SMTP needed locally. We pick a concrete provider (SendGrid / SES / self-hosted / etc.) at deploy time — none of the code knows which.

Default `From` and contact-form `To`: `mark.podlyashetskyi@remsoft.dev`; overridable per environment.

### 8.4 Configuration shape

```jsonc
// appsettings.json
{
  "ConnectionStrings": {
    "Postgres": "Host=postgres;Database=rsd;Username=rsd;Password=__set_via_env__"
  },
  "Email": {
    "From": "mark.podlyashetskyi@remsoft.dev",
    "ContactTo": "mark.podlyashetskyi@remsoft.dev",
    "Smtp": { "Host": "", "Port": 587, "User": "", "Password": "", "EnableSsl": true }
  },
  "Uploads": {
    "MaxBytes": 8388608,
    "AllowedContentTypes": [ "image/png", "image/jpeg", "image/webp", "image/svg+xml" ]
  },
  "Imaging": {
    "WebPQuality": 82,
    "Variants": { "Small": 480, "Medium": 1024, "Large": 1920 }
  },
  "Preview": {
    "SigningKey": "__set_via_env__",
    "TtlMinutes": 60
  },
  "OutputCache": { "DefaultTtlSeconds": 600 }
}
```

Sensitive values (`Postgres password`, `Preview:SigningKey`, `Email:Smtp:Password`, `RSD_BOOTSTRAP_ADMIN_*`) come from environment variables in production; never committed.

## 9. Phasing

Each phase is one PR that leaves the site working end-to-end. Phase 4 may be split if it grows unwieldy.

### Phase 1 — Foundation (no public-facing change)

- Postgres in `docker-compose.yml` with named volume + healthcheck; `uploads` volume on `web`.
- EF Core + Npgsql + ASP.NET Identity packages.
- `AppDbContext`, base `ContentEntity`, `SeoMetadata` owned type, Identity tables, `UploadedFiles`, `AuditLogEntries`, `ContactSubmissions`.
- Initial migration; migrate-on-startup hook in `Program.cs`.
- Services scaffolding: `IFileStorage` (`LocalDiskFileStorage`), `IImageProcessor` (`ImageSharpProcessor`), `ISlugger`, `IAuditLog`, `IEmailSender` (`SmtpEmailSender` + `LoggingEmailSender`), `IPublicPageCache`, `IPreviewTokenSigner`.
- `AuditSaveChangesInterceptor` wired into `AppDbContext`.
- `/admin` route group, `AdminLayout`, `/admin/login`, forgot-password.
- `AdminBootstrapper` for first admin.
- Empty admin shell — sidebar with placeholder nav, top bar, sign out. Each nav target is a "Coming soon" stub page.
- Shared admin components: `AdminDataTable<T>`, `StatusBadge`, `SlugField`, `ImageUploader` (functional, hits `IFileStorage` + `IImageProcessor`), `ConfirmDialog`, `Toast`.
- `RSD.Web.Tests` xUnit project added. Initial coverage: slug generation, audit-diff serialization, HTML sanitizer behavior, image-pipeline path generation, status transitions, seed-runner idempotency, `AdminBootstrapper`.

**End-of-phase demo:** log in to `/admin/login`, see the shell, log out. Public site unchanged.

### Phase 2 — Simple entities + Inbox

- Entities: `Testimonials`, `TeamMembers`, `Partners`, `Values`, `MissionStats`, `TechStackItems`, `ContactPoints`, `MessengerLinks`, `SocialLinks`.
- For each: list + new/edit pages, soft delete. These default `Status = Published`; the status workflow is still wired so the column is consistent across the system.
- Idempotent seeders populating today's hard-coded values.
- Wire the public site to read these from DB — replace the hard-coded arrays in `TestimonialsSection.razor.cs`, `TeamSection.razor.cs`, `ManagementSection.razor.cs`, `PartnersSection.razor.cs`, `ValuesSection.razor.cs`, `MissionSection.razor.cs`, `TechStackSection.razor.cs`, `ContactSection.razor.cs`.
- `/admin/inbox` viewer + handled flag.
- Public Contact form posts to `POST /api/contact` (honeypot + rate-limit) which inserts a `ContactSubmissions` row and triggers SMTP notification.

**End-of-phase demo:** edit a testimonial → it changes on the home page. Submit the contact form → it appears in `/admin/inbox` and the configured email gets a message.

### Phase 3 — List rows for Blog / Cases / Products / Services

- Entities: `BlogPosts`, `Cases`, `Products`, `Services` — header-level fields only (title, slug, description, cover image, tags, status, SEO, body fields seeded but not yet editable).
- List + edit pages with `SeoMetaPanel`, `SlugField`, `ImageUploader`.
- Public list pages (`/blog`, `/cases`, `/products`, `/services`) read from DB, filter `Status == Published`, order appropriately.
- Detail pages still render — header fields from DB, body fields from the seeded jsonb. Unknown slugs 404.
- Slug uniqueness validation per type.

**End-of-phase demo:** create a new blog post in admin (header only) → it appears on `/blog`; its detail page renders with the seeded default body.

### Phase 4 — Rich detail bodies

- Case detail and Product detail body editors — typed `RepeaterField` forms over `CaseDetailFields` / `ProductDetailFields`.
- Blog detail and Service detail body editors — `BlockListEditor` over `ArticleBody`.
- Quill `RichTextEditor` component (JS interop), HTML output sanitized server-side with `Ganss.Xss`.
- Preview routes `/preview/{type}/{slug}?token=…` with HMAC-signed tokens.

**End-of-phase demo:** create a full blog post from scratch including a rich body with every block type; preview it as draft; publish; the live page matches the visual quality of today's seeded example.

If this phase feels large, split into 4a (Cases + Products typed bodies) and 4b (Blog + Service block bodies + preview + Quill).

### Phase 5 — Operations polish

- `/admin/audit` viewer + filters.
- `/admin/trash` viewer + restore / hard delete.
- `/admin/media` library with reference tracking.
- `/admin/users` — invite, disable, password reset.
- Output cache fully wired with tag invalidation.
- `/sitemap.xml` and `/robots.txt` endpoints derived from published entities.
- Final pass to ensure every `<img>` in the public Razor tree (including non-entity assets like brand marks and decorative illustrations) uses `ResponsiveImage`.

**End-of-phase demo:** invite a teammate; they log in, edit content, and see their action in the audit log; image uploads come back as optimized WebP variants; sitemap reflects everything published.

## 10. Conventions checklist (per CLAUDE.md)

Every PR in every phase honors:

- **.NET 9 syntax**: primary constructors for DI, `await cts.CancelAsync()`, collection expressions `[]`, `required` keyword, `record` / `record class` for all DTOs and entities, `PeriodicTimer` where applicable.
- **Razor split**: every `.razor` is markup only; logic in `.razor.cs`; `#pragma warning disable S1144, S4487, S2933` at the top of code-behinds; no `[Inject]` properties — primary-constructor DI on partials.
- **No null in business logic**: services return `[]` / empty objects, not null. `Result<T>` (`record Result<T>(bool Ok, T? Value, string Error)`) for fallible operations where empty is ambiguous.
- **No primitive obsession**: typed value objects where a `string` / `int` represents a domain concept — `Slug`, `EmailAddress`, `ImagePath`.
- **Pure functions**: `private static` for any helper that doesn't touch instance state.
- **CC ≤ 4**: validated via analyzer or code review on each PR.
- **Semantic HTML + WAI-ARIA**: admin views as well as public.

## 11. Testing strategy

- **Unit (xUnit)**: slug generation + collision suffixing; HTML sanitizer behavior; image-pipeline path generation + variant selection; audit-diff serialization; SEO defaulting; content-status transitions; preview-token sign + verify; seed-runner idempotency; `AdminBootstrapper` decision logic.
- **Integration (xUnit + Testcontainers Postgres)**: full CRUD per entity via service layer; auth flows (login success/lockout, forgot-password); contact-form submission end-to-end; output-cache invalidation on save.
- **Bunit**: small targeted component tests for `BlockListEditor`, `RepeaterField`, `SlugField`, `SeoMetaPanel` interaction logic.
- **Out of v1 scope**: Playwright end-to-end tests; load tests.

## 12. Open future-work items (explicit out-of-scope for v1)

| Item | Why deferred | Cost to add later |
|---|---|---|
| `SlugRedirects` for renamed slugs | Rarely-needed for an early-stage site; admin can re-link manually | Small — new table + lookup in the slug-resolver middleware |
| Localization (`ITranslatable<>` fields, locale routing) | Brand isn't multi-language yet; modeling now is speculative | Significant — touches every translatable field, but well-understood pattern |
| Scheduled publishing (`PublishAt` + background job) | Workflow can be handled by saving-as-draft until ready | Small — column + Hangfire/QuartzNET worker |
| Per-entity granular permissions | Single-team marketing site doesn't need it | Medium — role checks at service layer |
| SSO / external IdP | No org-wide SSO requirement yet | Medium — ASP.NET Identity supports OIDC out of the box |
| Headless API for third-party consumers | No consumers yet | Medium — services already exist; only need controller layer |
| Horizontal scaling (distributed output cache) | One container is fine for v1 | Medium — swap `MemoryOutputCacheStore` for Redis |
| Automated DB / uploads backups | Operational concern, documented manual recipe instead | Variable, depends on hosting |
