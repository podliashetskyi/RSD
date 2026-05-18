# RSD — Tasks and Bugs

A running list of forward-looking tasks and issues found during manual testing. To be triaged and tackled in later passes.

> **Current status note (2026-05-18):** For the active UX/admin stabilization branch, use [`codex-ux-admin-fix-plan.md`](codex-ux-admin-fix-plan.md) as the deploy-gate source of truth. This file remains the broader historical backlog and may describe earlier behavior that has since changed.

Status legend: **open** = not started · **in-progress** = being worked on · **done** = resolved (leave entry for history) · **deferred** = on hold, may revisit later · **wontfix** = decided not to address.

---

# Tasks

## TASK-001 — Upgrade to .NET 10

- **Status:** open
- **Reported:** 2026-05-14
- **Area:** Solution-wide (`RSD.Web`, all class libraries, build/CI)

**Summary**
Upgrade the solution from .NET 9 to .NET 10. Coordinate across all projects in the solution, update Docker/CI images, and adopt any new C# language features that simplify existing code.

**Notes / Scope**
- Bump `<TargetFramework>` in every `.csproj`.
- Update NuGet packages to .NET 10–compatible versions (Blazor, EF Core, etc.).
- Update SDK version in `global.json` if pinned.
- Update CI build images and Dockerfile base images.
- Sweep for any new C# / .NET 10 syntax wins (per [CLAUDE.md §1](../CLAUDE.md) the rule is "always newest available syntax").
- Run full build + test suite; smoke-test public site + admin.

---

## TASK-002 — "Start a Project" flow (public site + backend + admin)

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-14 (Phase 7 — commits `2b10aee`, `81f9f83`, `b030509`; merge `1756274`)
- **Area:** Public site (FE), backend API, admin panel

**Summary**
Build an end-to-end "Start a project" flow so prospective clients can kick off an engagement from the public site. Submissions are stored, the team gets notified, and admins can triage incoming requests in the admin panel.

**Three deliverables**

1. **Frontend (public site)**
   - "Start a project" CTA + multi-step form (project type, scope, budget range, timeline, contact info, optional attachments).
   - Client-side validation, accessible (ARIA, keyboard), responsive (desktop + mobile in one pass).
   - Thank-you / confirmation state after submit.

2. **Backend**
   - Endpoint(s) to accept and persist submissions.
   - Validation, anti-abuse (rate limit / honeypot / captcha).
   - Notification (email to sales/internal channel) on new submission.
   - Domain model: `ProjectRequest` (record), `ProjectRequestStatus` enum, etc. — follow [CLAUDE.md §3–§4](../CLAUDE.md) (records for DTOs, no primitive obsession).

3. **Admin panel**
   - List view of incoming project requests with filters (status, date, type).
   - Detail view with full submission + audit trail.
   - Status transitions (New → Reviewing → Contacted → Won/Lost) with notes.

**Notes / Scope**
- Needs a brainstorming pass before implementation — fields, statuses, notification channel, attachment storage all need decisions.
- Hook into existing admin auth & layout — don't fork a new shell.
- Consider GDPR / data retention policy from day one (project requests contain PII).

