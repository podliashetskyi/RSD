# RSD — Codex UX/Admin Fix Plan

**Date:** 2026-05-18  
**Reviewer:** Codex  
**Status:** In progress — P0 resolved; UX-012 and UX-013 remain  
**Scope:** Public-site link UX, admin CMS usability, editor safety, and pre-deploy polish.

This document is the reviewed fix queue before deployment. It consolidates findings from:

- `research/tasks-and-bugs.md`
- `research/qa-issues-to-fix.md`
- `research/qa-manual-test-script.md`
- `research/image-audit.md`
- Current project file review on 2026-05-18

## Recommendation

Do one focused stabilization pass before deploy. Do not add new features except where required to make existing admin workflows usable and safe.

Target outcome:

- No public `href="#"` or malformed social/messenger links.
- No stale placeholder brand/contact data on public pages.
- No single-click destructive admin actions for leads or content.
- Editors can complete core CMS workflows without developer-only fields.
- Admin dashboard reflects real operational state, not placeholder phase text.
- Known QA docs are reconciled so “done” means done in current code and current seed data.

## Status Legend

- **open** — not started
- **in-progress** — being fixed
- **done** — fixed and verified
- **deferred** — consciously postponed, not deploy-blocking

## P0 — Deploy Blockers

### UX-001 — Remove fake public links everywhere

- **Status:** done
- **Severity:** P0
- **Area:** Public site, seed data, admin validation
- **Files:**
  - `RSD.Web/Components/Sections/Contact/ContactSection.razor`
  - `RSD.Web/Data/Seed/SocialLinkSeeder.cs`
  - `RSD.Web/Data/Seed/MessengerLinkSeeder.cs`
  - `RSD.Web/Components/Layout/Footer.razor`
  - `RSD.Web/Components/Admin/Pages/SocialLinks/SocialLinkEdit.razor`
  - `RSD.Web/Components/Admin/Pages/MessengerLinks/MessengerLinkEdit.razor`

**Problem**
The footer was partially fixed to skip empty URLs, but the same bug still exists elsewhere. Contact messenger cards render `href="#"`; contact social seed rows use `"#"`; the running DB also showed footer links with `"#"` and one malformed `google.com` URL.

**Expected**
Public pages must never render fake links. A social/messenger item with no valid link should be hidden or rendered as non-clickable content. Admin should reject `#`, bare domains, and invalid URLs.

**Fix**

- Use `m.Href` for messenger links instead of hardcoded `"#"`.
- Filter `Socials` and `Messengers` to render only valid URLs.
- Update seed data: empty string for unknown URLs, not `"#"`.
- Add server-side URL validation to social and messenger services.
- Add visible inline/admin-form errors for invalid URLs.
- Decide supported schemes:
  - Social: `https://`
  - Email: `mailto:`
  - Phone: `tel:`
  - Messenger: platform-specific `https://`, `tg://`, `viber://`, `whatsapp://` only if intentionally supported.

**Acceptance Criteria**

- `rg 'href="#"' RSD.Web/Components` returns no public-facing placeholder links.
- `/contact` renders no `href="#"`.
- Footer renders no `href="#"` and no bare-domain links.
- Admin cannot save `#` or `google.com` as a link.

### UX-002 — Add confirmation for Inbox and Estimate delete

- **Status:** done
- **Severity:** P0
- **Area:** Admin operations
- **Files:**
  - `RSD.Web/Components/Admin/Pages/Inbox/InboxList.razor`
  - `RSD.Web/Components/Admin/Pages/Inbox/InboxList.razor.cs`
  - `RSD.Web/Components/Admin/Pages/Estimates/EstimateList.razor`
  - `RSD.Web/Components/Admin/Pages/Estimates/EstimateList.razor.cs`
  - `RSD.Web/Services/Content/ContactSubmissionService.cs`
  - `RSD.Web/Services/Estimates/ProjectEstimateService.cs`

**Problem**
Lead records are hard-deleted from slide-over details with one click. These records are more sensitive than regular CMS content because they are inbound business leads and contain PII.

**Expected**
No lead or estimate can be deleted without confirmation. Prefer archive/handled retention over hard delete unless there is a clear data-retention reason.

**Fix**

- Add `ConfirmDialog` to Inbox and Estimates delete actions.
- Copy should be explicit: “Delete this submission permanently?”
- Consider a second phase: replace hard delete with `ArchivedAt` / retention workflow.

**Acceptance Criteria**

- Clicking Delete in Inbox opens a confirm dialog.
- Clicking Delete in Estimates opens a confirm dialog.
- Cancel keeps the record.
- Confirm deletes and shows a toast.

### UX-003 — Complete Blog author editing

- **Status:** done
- **Severity:** P0
- **Area:** Admin Blog editor, public blog detail/list metadata
- **Files:**
  - `RSD.Web/Components/Admin/Pages/Blog/BlogEdit.razor`
  - `RSD.Web/Components/Admin/Pages/Blog/BlogEdit.razor.cs`
  - `RSD.Web/Services/Content/TeamMemberService.cs`
  - `RSD.Web/Components/Pages/BlogDetail.razor.cs`
  - `RSD.Web/Components/Sections/Blog/PostsGridSection.razor.cs`

**Problem**
`AuthorId` exists in the model and is preserved, but the Blog edit form does not expose an author picker. Editors cannot set or change the author.

**Expected**
Blog editor has an author selector populated from Team Members. Public blog cards/details use the selected author.

**Fix**

- Load published team members in `BlogEdit`.
- Add an `Author` `<InputSelect>` with “RSD Team” fallback.
- Show name + role/avatar preview if selected.
- Keep fallback behavior for old posts with no author.

**Acceptance Criteria**

- Admin can create/edit a post and select an author.
- Saved author persists after reload.
- Blog list/detail shows selected author name/avatar.
- Empty author still renders “RSD Team” cleanly.

### UX-010 — Fix remaining public placeholder actions

- **Status:** done
- **Severity:** P0
- **Area:** Public site
- **Files:**
  - `RSD.Web/Components/Sections/Contact/ContactForm.razor`
  - `RSD.Web/Components/Sections/About/ManagementSection.razor`
  - `RSD.Web/Components/Sections/Article/ArticleHeaderSection.razor`

**Problem**
Additional public `href="#"` links still exist outside the social/messenger system:

- Contact form Terms of Service link.
- About Management “Work with us” link.
- Blog/article share buttons.

These are visible user actions. Fake anchors make the page feel unfinished and break keyboard/link semantics.

**Expected**
Every visible public action either works, is hidden, or is rendered as a non-link element if it is only decorative.

**Fix**

- Contact form terms link should point to `/terms-of-service`.
- “Work with us” should point to `/contact` or `/estimate`, or be hidden until the destination is decided.
- Article share buttons should generate real share URLs from the current article URL/title, or the whole Share block should be removed until implemented.

**Acceptance Criteria**

- No public component renders `href="#"`.
- Contact form Terms link opens Terms of Service.
- Article share actions either work or are not rendered.

### UX-011 — Remove stale placeholder brand/contact data

- **Status:** done
- **Severity:** P0
- **Area:** Public seed data
- **Files:**
  - `RSD.Web/Data/Seed/ContactPointSeeder.cs`
  - `RSD.Web/Data/Seed/TeamMemberSeeder.cs`
  - Any existing runtime DB seed rows edited during QA

**Problem**
Seed data still includes stale placeholder identity, for example `hello@nexatech.io`. The real brand strings are `RemSoft.Dev`, `RSD`, and `Remote Software Development`; placeholder company data should not appear on deploy.

**Expected**
Public contact information should be real or intentionally blank/hidden. No `nexatech`, dummy social profile, or test URLs should leak into public pages.

**Fix**

- Replace `hello@nexatech.io` with the correct RSD email.
- Replace dummy management social URLs (`https://www.linkedin.com/`, `https://x.com/`, etc.) with empty strings unless real profiles are provided.
- Add a rendered-page grep/check for known dummy strings.

**Acceptance Criteria**

- Rendered public pages contain no `nexatech`.
- Rendered public pages contain no dummy profile links.
- Empty optional contact/social values are hidden cleanly.

## P1 — Should Fix Before Deploy

### UX-004 — Hide developer-only image path inputs

- **Status:** done
- **Severity:** P1
- **Area:** Admin editors
- **Files:** All editors using `ImageUploader` plus manual path fallback.

**Problem**
Editors still see “Or paste a path manually.” That keeps the CMS developer-facing and invites broken images.