**What landed (Phase 7)**
- Public 4-step wizard at `/estimate` + success view (`Estimate.razor`, `EstimatorHero`, `StepIndicator`, `OptionCard`). Footer "Estimate Project Cost" CTA repointed from `/contact` to `/estimate`.
- `ProjectEstimate` record entity + EF config + hand-written migration `20260516140000_ProjectEstimates`. `IProjectEstimateService` mirrors `ContactSubmissionService`; fire-and-forget admin email via `ProjectEstimateTemplate` (includes summary chip and computed range).
- `EstimatePricing` (base × multipliers, rounded to $500, max = min × 1.5) and `EstimatorCatalog` (single source of truth for enum→label mapping, reused by service, email, public wizard, admin detail).
- `/api/estimate` Minimal API with its own rate-limit policy (5 reqs / 5 min / IP) and honeypot. Global `JsonStringEnumConverter` so enum names serialize as strings. **In-page wizard bypasses the endpoint** and calls the service directly to avoid the InteractiveServer self-loopback issue (Kestrel binds `:8080` inside the container; public URL uses host-mapped `:8082` which doesn't route back). Endpoint kept for direct API/automation callers.
- `/admin/estimates` list (Open / Handled / All filters, search across name/email/company/description, paginated) + slide-over detail (preliminary range, four selections with labels, project description, Reply-by-email mailto, Mark handled / Reopen / Delete). "Estimates" added to admin Operations nav.
- Audit log is auto-wired via `AuditSaveChangesInterceptor` — admin mutations on `ProjectEstimate` show up in `/admin/audit` without per-entity hookup.

**Known gap vs original spec (deferred)**
- Status transitions are currently **binary** (`IsHandled` true/false, mirroring the Contact Inbox) rather than the spec'd lifecycle (New → Reviewing → Contacted → Won/Lost) with per-status notes. Internal triage CRM features can be a follow-up if needed.
- No attachments. The wizard doesn't include a file upload field. Add if customers actually need to share specs at request time.
- No captcha — rate limit + honeypot only. Sufficient for the current threat model.

---

## TASK-003 — Add "3D Modeling" and "3D Printing" as adjacent services

- **Status:** deferred
- **Reported:** 2026-05-14
- **Deferred:** 2026-05-14 — on hold, will revisit when the adjacent-services strategy is decided
- **Area:** Public site (Services), content model, admin

**Summary**
Add two new services — **3D Modeling** and **3D Printing** — that are *adjacent* to RemSoft.Dev's core software development offering rather than part of it. They should be presented clearly as separate / partner offerings so visitors don't conflate them with the core RSD brand.

**Notes / Scope**
- Decide presentation: dedicated pages (e.g. `/services/3d-modeling`, `/services/3d-printing`) with visually distinct treatment from core software services, OR a separate "Adjacent Services" block on the Services index.
- Content model: reuse the existing `Service` entity if possible, plus a flag/category (e.g. `ServiceCategory.Adjacent` vs `ServiceCategory.Core`) so they can be filtered/styled differently.
- Copy is dummy until the user provides real text — only `RemSoft.Dev` / `RSD` / `Remote Software Development` are real brand strings.
- Admin: editors should be able to create/edit these like any other service, but the "category" toggle must be visible.

---

## TASK-004 — Audit: inventory every image field in the admin + every missing `alt` on the public site

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-14 (Phase 8 / T30, branch `feature/phase8-image-ux`)
- **Area:** Solution-wide research (admin editors + public site rendering)
- **Blocks:** [TASK-005](#task-005--replace-image-path-text-inputs-with-a-real-image-upload-control-across-the-admin)

**Summary**
Before rebuilding the image-input UX, do a read-only audit so we know the full surface area. Two outputs:

1. **Admin image-field inventory** — every form/editor that references an image, what kind of image, where its file currently lives, whether the field is a plain "path" text input or already something better, and whether an alt-text field exists alongside.
2. **Public-site alt-text gap list** — every place an `<img>` (or `background-image` used as content) is rendered on the public site without a meaningful `alt` attribute.

**Deliverable**
A new file `Research/image-audit.md` (or a section appended to this file) containing two tables:

- **Admin fields** — columns: editor (file path), field name, expected image type (avatar / hero / OG / icon / cover / screenshot / logo / …), current control (path text input / file upload / other), current storage location, alt-text field present? (yes/no), notes.
- **Public-site renderings** — columns: page/component (file path : line), what image is rendered, current `alt` value (or "missing"/"empty"/"decorative"), recommended action (add real alt / mark decorative with `alt=""` / source from CMS).

**Notes / Scope**
- Read-only — no code changes in this task. Output is reference material that scopes TASK-005 and any accessibility-fix follow-ups.
- Use `grep`-style searches for: `*.razor` files with `<img`, references to `images/`, file inputs (`InputFile`), property names containing `Avatar`, `Image`, `Logo`, `Icon`, `Cover`, `Photo`, `Path`.
- Group public-site findings by "decorative vs. content" — decorative images need `alt=""` (still missing, but the fix is different from content images).
- Tag any image references that look like they belong to a layout/template (rendered on many pages) — fixing those has the biggest payoff.

**What landed (T30)**
- New audit doc at [Research/image-audit.md](image-audit.md) with two tables: admin image-field inventory (14 fields across 13 editors + the shared `SeoMetaPanel`) and a public-site `<img>` audit grouped into decorative / already-hidden / content / out-of-scope.
- One mechanical a11y fix bundled in: `Blog/HeroSection.razor:16` now has `aria-hidden="true"` on the decorative search icon (the only stray `<img>` whose ancestor chain wasn't already aria-hidden — most decorative tiles inside `Contact/Cases/Products/Services/About/HeroSection.razor` already sit inside a `<div aria-hidden="true">` ancestor and don't need img-level treatment).

---

## TASK-005 — Replace "image path" text inputs with a real image-upload control across the admin

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-14 (Phase 8 / T31 + T32, branch `feature/phase8-image-ux`)
- **Area:** Admin — every form that references an image (Team members, Pages hero, Services, Blog covers, Cases screenshots, etc.)
- **Blocked by:** [TASK-004](#task-004--audit-inventory-every-image-field-in-the-admin--every-missing-alt-on-the-public-site)

**Summary**
Several admin editors expose image fields as plain text inputs that expect a relative path (e.g. the Team member editor's **"Avatar path"** field with placeholder `images/about/team/avatar-01.png`). Editors are expected to type or paste the path themselves, which is developer-facing, error-prone (typos, missing files, wrong folder), and unusable for non-technical content authors. Replace this pattern with a proper upload control everywhere it appears.

**Repro (current behavior, Team example)**
1. Admin → Team → Edit member.
2. The "Avatar path" field is a free-text input.
3. The user must already know the exact relative path on disk to a pre-uploaded image — there is no browse, no upload, no preview.

**Expected (target behavior)**
A shared image-picker component:
- **Drag-and-drop + click-to-browse** upload from the user's machine.
- **Preview** of the selected/current image inline.
- **Replace / Remove** actions on an existing image.
- **Client-side validation**: file type (jpg/png/webp/svg as appropriate), max file size, max dimensions.
- **Server-side**: receive upload, store under the correct folder (e.g. `wwwroot/images/about/team/`), generate a unique filename (avoid collisions), persist the resulting relative path on the entity.
- **Optional but recommended**: generate resized/optimized variants (e.g. WebP @ 1x/2x) at upload time.
- Accessible (keyboard upload, alt-text field alongside).

**Notes / Scope**
- Cross-cutting — applies to **every** image field in the admin, not just Team. Quick audit needed: Team avatars, Page hero/OG images, Service icons, Blog cover images, Case screenshots, Tag icons (if any), site logo, etc.
- Build as a single shared Razor component (`ImageUploadField` or similar) consumed by every editor, so the UX and validation rules are uniform.
- Decide storage strategy up front: filesystem under `wwwroot/uploads/...` vs. a blob store (Azure Blob / S3). Filesystem is simplest now; blob is cleaner long-term.
- Add an **alt text** field next to every image upload — required for accessibility, currently absent.
- Consider a lightweight in-app media library (list/reuse already-uploaded images) as a follow-up, not blocking.

**What landed (T31 — control swap)**
- Replaced the plain `<InputText>` image-path field with the existing shared `ImageUploader` (drag-and-drop + click-to-browse + preview + remove, server-side WebP variants via `IImageProcessor`, files written to `wwwroot/uploads/{subfolder}/{yyyy}/{MM}/`) in 8 admin editors + 1 shared panel:
  - Team avatars (`team`), Testimonial avatars + the embedded testimonial avatar inside `CaseBodyEditor` (`testimonials`), Value icons (`values`), Partner photos (`partners`), Tech-stack logos (`tech`), Social-link icons (`social-links`), Messenger-link large + small icons (`messenger-links`), `SeoMetaPanel` OG image (`seo` — auto-applies to ~14 editors that consume the panel).
- Each editor switched from static SSR form (`method="post"` + `[SupplyParameterFromForm]`) to `@rendermode InteractiveServer` so the uploader's JS interop works. The path text input is kept underneath the uploader as a fallback for manual entry.
- Fix bundled in: `ImageUploader` previously bound its preview to the `CurrentFile` parameter; since no caller passes that parameter back, the preview never rendered after a successful upload. Switched the preview block to a private `Preview` field updated on upload / cleared on Remove, so the just-uploaded image now shows immediately.

**Current update (UX-004, 2026-05-18)**
- Manual image path inputs are no longer shown as part of the normal editor workflow. They remain bound for existing data and QA inspection, but are hidden behind an "Advanced: edit stored path" disclosure.

**What landed (T32 — alt-text layer)**
- `ImageUploader` accepts optional `Alt` + `AltChanged` parameters; renders an inline alt-text input below the dropzone only when a caller wires `AltChanged`. Backwards-compatible for the 8 editors above that don't pass alt (those still derive alt from a sibling field like `Name` / `Label` / `AuthorName`).
- New schema columns added via `20260516150000_ImageAltText` migration:
  - `CoverImageAlt` on `blog_posts`, `cases`, `products`, `services`.
  - `SeoOgImageAlt` on every ContentEntity table (15 in total) via the owned `SeoMetadata` config in `ContentEntityConfiguration`.
- `BlogPostUpsert` / `CaseUpsert` / `ProductUpsert` / `ServiceUpsert` records carry the new alt; the four services thread it through `NewEntityFrom` / `ApplyUpdate`. The four edit pages wire `Alt`/`AltChanged` to their cover uploader; `SeoMetaPanel` does the same for the OG image.
- Public-side alt rendering wired up:
  - Detail-page hero (`Sections/Detail/HeroSection.razor`) gained an `Alt` parameter; the wrong `aria-hidden="true"` on the content hero image is gone; `BlogDetail` / `CaseDetail` / `ProductDetail` / `ServiceDetail` now pass `HeroAlt` (= entity's `CoverImageAlt` with `Title`/`Name` fallback).
  - `Article/ArticleHeaderSection.razor` — author avatar is `alt="@AuthorName"`.
  - `Blog/PostsGridSection.razor` — list-card author avatar is `Alt="@p.AuthorName"`.
  - `Services/TechStackSection.razor` — tech logo is `Alt="@item.Label"`.

**Deferred (Phase 9+ candidates, captured here so they don't get lost):**
- Re-hydrate the existing image preview when re-opening an editor — currently `ImageUploader.CurrentFile` isn't populated from the entity's path on edit, so a saved image only shows a preview if the admin re-uploads. Needs a lookup of `UploadedFile` by `Path` on load.
- Grid covers (`PostsGridSection` / `CasesGridSection` / `ProductsListSection` / `Services/FeaturesSection`) still fall back to `Title + " cover image"` for the card alt; the detail-page hero already uses the new `CoverImageAlt`. Wire the new column through the listing viewmodels so admins can override the card alt too.
- Lightweight in-app media library (browse / reuse already-uploaded images).
- Migrate storage from filesystem to a blob store (S3 / Azure Blob).
- Audit the admin-only `ResponsiveImage.razor` callers — make the `Alt` parameter required and source it sensibly per call site.

---

## TASK-006 — Admin-managed taxonomies for Case + Blog filter values

- **Status:** done
- **Reported:** 2026-05-15
- **Resolved:** 2026-05-15 (branch `feature/task-006-filters`, commit `d3ee1ce`, merged via `508b36a`)
- **Area:** Admin (new lookup tables + editors), Case + Blog editors, public filter sections
- **Relates to:** [BUG-005](#bug-005--cases-page-filters-do-not-filter-clicking-a-filter-option-navigates-to-a-detail-page) (closed in T36 against free-text data), [BUG-006](#bug-006--public-pages-search--filters-are-non-functional) (T37)

**Summary**
After T36 the public `/cases` filters work, but their options are derived by scanning distinct values across free-text `Industry` and `TechTags` fields on each Case. That makes filter taxonomy fragile (typos create duplicates: "Fintech" vs "Fin-Tech") and gives the admin no clear place to manage the list — they have to set a value on some case before it appears as a filter option. Same shape will apply to Blog's Category and Tags once T37 lands. Replace with controlled admin-managed taxonomies.

**Scope**
- Four new entities (or one `Taxonomy` table with a `Type` discriminator): `CaseIndustry`, `CaseTechTag`, `BlogCategory`, `BlogTag`. Each carries `Id`, `Label`, `Slug`, `DisplayOrder`, soft-delete + audit (mirror existing simple-entity shape).
- Four new admin pages (Operations or Content sidebar group): `/admin/case-industries`, `/admin/case-tech-tags`, `/admin/blog-categories`, `/admin/blog-tags` — simple CRUD lists modelled on `/admin/social-links` or `/admin/values`.
- Case editor: replace the free-text `Industry` `<InputText>` with an `<InputSelect>` sourced from `CaseIndustry`; replace the `TagInput` for `TechTags` with a constrained variant that only allows picking from `CaseTechTag` (likely a multi-select chip picker — design call needed).
- Blog editor: replace `Category` `<InputText>` with `<InputSelect>` from `BlogCategory`; replace the `Tags` `TagInput` with a constrained picker from `BlogTag`.
- Schema decision: keep the Case/Blog columns as denormalized strings (`Industry`, `Category`, `TechTags`, `Tags`) for query simplicity OR migrate to FK / join tables. Recommendation: keep denormalized for now (cheap to query, no join), have the admin pickers write the label string. Migration to FKs can be a later cleanup.
- Public side: `CasesGridSection` and `Blog/PostsGridSection` switch from scanning case/post data for options to loading them from the new taxonomy tables. Filter still matches on the denormalized string column.
- Backfill: seed the taxonomy tables from currently-distinct strings on existing Cases / BlogPosts so nothing disappears from the public filters on deploy.

**Notes / Scope**
- Decide unified `Taxonomy` table vs. four discrete entities up front. Unified is fewer files, but couples the four use-cases. Four entities is more boilerplate but keeps domain edges sharp. Probably four — matches the existing one-entity-per-concern shape of the codebase.
- The constrained tag picker is the real design question — chip picker with autocomplete sourced from the table, no free-text add. Or allow "Add new" that round-trips through a confirmation step so the admin doesn't accidentally proliferate one-off values.
- Don't ship until existing data is backfilled — otherwise published-page filters stop showing pre-existing values until each post is re-saved.
- Out of scope for this task: a generic taxonomy admin UI for *all* future filter dimensions. Just the four listed.

**Decisions locked at brainstorm**

- **Shipped as a unified `Filter` entity with a `Type` discriminator**, not four discrete entities. Drove one sidebar item ("Filters"), one admin page with tabs, fewer files. Adding a new filter dimension later is a one-line enum addition.
- Sidebar name **"Filters"** — avoids ambiguity with the existing `BlogPost.Tags` string column.
- Picker UX: **strict list-only.** No inline "add new" on the Case/Blog editors. New values are added at `/admin/filters`.
- Storage: **kept denormalized string columns** on Case/Blog (Industry, Category, TechTags, Tags). Picker writes the filter's `Label`. No FKs. Renames don't cascade — caveat below.

**What landed**

- **Schema** ([`20260516180000_Filters`](../RSD.Web/Data/Migrations/20260516180000_Filters.cs)): new `filters` table inheriting `ContentEntity`'s column shape (slug + soft-delete + Seo* + audit timestamps) plus `Type` (varchar 40, stored as enum string), `Label` (varchar 200), `DisplayOrder`. Partial-unique index on `Slug WHERE "IsDeleted" = false`; indexes on `Type` and `DisplayOrder`.
- **Backfill** inside the same migration: distinct existing values from `cases.Industry`, `unnest(cases.TechTags)`, `blog_posts.Category`, `unnest(blog_posts.Tags)` seeded with type-prefixed slugs (`caseindustry-fintech`, `blogtag-react`, …). `ON CONFLICT DO NOTHING` guards post-normalization collisions. Confirmed counts on deploy: 4 BlogCategory · 9 BlogTag · 6 CaseIndustry · 16 CaseTechTag.
- **Entity + service:** [`Filter`](../RSD.Web/Data/Entities/Filter.cs) + [`FilterType`](../RSD.Web/Data/Entities/FilterType.cs); [`FilterConfiguration`](../RSD.Web/Data/Configurations/FilterConfiguration.cs) mirrors `ValueConfiguration`. [`IFilterService`](../RSD.Web/Services/Content/IFilterService.cs) inherits `ISimpleContentService<Filter>` and adds `ListByTypeAsync(type, ct)` (filters by Type + Status=Published, ordered by DisplayOrder then Label). [`FilterService`](../RSD.Web/Services/Content/FilterService.cs) uses `$"{Type}-{Label}"` as the slug seed so the base class's collision resolver produces distinct slugs per (type, label) pair.
- **Admin:** [`/admin/filters`](../RSD.Web/Components/Admin/Pages/Filters/FilterList.razor) with four type tabs + per-tab counts + scoped reorder (↑/↓) + edit/delete. `+ New` button passes `?type=<active>` to the create form. [`/admin/filters/new`](../RSD.Web/Components/Admin/Pages/Filters/FilterEdit.razor) + `/admin/filters/{Id}`: Type read-only after create (delete + recreate to move a filter between types). New "Filters" entry in the Content sidebar group.
- **Shared component** [`ConstrainedTagPicker`](../RSD.Web/Components/Admin/Shared/ConstrainedTagPicker.razor): chip-row picker with a passed `Options` list, click-to-toggle, no free-text input. Renders empty-state copy with an optional "Manage" link when `Options` is empty.
- **Editors:** [`CaseEdit`](../RSD.Web/Components/Admin/Pages/Cases/CaseEdit.razor) — Industry `<InputText>` → `<InputSelect>`; TechTags `<TagInput>` → `<ConstrainedTagPicker>`. [`BlogEdit`](../RSD.Web/Components/Admin/Pages/Blog/BlogEdit.razor) — same for Category + Tags. Both load options once in `OnInitializedAsync` via `IFilterService.ListByTypeAsync`.
- **Public sections:** [`CasesGridSection`](../RSD.Web/Components/Sections/Shared/CasesGridSection.razor.cs) and [`PostsGridSection`](../RSD.Web/Components/Sections/Blog/PostsGridSection.razor.cs) now load their dropdown options from `IFilterService` instead of scraping distinct values from the rendered entity list. CasesGridSection only pays for the two filter queries when `ShowFilters` is true (Home's `MaxItems=3` callsite stays cheap). Filter matching logic is unchanged — still string-matches `c.Industry` / `c.TechTags`. Public dropdowns now order by `DisplayOrder` instead of alphabetical.

**Known caveats (documented now, deferred fixes)**

- **Renames don't cascade.** If admin renames `"Fintech"` → `"FinTech"`, existing Case rows with `Industry = "Fintech"` are no longer filterable by the new chip. Workaround for v1: admin re-saves affected rows. Future polish: a "rename + bulk-update existing references" admin action.
- **Soft-deleting a Filter doesn't strip the value from entities.** Same shape — the value persists in the denormalized column; public filter UI hides the chip but the row stays published with its old value attached.
- **Type can't be changed after create.** A `CaseIndustry` can't become a `BlogCategory`. Admin deletes + recreates. Future polish: type-switch with an orphaning warning.

---

# Bugs

## BUG-001 — No max-length on text inputs across the admin

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-15 (Phase 9 / T38, branch `feature/phase9-bug-polish`, commit `e7cf826`)
- **Area:** Admin / all editor forms (Pages, Services, Blog, Cases, Tags, etc.)

**Summary**
There is no standardized maximum character limit on any text field in the admin editors. Users can paste arbitrarily long strings into titles, slugs, descriptions, tags, etc.

**Repro**
1. Open any admin editor (e.g. edit a Page, Service, Blog post, Case).
2. Paste 1000+ characters into any text field.
3. Field accepts it without warning, validation, or truncation.

**Expected**
Each field has a defined, standardized max length appropriate to its role (e.g. title ≤ 120 chars, short description ≤ 240 chars, slug ≤ 80 chars). Either enforced via `maxlength` + server-side validation, with a visible character counter near limit.

**Notes / Scope**
- Define a single source of truth for limits (e.g. `FieldLimits` constants in domain layer) so FE + BE stay aligned.
- Apply consistently across every admin form, not per-page.

**What landed (T38)**
- New [`Data/FieldLimits.cs`](../RSD.Web/Data/FieldLimits.cs) static class nested by entity. Every column length referenced by an admin input lives here as a const. EF `*Configuration.cs` calls, input-model `[StringLength]`, and `maxlength="..."` on every `<InputText>` / `<InputTextArea>` all read from the same constants — change one place, FE+BE+DB stay aligned.
- New shared [`Components/Admin/Shared/FieldField.razor`](../RSD.Web/Components/Admin/Shared/FieldField.razor) wrapper renders a live `n / max` counter under any wrapped input. Counter goes amber at 90% of the limit, red at the cap. Applied to long-form text areas (Description on the 4 cover entities, Quote on Testimonial, Description on Value, Summary fields from T39).
- `SeoMetaPanel.razor` swaps its hardcoded `maxlength="200"`/`"500"` to the new constants.

---

## BUG-002 — List/index pages render the full description, not a truncated summary

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-15 (Phase 9 / T39, branch `feature/phase9-bug-polish`, commit `c9aaeed`)
- **Area:** Public list pages (Services, Blog, Cases, etc.) — card / list-item rendering

**Summary**
On listing pages (Services, Blog, etc.), each item shows its full body/description as entered in the editor. A 500–1000 character description floods the card layout.

**Repro**
1. Edit a Service or Blog post and set a long description (~500–1000 chars).
2. Save and navigate to the public list page (e.g. `/services`, `/blog`).
3. The card shows the entire description instead of a short summary.

**Expected**
List items show a truncated summary (e.g. first ~160–200 chars with ellipsis, or a dedicated `Summary`/`Excerpt` field separate from the body). Full content shows only on the detail page.

**Notes / Scope**
- Decide: truncate the body server-side at render time, OR add a separate `Summary` / `Excerpt` field on the model.
- Recommend a dedicated `Summary` field — gives editors control over the card copy and avoids mid-sentence cuts.
- Relates to [BUG-001](#bug-001--no-max-length-on-text-inputs-across-the-admin) (the Summary field would have its own char limit).

**What landed (T39)**
- New hand-written migration [`20260516160000_ListSummary`](../RSD.Web/Data/Migrations/20260516160000_ListSummary.cs) adds `Summary varchar(280) NOT NULL DEFAULT ''` to `blog_posts`, `cases`, `products`, `services`. Designer + `AppDbContextModelSnapshot` updated.
- `Summary` property added to the 4 entities. Length governed by `FieldLimits.X.Summary` (280) in each `*Configuration.cs`.
- Threaded through `BlogPostUpsert` / `CaseUpsert` / `ProductUpsert` / `ServiceUpsert` records and through `NewEntityFrom` + `ApplyUpdate` in the four `*Service.cs`.
- Each of the 4 cover-entity editors gained a Summary `<InputTextArea rows="2">` wrapped in the T38 `FieldField` counter, placed right before the existing Description. Hint copy: "Short blurb shown on listing cards. Falls back to Description if blank."
- Public list cards render Summary with Description fallback when blank: `PostsGridSection` (via a `CardBlurb` field on the row viewmodel — search also matches `CardBlurb` instead of raw Description), `CasesGridSection`, `ProductsListSection`, `Services/FeaturesSection`. Detail pages continue rendering the full `Description`.

---

## BUG-003 — Slug unlock/edit/save is broken on Page editor (and all editors with a slug)

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-15 (Phase 9 / T35, branch `feature/phase9-bug-polish`, commit `5e41822`)
- **Area:** Admin / Page editor + every editor that has a slug field (Services, Blog, Cases, etc.)

**Summary**
The slug field uses a lock/unlock toggle. The current behavior is inconsistent:

1. First edit session: clicking **unlock** does not actually enable editing of the slug input — the field stays read-only.
2. After saving the page and reopening it for edit: the slug *can* be edited, **but** clicking **Save** afterwards does not persist (save appears to fail / no update).

**Repro**
1. Create or open a Page (or Service / Blog post / Case).
2. Click the lock icon next to the slug → unlocked state.
3. Try to type in the slug input → no edit allowed.
4. Save the page, reopen it for editing.
5. Click unlock again → now the slug input accepts edits.
6. Change the slug and click Save → save does not work (no persistence, possibly silent failure or error).

**Expected**
- Unlocking the slug enables editing the field immediately, on first edit, the same way it does on subsequent edits.
- Saving a page with an edited (unlocked) slug persists the new slug and surfaces any validation error (e.g. duplicate slug) clearly.

**Root cause (what landed in T35)**
Two layered bugs:

1. Razor type-inference on string-typed component parameters: `Value="Input.Slug"` was being parsed as the **literal string** `"Input.Slug"`, not the C# expression. All 4 SlugField call sites (Blog / Cases / Products / Services) were affected. Fix: prefix with `@` — `Value="@Input.Slug" TitleSource="@Input.Title" CurrentEntityId="@Id"`. (List/bool/int-typed params worked because Razor can rule out the literal-string interpretation by type — explains why `<TagInput Value="Input.Tags">` was fine.)
2. `SlugField.OnParametersSetAsync` unconditionally re-derived `Slugify(TitleSource)` and propagated it back to the parent every render whenever `Locked=true` (the default), silently clobbering any custom slug that differed from the title-derived form. Added an `Initialized` flag so the first render with a non-empty `Value` preserves the loaded slug verbatim and auto-unlocks. Also hardened `disabled="@Locked"` → `disabled="@(Locked ? "disabled" : null)"` to dodge the bool-attr edge case on plain `<input>`.

**Data note:** existing rows likely have stale `"input-title[-N]"` slugs that were generated under the broken binding (the literal `"Input.Title"` slugified to `"input-title"` and that's what got saved). Re-save affected posts after deploy to clean them up.

---

## BUG-004 — Pressing Enter in the tag input saves & exits the editor

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-15 (Phase 9 / T34, branch `feature/phase9-bug-polish`, commit `e172a32`)
- **Area:** Admin / Page editor (and any editor using the tag picker)

**Summary**
While editing a Page (or any entity with the tag picker), typing a new tag name and pressing **Enter** triggers a full form submit. The page is saved and the user is redirected back to the list view, mid-edit.

**Repro**
1. Open a Page (or Service / Blog post / Case) for editing.
2. Make some changes (do not save).
3. Click into the "tag" input.
4. Type a new tag name → press **Enter**.
5. The entire form submits. User is redirected to the list view immediately.

**Expected**
Pressing Enter inside the tag input **adds the tag locally** (commits it into the tag chips on the form) and **does not** submit the parent form. The user remains on the editor and can keep editing.

**Notes / Scope**
- Cause: the tag input is inside the `<form>` and there is no `preventDefault` on its Enter handler — browser default `<form>` submit fires.
- Fix in the shared TagInput component: handle `keydown.enter` with `preventDefault` and commit the tag locally.
- Also confirm the same fix applies to other inline inputs (e.g. "add link" / "add image alt" / etc.) that might submit the form on Enter.

**What landed (T34)**
- Blazor's `@onkeydown:preventDefault` is evaluated at render time, not event time, so it can't be conditional on the key value (an unconditional `true` would block character input). Solved via a tiny JS-interop helper: new [`wwwroot/js/admin/tag-input.js`](../RSD.Web/wwwroot/js/admin/tag-input.js) attaches a `keydown` listener that calls `e.preventDefault()` only when the key is Enter or `,`. [`TagInput.razor.cs`](../RSD.Web/Components/Admin/Shared/TagInput.razor.cs) now imports the module via `IJSRuntime` in `OnAfterRenderAsync(firstRender)` and detaches on `DisposeAsync`. Existing Blazor `OnKeyDownAsync` handler still commits the tag chip — JS just swallows the browser default submit.

---

## BUG-005 — Cases page filters do not filter; clicking a filter option navigates to a detail page

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-15 (Phase 9 / T36, branch `feature/phase9-bug-polish`, commit `d26a043`)
- **Area:** Public `/cases` page — filter UI (Industry, Tech Stack, Project Type, Year)

**Summary**
On the public Cases page, the filter dropdowns (Industry, Tech Stack, Project Type, Year) do not filter at all. Worse, clicking on an option inside a dropdown navigates the user to a (case detail?) page instead of applying a filter.

**Repro**
1. Visit `/cases` (public site).
2. Click the **Industry** dropdown → list of industries appears.
3. Click an industry option.
4. Browser navigates to a different page (looks like a case detail or some unrelated route) instead of filtering the case list.

**Expected**
Clicking a filter option:
- Applies the filter (Cases list narrows to matching items).
- Stays on the `/cases` page (URL may reflect filter state, e.g. `?industry=fintech`).
- Multiple filters combine (AND across filter dimensions).

**Notes / Scope**
- Likely each option is wrapped in an `<a href="...">` to the case detail route by mistake (template reuse from case cards).
- Confirm whether filter state should live in the URL (recommended — shareable links, browser back works).

**What landed (T36)**
- Filter options were rendered as `<a href="#">` so clicking navigated instead of filtering, and `CasesGridSection` had no filter state at all. Replaced the Flowbite `data-dropdown-toggle` chrome + anchor options with Blazor-controlled `<button>`s, added `Industry` / `TechStack` / `Year` nullable filter state, an `OpenFilter` enum for which dropdown is open, AND-semantics in `DisplayedCases`, and a "Clear all" affordance.
- Options derived dynamically from the loaded published cases (distinct `Industry` strings, flattened `TechTags`, years from `PublishedAt ?? CreatedAt`).
- `InteractiveServer` set at the `/cases` call site only — Home's `<CasesGridSection MaxItems="3" />` stays static SSR.
- Deviation from original plan: the "Project Type" dropdown was dropped — `Case` has no such field, so it would've been a non-functional control. Easy to add later if the column is introduced.
- URL state (e.g. `?industry=fintech`) deferred — captured under Phase 10+ candidates.
- Follow-up captured separately: [TASK-006](#task-006--admin-managed-taxonomies-for-case--blog-filter-values) — replace free-form `Industry` / `TechTag` inputs in the Case editor with admin-managed taxonomies so filter values stop being scraped from free-text.

---

## BUG-006 — Public Pages: search + filters are non-functional

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-15 (Phase 9 / T37, branch `feature/phase9-bug-polish`, commit `980db5b`)
- **Area:** Public `/blog` — search box + category filter chips

**Summary**
On the public Pages listing, the search input does nothing and the filter controls do nothing. Typing in search returns no change in results; clicking a filter has no visible effect — no separate "filtered" block, no narrowing of the list.

**Repro**
1. Visit the public Pages listing.
2. Type a query into the search box.
3. No filtering of results occurs (full unfiltered list remains).
4. Click any filter control.
5. No filtering happens. No filtered results block appears.

**Expected**
- Search: typing a query filters the visible list in real-time (or on submit) by title / summary / tags.
- Filters: clicking a filter narrows the list and stays on the same page; combine across dimensions.
- Filter state should be reflected in the URL.

**Notes / Scope**
- Likely the search/filter inputs are not wired to any query handler at all (placeholder UI).
- Define whether filtering is client-side (already-loaded list) or server-side (re-query on change). Server-side is preferred once datasets grow.
- Same UX patterns should apply to [BUG-005](#bug-005--cases-page-filters-do-not-filter-clicking-a-filter-option-navigates-to-a-detail-page) — recommend a shared filter component.

**What landed (T37)**
- Search input and chip row originally lived in `HeroSection` with no event bindings and no link to `PostsGridSection` — they were inert placeholder UI. Consolidated them into `PostsGridSection` where the post list is rendered (`HeroSection` stripped back to just the heading + intro; its hardcoded placeholder `FilterChips` removed).
- `PostsGridSection`: search input bound live via `@bind:event="oninput"` (case-insensitive Contains over Title + Summary/Description); category chips driven dynamically by the distinct `Category` column on loaded posts, with "All" pinned first; AND-semantics across search and category in `DisplayedPosts`; `aria-pressed` on the active chip; empty state when filter yields zero results.
- Dead `<Button Href="#">View more</Button>` removed — pagination never worked; clean placeholder out rather than ship broken UI.
- `Blog.razor`: `@rendermode="InteractiveServer"` on `PostsGridSection` only (`HeroSection` stays static).
- Same free-text caveat as T36: chips are scraped from each post's `Category` field, so typos surface as duplicate chips. Captured under [TASK-006](#task-006--admin-managed-taxonomies-for-case--blog-filter-values).

---

## BUG-007 — Hero heading shows a focus outline after every client-side navigation

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-15 (Phase 9 / T33, branch `feature/phase9-bug-polish`, commit `e941477`)
- **Area:** Public site — shared hero / page header component, applies on every page that has a hero section

**Summary**
After navigating from one page to another on the public site, the hero section's main heading renders with a visible blue focus outline drawn around it (and the "Our Services"–style pill that precedes it). The outline appears on every page transition and only disappears when the user clicks elsewhere on the page. It is not a designed/intentional treatment — it is the browser's default focus indicator showing on a programmatically focused element.

**Repro**
1. Open the public site.
2. Navigate from any page to another page that has a hero section (e.g. `/` → `/services`).
3. On the destination page, the hero heading is wrapped in a blue focus outline immediately after the page renders.
4. Click anywhere else on the page — outline disappears.
5. Repeat for any cross-page navigation — the outline reappears each time.

**Expected**
- Page transitions should not leave a visible focus ring on the hero heading for **mouse / pointer** users.
- For **keyboard** users, a clear focus indicator should still be shown (accessibility requirement — do not just blanket-remove the outline).

**Root cause (likely)**
On client-side navigation, the heading (or a "skip to main content" anchor wrapping it) is being programmatically focused for screen-reader accessibility — a common pattern to announce the new page. The element has `tabindex="-1"` (or similar) so it can receive focus. The browser then paints its default `:focus` outline, which fires regardless of input modality.

**Fix direction**
- Style the focus indicator with **`:focus-visible`** rather than `:focus`, so the outline only renders for keyboard/AT users, not after a programmatic mouse-driven navigation.
- Alternatively (less ideal), wrap the programmatic focus call so the heading receives focus but explicitly suppresses the outline (`outline: none` paired with a screen-reader-only announcement) — only acceptable if a keyboard-visible indicator is still provided elsewhere.
- Do **not** just delete the programmatic focus / `tabindex="-1"` — that breaks the accessibility behavior it was added for.

**Notes / Scope**
- Fix lives in the shared hero / page-header component (and/or the global focus CSS), so a single change covers every page.
- Verify with both mouse navigation (no outline) and `Tab` / keyboard navigation (outline still visible) before closing.

**What landed (T33)**
- Root cause confirmed: Blazor's `<FocusOnNavigate Selector="h1" />` in [`Routes.razor`](../RSD.Web/Components/Routes.razor) sets `tabindex="-1"` on the matched `<h1>` and programmatically focuses it for screen-reader page-announcement on every client-side navigation. Browsers classify that as `:focus-visible` because it follows a user gesture (the link click), so a plain `*:focus:not(:focus-visible)` rule doesn't catch it.
- Two rules added to [`Styles/app.css`](../RSD.Web/Styles/app.css) `@layer base`:
  - `*:focus:not(:focus-visible) { outline: none; }` — suppress browser default for non-keyboard focus everywhere.
  - `[tabindex="-1"]:focus, [tabindex="-1"]:focus-visible { outline: none; }` — `tabindex="-1"` elements are programmatic-focus-only by definition (removed from Tab order), so always suppress the outline. Keyboard users still get focus rings on real interactive elements.

---

## BUG-008 — Admin panel light/dark theme flips between pages

- **Status:** done
- **Reported:** 2026-05-14
- **Resolved:** 2026-05-15 (Phase 9 / T40, branch `feature/phase9-bug-polish`, commit `a721a68`)
- **Area:** Admin panel — global theming (`AdminLayout`, every admin editor + list page, shared admin components)

**Summary**
The admin panel doesn't have a consistent theme. As an admin navigates between pages, the appearance flips between light and dark seemingly at random — some pages render with white surfaces, others with the dark slate/gray-900 surfaces, with no user-controlled toggle and no consistency tied to the user, the page, or the device.

**Repro**
1. Log into `/admin` and visit any sequence of pages — Blog → Cases → Inbox → Estimates → Audit, etc.
2. Observe that the background, panel chrome, text colour, and form-input styling switch between a "white" treatment and a "dark" treatment from page to page (or sometimes between sections of the same page).
3. No user action causes the switch; reloading the same page can land on a different treatment than the previous visit.

**Expected**
- A single, deliberate visual theme for the admin panel.
- If both light **and** dark modes are supported, the choice is **stable and explicit** — driven by an admin-level toggle (with persistence) and/or the OS `prefers-color-scheme`, never random.
- Every admin page (every editor, every list, every shared dialog/toast/modal) honours the chosen mode end-to-end. No mixed surfaces inside the same view.

**Likely cause**
The admin Razor components are written with Tailwind utility pairs (`bg-white dark:bg-gray-950`, `text-gray-900 dark:text-white`, `border-gray-200 dark:border-gray-800`, etc.) sprinkled throughout, but:
- There is no global `dark` class toggle on a root element (e.g. `<html class="dark">`), so the `dark:` variant only fires when the user's OS happens to report `prefers-color-scheme: dark`.
- Some admin components only specify the light side (`bg-white` without a `dark:` companion) or only the dark side. When `prefers-color-scheme` flips mid-session, or when a component lacks the matching pair, the surfaces don't agree.
- No admin-side theme provider or user preference is persisted, so behaviour depends on the browser/OS state per render.

**Fix direction**
- Pick a target: dark-only, light-only, or supported-both-with-explicit-toggle. Whatever the choice, make it deterministic for the admin panel.
- If supporting both: add a small theme service (cookie or `localStorage`-backed) read on first render, with a `data-theme` or `class="dark"` applied to the admin layout's root element. Wire a header toggle in `AdminNavbar`.
- Sweep every admin component (`Components/Admin/Layout/`, `Components/Admin/Shared/`, every `Components/Admin/Pages/**`) to ensure every `bg-`, `text-`, `border-`, `ring-`, `placeholder:`, and `divide-` utility has the matching `dark:` companion (or doesn't depend on dark mode at all). Likely candidates for the worst offenders: forms (`FormField`, `RichTextEditor`, `ImageUploader`, `SlugField`, `TagInput`, `SeoMetaPanel`), modals (`ConfirmDialog`), the slide-over detail panels in `Inbox` / `Estimates` / `Trash`, and `ToastHost`.
- The public-site theme is separate (light-only currently); this work is scoped to `/admin/*`.

**Notes / Scope**
- Don't touch the public-site marketing pages — they're light-only by design.
- Verify by clicking through **every** admin page in both light and dark modes, ideally with `prefers-color-scheme` flipped mid-session to confirm there's no mixed state.
- Consider whether `RichTextEditor` (Quill via CDN) honours the chosen mode — Quill's default toolbar/editor may need explicit theming CSS to match.

**What landed (T40)**
- The sweep originally planned for this task found **nothing missing** — every admin layout, shared component, and block editor already had `dark:` companions on every utility (the codebase was built with `darkMode: 'class'` from the start). The reported "themes flip randomly" symptom was the absence of an explicit user override: the inline `/js/theme.js` boot script was correctly honoring `prefers-color-scheme`, but users had no way to pin the choice when their OS changed (or when DevTools simulated a different mode). The toggle closes that gap.
- New ES module [`wwwroot/js/admin/theme-toggle.js`](../RSD.Web/wwwroot/js/admin/theme-toggle.js) exports `getResolvedTheme()` and `setTheme(mode)`. The inline `/js/theme.js` boot script stays as-is — it must run before first paint to avoid a FOUC.
- New [`Components/Admin/Layout/ThemeToggle.razor`](../RSD.Web/Components/Admin/Layout/ThemeToggle.razor) (+ `.razor.cs`) — sun/moon icon button. Imports the module on first render via `IJSRuntime`, reads the resolved theme to pick the right icon, persists the new choice on click via `localStorage.theme`. `IAsyncDisposable` releases the module reference.
- `AdminNavbar` mounts `<ThemeToggle @rendermode="InteractiveServer" />` to the right of "View site". Interactive island inside the otherwise-static admin shell.
- [`Styles/app.css`](../RSD.Web/Styles/app.css) gains a Quill dark block targeting `html.dark .ql-toolbar`, `.ql-container`, `.ql-editor`, `.ql-stroke`/`.ql-fill`, `.ql-picker`, with hover/active states. Scoped to `html.dark` so the public site (light-only) is unaffected.

---

## BUG-009 — Per-manager social links are managed separately from the Team editor

- **Status:** done
- **Reported:** 2026-05-15
- **Resolved:** 2026-05-15 (branch `feature/bug-009-team-socials`, commit `038f12f`, merged via `2e9f89d`)
- **Area:** Admin / Team editor + Social Links editor; public `About → ManagementSection`

**Summary**
Each manager's social links (LinkedIn, Twitter, etc.) are currently editable only via the global `/admin/social-links` editor under `SocialLinkScope.Management`. That scope is a single shared list rendered identically under **every** manager card on the public `About` page's `ManagementSection` — so every manager visually links to the same set of URLs, which is wrong. There is no way today to set Jane Doe's LinkedIn vs John Smith's LinkedIn — those URLs need to live on the `TeamMember` editor.

**Repro**
1. Admin → Team → edit any member with `IsManagement` checked.
2. Notice there is no field for LinkedIn / X / GitHub / personal site.
3. Admin → Social links → see a single `Management`-scoped list.
4. Public `/about` → every manager card renders the same icon row pointing at the same URLs.

**Expected**
- Each `TeamMember` editor has inline social fields (LinkedIn, X/Twitter, GitHub, personal site — likely a small repeatable list of `{ Platform, Url }`).
- `ManagementSection` renders each manager's own social row, not a shared global one.
- `SocialLinkScope.Management` is either removed or repurposed (e.g. one shared "Work with us" CTA only — not per-person link icons).

**Notes / Scope**
- Schema: add a `TeamMemberSocials` owned collection (or new entity) on `TeamMember`. Decide on a fixed-platform shape vs. a generic `{ Label, Url, Icon }` list. Fixed-platform is simpler for icon mapping (LinkedIn → LinkedIn SVG, etc.) and matches how managers actually present themselves.
- Public render: `ManagementSection.razor:28` currently iterates a shared `SocialIcons` collection — switch to `@m.Socials`.
- Admin UI: inline the new fields between the `IsManagement` checkbox and the save button on [`TeamEdit.razor`](../RSD.Web/Components/Admin/Pages/Team/TeamEdit.razor). Reuse `ImageUploader` only if we keep an icon override; otherwise platform → static icon mapping is fine.
- Migration: add the new table/columns; backfill from the global `Management`-scoped list if it makes sense (likely just drop it — the existing entries are placeholder duplicates).
- Audit: same `SocialLink` entity is also used for `Footer` and `Contact` scopes — leave those alone; only the `Management` scope is replaced.

**What landed**
- **Schema** ([`20260516170000_TeamSocials`](../RSD.Web/Data/Migrations/20260516170000_TeamSocials.cs)): `team_members` gains `LinkedInUrl`, `XUrl`, `GitHubUrl` (each `varchar(500)`) + `Email` (`varchar(320)`), all `NOT NULL DEFAULT ''`. The 5 `Scope='Management'` rows in `social_links` are deleted in the same migration. Designer + `AppDbContextModelSnapshot` updated.
- **Entity + config:** `TeamMember` gains four properties. `TeamMemberConfiguration` uses new `FieldLimits.Team.SocialUrl` (500) and `FieldLimits.Team.Email` (320) constants. `SocialLinkScope` drops the `Management` enum value (still has `Footer` and `Contact`).
- **Admin:** [`TeamEdit`](../RSD.Web/Components/Admin/Pages/Team/TeamEdit.razor) gains a "Social links" fieldset (LinkedIn / X / GitHub / Email inputs). The whole fieldset is conditional on `Input.IsManagement` so non-management members don't see it; toggling the checkbox shows/hides it live (page is `@rendermode InteractiveServer`). Unticking does NOT clear saved URLs — they persist hidden, restored if the member is re-flagged as management. `SocialLinkEdit` drops the "Management section" `<option>`. `SocialLinkSeeder` drops the 5 Management entries; `TeamMemberSeeder` adds dummy URLs on the 4 Management seed entries so a fresh DB renders icons on `/about` without admin editing.
- **Public render** ([`ManagementSection`](../RSD.Web/Components/Sections/About/ManagementSection.razor)): drops the `ISocialLinkService` dependency. New `IconsFor(TeamMember)` helper yields LinkedIn / X / GitHub / Email icons only for non-empty URLs — empty fields are silently hidden. The divider + icons row are wrapped in `@if (icons.Count > 0)`, so a manager with no socials renders as avatar + name + role only with no orphaned divider line. Non-email icons open in a new tab with `rel="noopener"`; Email opens in-place via `mailto:`.
- **Side fix during the work:** `[EmailAddress]` was initially added on the `Email` input model property but had to be dropped — the attribute rejects empty strings, which silently failed `DataAnnotationsValidator` and made the "Save changes" button look dead. The `type="email"` on the input gives a browser hint; admins are responsible for typing well-formed addresses.