**Expected**
Image upload is the normal path. Manual path entry is hidden behind an advanced/dev affordance or removed from production UI.

**Fix**

- Introduce an `Advanced` disclosure for manual path fields, or remove them from non-dev builds.
- Keep the bound path in the model, but do not present it as the primary editor workflow.

**Acceptance Criteria**

- New non-technical editor sees upload/preview/remove, not a raw path field.
- Existing saved paths still render.
- QA can still inspect path if the advanced disclosure is used.

### UX-005 — Replace placeholder admin dashboard

- **Status:** done
- **Severity:** P1
- **Area:** Admin dashboard
- **File:** `RSD.Web/Components/Admin/Pages/Index.razor`

**Problem**
The dashboard still says content screens land in Phase 2, even though the admin is live.

**Expected**
Dashboard provides real operational value.

**Fix**

- Show counts:
  - Open inbox submissions
  - Open estimates
  - Draft blog/cases/products/services
  - Recent audit events
  - Recently uploaded media
- Add quick links to common actions.

**Acceptance Criteria**

- No phase-placeholder copy remains.
- Admin landing page helps decide what needs attention.

### UX-006 — Make admin tables responsive

- **Status:** done
- **Severity:** P1
- **Area:** Admin shared table
- **File:** `RSD.Web/Components/Admin/Shared/AdminDataTable.razor`

**Problem**
Tables are full-width with `overflow-hidden`, which makes wide admin lists brittle on smaller screens and with long data.

**Expected**
Tables should remain usable on tablet/narrow desktop without losing columns or clipping actions.

**Fix**

- Wrap table in `overflow-x-auto`.
- Add `min-w-*` for known dense tables.
- Re-check row actions on Blog, Cases, Products, Services, Inbox, Estimates, Media, Audit.

**Acceptance Criteria**

- At 768px wide, table actions remain reachable.
- No important text or buttons are clipped.

### UX-007 — Improve dialog and drawer accessibility

- **Status:** done
- **Severity:** P1
- **Area:** Admin shared dialogs and slide-overs
- **Files:**
  - `RSD.Web/Components/Admin/Shared/ConfirmDialog.razor`
  - `RSD.Web/Components/Admin/Pages/Inbox/InboxList.razor`
  - `RSD.Web/Components/Admin/Pages/Estimates/EstimateList.razor`
  - `RSD.Web/Components/Admin/Pages/Trash/TrashList.razor`
  - `RSD.Web/Components/Admin/Pages/Media/MediaGrid.razor`

**Problem**
Dialogs have basic ARIA, but focus management is incomplete. Slide-overs are custom and need Escape/focus behavior.

**Expected**
Opening a dialog/drawer moves focus inside it, Escape closes it, and keyboard focus does not wander behind it.

**Fix**

- Add focus-on-open and Escape handling.
- Add focus restore to the invoking control where practical.
- Consider a tiny JS helper for focus trap if Blazor-only gets awkward.

**Acceptance Criteria**

- Confirm dialogs are keyboard usable.
- Inbox/Estimate drawers close with Escape.
- Focus does not move behind open modal content.

### UX-012 — Remove raw Tailwind/CSS class editing from content body forms

- **Status:** open
- **Severity:** P1
- **Area:** Admin Case/Product body editors
- **Files:**
  - `RSD.Web/Components/Admin/Pages/Cases/CaseBodyEditor.razor`
  - `RSD.Web/Components/Admin/Pages/Products/ProductBodyEditor.razor`
  - `RSD.Web/Components/Admin/Shared/BodyForms/BadgeRow.cs`

**Problem**
Case/Product body editors expose fields like `Background class` and `Text class`. That is a developer API, not a content editor UI.

**Expected**
Editors choose from semantic visual variants, not Tailwind class strings.

**Fix**

- Replace class text inputs with a small select such as `Neutral`, `Blue`, `Green`, `Amber`, `Red`, `Purple`.
- Map those semantic variants to CSS classes in code.
- Preserve existing saved class values by mapping known current classes to variants, with a fallback.

**Acceptance Criteria**

- Editors cannot type arbitrary CSS/Tailwind classes.
- Existing badge styling still renders.
- Admin form labels describe content intent, not implementation.

### UX-013 — Make admin search labels match what is searched

- **Status:** open
- **Severity:** P1
- **Area:** Admin list UX
- **Files:** All admin list pages with `placeholder="Search slug…"`

**Problem**
Many admin lists say “Search slug…” even when the user thinks in names/titles/labels. This is technically accurate in some services but bad UX.

**Expected**
Search placeholders should match editor language and actual indexed fields.

**Fix**

- Change placeholders to “Search title…”, “Search name…”, “Search label…”, or “Search…” as appropriate.
- Ideally update service search to include the visible label/title/name, not only slug.

**Acceptance Criteria**

- List search placeholders no longer default to `slug` unless slug is truly the primary search target.
- Search finds the visible row name/title for major content types.

## P2 — Important Polish

### UX-008 — Normalize admin language

- **Status:** done
- **Severity:** P2
- **Area:** Admin labels and hints

**Problem**
Some labels expose implementation language: `Href`, `Slug`, `Display order`, `OG image`, `Scope`.

**Expected**
Labels should match editor intent first, technical detail second.

**Fix Examples**

- `Href` → `Link URL`
- `Scope` → `Where this appears`
- `Display order` → `Sort order`
- `OG image` → `Social sharing image`
- `Slug` can stay, but hint should say “URL path.”

**Acceptance Criteria**

- A non-technical editor can infer what each field changes.

### UX-009 — Reconcile research docs

- **Status:** done
- **Severity:** P2
- **Area:** Research/process
- **Files:**
  - `research/tasks-and-bugs.md`
  - `research/qa-issues-to-fix.md`
  - `research/qa-manual-test-script.md`
  - `research/image-audit.md`

**Problem**
Some docs report issues as resolved while current code/data still show related failures. This causes repeated rediscovery.

**Expected**
Research docs should clearly distinguish fixed-in-code, fixed-in-seed, fixed-in-current-DB, deferred, and still open.

**Fix**

- Mark superseded QA items as superseded by this document.
- Move still-open items here or link them clearly.
- After fixes, run a short verification pass and update statuses.

**Acceptance Criteria**

- One source of truth exists for pre-deploy blockers.
- No known P0/P1 issue is duplicated with conflicting status.

## Suggested Fix Order

1. UX-001 fake links and URL validation.
2. UX-002 delete confirmations for leads/estimates.
3. UX-003 blog author selector.
4. UX-010 remaining public placeholder actions.
5. UX-011 stale placeholder brand/contact data.
6. UX-004 hide raw image paths.
7. UX-005 dashboard.
8. UX-006 responsive tables.
9. UX-007 dialog/drawer accessibility.
10. UX-012 remove raw CSS/Tailwind body editing.
11. UX-013 admin search labels.
12. UX-008 language cleanup.
13. UX-009 docs reconciliation.

## Current Remaining Work

- **UX-012:** Remove raw Tailwind/CSS class editing from content body forms.
- **UX-013:** Make admin search labels match what is searched.

## Reconciliation Notes

- This document is the current pre-deploy UX/admin source of truth for the stabilization branch.
- `research/qa-issues-to-fix.md` is retained as the original QA-run backlog. Items already represented in this plan should be treated according to the UX status here.
- Older QA issues that are not represented here, such as custom 404 handling, reset-password token validation, case empty states, and responsive sidebar polish, remain separate backlog candidates unless Mark promotes them into this deploy pass.
- `research/qa-manual-test-script.md` remains the raw 2026-05-15 test transcript and should not be read as current pass/fail status without a fresh rerun.

## Verification Plan

Before deploy:

- Run full automated test suite.
- Run focused manual QA for:
  - `/contact`
  - footer links
  - `/admin/blog/new`
  - `/admin/inbox`
  - `/admin/estimates`
  - one media upload editor
  - one dense table at 768px
- Use a rendered-page check for `href="#"`.
- Use a rendered-page check for stale dummy strings (`nexatech`, bare `google.com`, `example.com` where public-facing).
- Use admin create/edit/save cycles, not just code inspection.

## Deploy Gate

Deployment is allowed only when:

- All P0 items are `done`.
- All P1 items are `done` or explicitly accepted as `deferred`.
- `research/qa-issues-to-fix.md` and this document do not contradict each other.
- Current DB seed/runtime data has no fake visible public links.
