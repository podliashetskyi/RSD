# RSD — QA Issues To Fix

Issues surfaced by the manual QA run on 2026-05-15 (see [qa-manual-test-script.md](qa-manual-test-script.md) for full test transcripts). Triaged from the QA run summary.

> **Current status note (2026-05-18):** This file is the original QA backlog, not the current deploy gate. For the active stabilization branch, use [`codex-ux-admin-fix-plan.md`](codex-ux-admin-fix-plan.md) as the source of truth. Entries below may describe original failures that have since been fixed by UX-001 through UX-008.

**Total:** 14 product bugs + 6 side-finding polish items. Grouped by severity. Each entry has: location · repro · root cause · fix suggestion.

**Environment for repro:** `http://localhost:8082` (Docker). Admin section needs to be logged in.

---

## Major (5)

### M1 — Footer social icons use `href="#"` (was T1.3)

**Current status:** Superseded by UX-001 / UX-011 in [`codex-ux-admin-fix-plan.md`](codex-ux-admin-fix-plan.md); fixed in the active stabilization branch.

**Where:** [Footer.razor:14](../RSD.Web/Components/Layout/Footer.razor#L14) · seed data in `SocialLinkSeeder`.

**Repro:**
1. Navigate to `http://localhost:8082/`.
2. Scroll to the footer.
3. Inspect the social icon row.
4. **Observed:** all 5 icons (LinkedIn, X, GitHub, Facebook, Instagram) have `href="#"`; clicking goes nowhere.
5. **Expected:** each icon links to a real `https://` URL **or** is not rendered at all. No `href="#"`.

**Root cause:** Template renders `<a href="@(string.IsNullOrEmpty(s.Href) ? "#" : s.Href)">` — empty `Href` on a seeded row falls back to `#`. The 5 `Footer`-scope rows in `SocialLinkSeeder` have empty URLs (placeholder data).

**Fix:**
- In [Footer.razor](../RSD.Web/Components/Layout/Footer.razor), change the foreach to skip rows with no href:
  ```razor
  @foreach (var s in Socials.Where(x => !string.IsNullOrWhiteSpace(x.Href)))
  ```
  Drop the `string.IsNullOrEmpty(s.Href) ? "#" : s.Href` ternary.
- Separately: in `SocialLinkSeeder`, either populate the 5 Footer-scope rows with real URLs or remove them entirely.

---

### M2 — No custom 404 page for unknown routes (was T1.4)

**Where:** [Program.cs](../RSD.Web/Program.cs) — no `UseStatusCodePages*` middleware registered.

**Repro:**
1. Navigate to `http://localhost:8082/this-route-does-not-exist-xyz`.
2. **Observed:** Chrome's native "HTTP ERROR 404" page renders with no app shell, header, footer, or branding.
3. **Expected:** A themed "Page not found" view inside the public site's app shell.

**Root cause:** Kestrel returns a bare 404 with no HTML body; no middleware re-executes a custom error page.

**Fix:**
- Create `/Components/Pages/NotFound.razor` with the public site's header + footer + a themed "Page not found" body (include a "Back to home" CTA and optionally search).
- In [Program.cs](../RSD.Web/Program.cs) after the existing middleware setup, add:
  ```csharp
  app.UseStatusCodePagesWithReExecute("/404", "?statusCode={0}");
  ```
- Route `/404` to the new component.
- Same 404 surfaces after a slug rename (old slug stops resolving) — the custom page helps users recover.

---

### M3 — Admin theme toggle is a no-op (was T3.3)

**Where:** [ThemeToggle.razor](../RSD.Web/Components/Admin/Layout/ThemeToggle.razor) + [AdminNavbar.razor:11](../RSD.Web/Components/Admin/Layout/AdminNavbar.razor#L11) · investigate [AdminLayout.razor](../RSD.Web/Components/Admin/Layout/AdminLayout.razor).

**Repro:**
1. Log in to `/admin`.
2. Observe the sun/moon toggle in the top bar (aria-label "Switch to dark mode").
3. Click it repeatedly.
4. **Observed:** Nothing changes. `document.documentElement.classList.contains('dark') === false`, `localStorage.theme === 'light'` (set once on first load, never flipped), aria-label never updates.
5. **Expected:** Theme flips light↔dark, persists across navigation and reload.

**Root cause hypothesis:** The rendered DOM has no `onclick` and no Blazor event delegate, which means the component is being rendered as **static SSR** even though `AdminNavbar.razor:11` has `<ThemeToggle @rendermode="InteractiveServer" />`. Either:
- `AdminLayout` itself is rendered as static SSR and a child `@rendermode` annotation doesn't bubble up in that context.
- `OnAfterRenderAsync`'s `import` of `theme-toggle.js` is failing silently, leaving `Module` null, so `ToggleAsync`'s early-return on `if (Module is null) return` swallows every click.

**Fix (pick one):**
- **A — Verify and fix render mode:** open [AdminLayout.razor](../RSD.Web/Components/Admin/Layout/AdminLayout.razor); confirm the layout (or at minimum the navbar region) renders interactive. Add a `Console.WriteLine` (or `Js.InvokeVoidAsync("console.log", "ThemeToggle:firstRender")`) inside `OnAfterRenderAsync` to confirm it ever fires.
- **B — Cheapest robust path (recommended):** drop the Blazor interactivity for this button and ship it as plain JS.
  - Render the button as static HTML with `data-theme-toggle` attribute.
  - In `wwwroot/js/admin/theme-toggle.js` (which is already loaded), attach a delegated `click` listener on `[data-theme-toggle]` at boot.
  - Compute icon state in JS from `localStorage.theme` (or the resolved theme); rotate icon + aria-label via CSS data-attribute selectors.
  - No `@rendermode` needed; no Blazor circuit needed for a toggle.

---

### M4 — Row-action Delete is single-click destructive (was T11.4)

**Current status:** Superseded for the active deploy pass by the shared `DeleteRowButton` pattern and UX-002 lead/estimate confirmations. Keep this entry only as historical context for the original QA run.

**Where:** Every `*List.razor` page with a Delete row action — Blog, Cases, Products, Services, Testimonials, Team, Partners, Values, Stats, Tech, Contact points, Messenger links, Social links, Filters.

**Repro:**
1. Log in to `/admin/blog`.
2. Click **Delete** on any row.
3. **Observed:** Row vanishes immediately. Only feedback is a "Post deleted." toast. No confirm dialog.
4. **Expected:** A confirm dialog ("Move 'X' to Trash?" / Cancel / Delete). Item only moves to trash on confirm.

**Root cause:** No `ConfirmDialog` interception on row Delete. The action goes straight to `Service.DeleteAsync`. (The strong "Type DELETE to confirm" modal in `/admin/trash` and `/admin/media` is correctly gated — the asymmetry is the issue.)

**Fix:**
- Wrap every list-page row Delete in a soft-confirm. Don't reuse the typed-DELETE modal (overkill for a recoverable soft-delete); a simple "Move 'X' to Trash?" with Cancel/Delete is enough.
- Centralize as a shared component to prevent future list pages from skipping it:
  ```razor
  <DeleteRowButton EntityLabel="@post.Title" OnConfirm="@(() => DeleteAsync(post.Id))" />
  ```
- Apply uniformly across all 14 list pages (single sweep PR).

---

### M5 — Silent save failure on Partner / Tech / Messenger editors (was T5.4, T5.7, T5.9)

**Current status:** Related UX/admin polish has moved to [`codex-ux-admin-fix-plan.md`](codex-ux-admin-fix-plan.md). Re-test after UX-012 / UX-013 if this still reproduces.

**Where:** [PartnerEdit.razor / .razor.cs](../RSD.Web/Components/Admin/Pages/Partners/), [TechStackItemEdit.razor / .razor.cs](../RSD.Web/Components/Admin/Pages/Tech/), [MessengerLinkEdit.razor / .razor.cs](../RSD.Web/Components/Admin/Pages/MessengerLinks/) · plus the three corresponding services.

**Repro (Tech, but same shape for the other two):**
1. Log in to `/admin/tech`.
2. Click `+ New tech stack item`.
3. Fill: Label `QA Tech 1724`, upload a logo (upload succeeds), Status=Published.
4. `form.checkValidity() === true`.
5. Click **Create** (or call `form.requestSubmit()`).
6. **Observed:** URL stays at `/admin/tech/new`. No toast. No inline error. No row created.
7. **Expected:** Save succeeds and redirects to the list, **or** an error is surfaced visibly.

**Sibling control:** Value, Stat, Contact point — same shape (Label + media path + Status + DisplayOrder) — save cleanly. So it's not a generic Blazor/form issue.

**Root cause hypothesis:** The `.razor.cs` files for Tech and Value are structurally identical (verified) — same `SaveAsync`, same `if (!ok) { ErrorMessage = error; return; }`, same upload handler. So the divergence is one of:
1. **Most likely:** The `.razor` template for these three editors doesn't render `@ErrorMessage` visibly. The service returns `(Ok: false, Error: "some validation message")` and the message is set on the field but never shown. Same pattern as M9 (duplicate slug).
2. The service layer has an extra validation (unique-`Label`, required `Slug`, file-size limit on second uploader) not reflected in the Razor.
3. For Messenger specifically: race between the second `ImageUploader`'s async completion and form submit, leaving `Input.SmallIconPath` empty when `SaveAsync` runs.

**Fix:**
1. Open all three `*Edit.razor` templates. Confirm `@ErrorMessage` is rendered at the top of the form with `role="alert"` and amber/red text. If it's not, add it — one line per file likely closes most of these.
2. Add `Console.WriteLine($"SaveAsync failed: {error}")` (or a server toast) on the `!ok` branch so silent server validation surfaces during dev.
3. For Messenger: disable the Save button until both `ImageUploader`s have reported success (track two `bool LogoLargeReady` / `LogoSmallReady` flags).
4. Grep `PartnerService`, `TechStackItemService`, `MessengerLinkService` for hidden validation (unique constraints, required fields the form doesn't expose).

---

## Minor (6)

### m1 — Reset-password page renders form for missing/invalid tokens (was T2.7)

**Where:** [ResetPassword.razor / .razor.cs](../RSD.Web/Components/Admin/Pages/)

**Repro:**
1. Log out.
2. Navigate to `http://localhost:8082/admin/reset-password` (no query).
3. Then to `http://localhost:8082/admin/reset-password?token=garbage`.
4. **Observed:** Both render the reset form (New password + Confirm new password + Reset button) as if the token were valid. No "Invalid or expired link" message.
5. **Expected:** Token missing/bogus → show error state, hide password fields.

**Root cause:** GET handler doesn't validate the token before rendering.

**Fix:**
- In `OnParametersSetAsync` (or the GET handler), call `UserManager.VerifyUserTokenAsync` (or equivalent for password-reset) with the provided token.
- If absent/invalid, set a `TokenValid = false` flag and render an "Invalid or expired link" view with a "Request a new reset link" CTA pointing at `/admin/forgot-password`.
- **Also verify server-side POST rejection** — the QA tester deliberately didn't submit a bogus-token form to avoid affecting the live admin account. Don't want to discover the POST handler also skips validation.

---

### m2 — Restore from Trash brings a post back as Draft (was T6.2)

**Where:** Restore logic in trash service (look for `RestoreAsync` in `ITrashService` / corresponding `*Service`).

**Repro:**
1. Publish a blog post.
2. From `/admin/blog`, click Delete on the post.
3. Go to `/admin/trash`, click **Restore**.
4. **Observed:** Post returns to `/admin/blog` with Status=Draft. `/blog/{slug}` returns 404.
5. **Expected:** Restored post returns with its previous Status (was Published → should still be Published) **or** the UX clearly tells the admin it's been demoted to Draft.

**Root cause:** Either the soft-delete mutates `Status` (it shouldn't), or restore uses a default-Status path that resets to Draft.

**Fix:**
- Audit `SoftDeleteAsync` — confirm it does not touch `Status`.
- Audit `RestoreAsync` — confirm it leaves the entity exactly as it was pre-delete.
- **Alternative product decision:** if restore-as-Draft is intentional safe-default (so the admin can review before re-publishing), keep it but:
  - Update the restore toast to "Restored as Draft — re-publish to make public."
  - Add a note in the Trash detail pane: "Restoring will return this item as Draft."

---

### m3 — `/cases` empty filter state has no message (was T10.3)

**Where:** [CasesGridSection.razor](../RSD.Web/Components/Sections/Cases/CasesGridSection.razor) (or wherever the grid is rendered).

**Repro:**
1. Navigate to `/cases`.
2. Apply filters that match nothing (e.g. Industry=EdTech + Tech Stack=AWS).
3. **Observed:** The grid area is empty — no message between the filter chips and the "Want Similar Results?" CTA.
4. **Expected:** "No cases match these filters. [Clear filters]" message in the grid area.

**Fix:**
- After the existing `@foreach (var c in DisplayedCases)`, add:
  ```razor
  @if (!DisplayedCases.Any())
  {
      <div class="text-center py-12 text-ink-muted">
          <p>No cases match these filters.</p>
          <button @onclick="ClearAll" class="mt-4 underline">Clear filters</button>
      </div>
  }
  ```
- `PostsGridSection` (blog) already has an equivalent empty state per T37 notes — confirm visual parity.

---

### m4 — SEO meta panel missing visible char counter (was T11.2)

**Current status:** Fixed before this stabilization pass; `SeoMetaPanel` now uses `FieldField` counters for meta title and description.

**Where:** [SeoMetaPanel.razor](../RSD.Web/Components/Admin/Shared/SeoMetaPanel.razor)

**Repro:**
1. Log in to `/admin/blog/{any-post}`.
2. Scroll to the **SEO** section.
3. **Observed:** Meta title (`maxlength="200"`) and Meta description (`maxlength="500"`) inputs have no character counter beneath them. (The hard cap is enforced by the browser, but admin sees no warning approaching the limit.)
4. **Expected:** Live `n / max` counter under each field, amber at 90%, red at cap — same as the Summary counter on the post body.

**Root cause:** The `FieldField` wrapper that renders the counter (added in T38) hasn't been applied to the SEO panel.

**Fix:**
- In `SeoMetaPanel.razor`, wrap the Meta Title `<InputText>` and Meta Description `<InputTextArea>` in `FieldField` with the appropriate `Max` values:
  ```razor
  <FieldField Label="Meta title" Max="@FieldLimits.Seo.Title">
      <InputText @bind-Value="..." maxlength="@FieldLimits.Seo.Title" />
  </FieldField>
  <FieldField Label="Meta description" Max="@FieldLimits.Seo.Description">
      <InputTextArea @bind-Value="..." maxlength="@FieldLimits.Seo.Description" />
  </FieldField>
  ```

---

### m5 — Duplicate slug silently rejected (was T11.3)

**Where:** `BlogPostService.CreateAsync` + likely parallel services for Cases / Products / Services / any other entity with a unique Slug.

**Repro:**
1. Log in to `/admin/blog/new`.
2. Title `QA Dup 1655`, Summary `Dup slug test.`, Description anything.
3. Unlock slug, paste an existing slug (e.g. `security-first-fintech`).
4. Click **Create post**.
5. **Observed:** URL stays at `/admin/blog/new`. User's input retained. **No inline error**, no toast, no validation summary, no `aria-invalid`.
6. **Expected:** Visible "Slug already in use" message — inline under the slug field, **or** toast, **or** form-top validation summary.

**Root cause:** Service returns `(Ok: false, Error: "slug in use")` but the editor's `@ErrorMessage` rendering isn't surfacing it visibly. Same root cause as M5 — overlapping fix.

**Fix:**
- Same pattern as M5: ensure every editor renders `@ErrorMessage` near the form top with `role="alert"`.
- Better long-term: return a structured `ServiceResult` with `FieldErrors["Slug"] = "Already in use"`. Razor then renders the message inline under the slug field with `aria-invalid="true"`. Apply to every editor with a unique constraint (Slug, Email-on-user, Label-on-tech-stack, etc.).

---

### m6 — Admin sidebar doesn't collapse at narrow viewports (was T12.6)

**Where:** [AdminLayout.razor](../RSD.Web/Components/Admin/Layout/AdminLayout.razor) + [AdminSidebar.razor](../RSD.Web/Components/Admin/Layout/AdminSidebar.razor)

**Severity:** Cosmetic per script rule (content reachable via horizontal scroll). Admin is documented desktop-first.

**Repro:**
1. Resize Chrome to ≤ 768px wide.
2. Log in, navigate to `/admin/blog`.
3. **Observed:** Full sidebar (Content + Operations) stays visible, eating ~280px. Content area needs horizontal scroll to reach the Actions column. No hamburger.
4. **Expected:** Sidebar collapses to a hamburger / drawer at narrow widths.

**Fix (low priority):**
- Add a hamburger button to `AdminNavbar` visible at `lg:hidden`. Wire it to toggle `data-sidebar-open` on the layout root.
- CSS: at `< lg` widths, position the sidebar `fixed left-0` translated off-screen, slide in when `data-sidebar-open="true"`, overlay a dark backdrop, dismiss on backdrop click.
- Don't invest heavily — admin is desktop-first, and the QA confirmed everything is reachable via h-scroll today.

---

## Side findings (polish — not test failures)

These are not in the QA-pass/fail tally but worth filing as small tickets:

### s1 — `/products` H1 missing space + lowercase `Saas`

- **Where:** Products page hero / heading component.
- **Observed:** H1 reads "Ready-to-UseSaas Solutions".
- **Fix:** Update copy to "Ready-to-Use SaaS Solutions" (proper casing, proper space).

### s2 — `/cases` H1 renders both desktop and mobile variants concatenated

- **Where:** Cases page hero section.
- **Observed:** DOM contains both "OurSuccessful Projects" and "Our SuccessfulProjects" rendered in the same string.
- **Fix:** One variant needs `hidden md:block`, the other `block md:hidden`. Verify breakpoint switch.

### s3 — After saving a new blog post, redirect goes to list instead of edit URL

**Current status:** Not part of the active UX-001 through UX-013 deploy gate. Keep as backlog unless promoted.

- **Where:** [BlogEdit.razor.cs](../RSD.Web/Components/Admin/Pages/Blog/BlogEdit.razor.cs) `SaveAsync`.
- **Observed:** Creates a new post → redirect to `/admin/blog` (list). Script expected `/admin/blog/{guid}` (edit page) so the admin can keep iterating.
- **Fix:** On the Create branch of `SaveAsync`, change `Nav.NavigateTo("/admin/blog")` to `Nav.NavigateTo($"/admin/blog/{created.Id}")`. Audit Cases / Products / Services for the same pattern.

### s4 — `/admin/blog` list missing Slug column

- **Where:** [BlogList.razor](../RSD.Web/Components/Admin/Pages/Blog/BlogList.razor).
- **Observed:** Columns are Title · Category · Status · Updated · Actions. Spec expected Slug as "at minimum".
- **Fix:** Either add Slug as a hidden-by-default column toggle (the table component might already support it) **or** accept Category as a deliberate UX call — in which case update the test script's expectation.

### s5 — Invited users render as "Active" instead of "Invited" / "Pending"

- **Where:** [UsersList.razor](../RSD.Web/Components/Admin/Pages/Users/UsersList.razor).
- **Observed:** New invitee has Last login = Never, Status = Active. Admin can't tell at-a-glance whether a row is a never-logged-in invitee or a real active user.
- **Fix:** Differentiate `LastLoginAt == null` users with an "Invited" badge (or "Pending"), distinct from "Active".

### s6 — Privacy Policy page title may still have " QA marker"

- **Where:** Admin `/admin/privacy-policy` data — leftover from the QA run that couldn't reliably revert.
- **Observed:** The QA tester appended " QA marker" to the Page title to test edit propagation and couldn't reliably clear it via the agent's keystroke driver.
- **Fix:** Open `/admin/privacy-policy` manually, remove " QA marker" from the Page title field, save. (Not a code bug — just data cleanup.)

---

## Suggested order of attack

Grouped by leverage:

1. **One-line / one-file fixes with high impact** — bundle into a single PR:
   - **M1** (Footer foreach + seed cleanup)
   - **m3** (cases empty state)
   - **m4** (wrap SEO inputs in `FieldField`)
   - **M5 + m5** (verify `@ErrorMessage` is rendered visibly in each editor — likely closes 4 issues in one careful pass)

2. **Scoped to one feature each:**
   - **m1** (reset-password token validation — both GET and POST)
   - **m2** (restore preserves Status — or update UX wording)
   - **M4** (wrap row Deletes in a shared `DeleteRowButton` confirm component)

3. **Bigger / requires design call:**
   - **M2** (custom 404 page + middleware)
   - **M3** (theme toggle — investigate render mode OR rewrite as plain JS)
   - **m6** (responsive admin sidebar — defer unless someone actually edits on mobile)

4. **Copy polish (5 min each):**
   - **s1** through **s6**.

---

## Out-of-scope for this list

The following were "Blocked" in the QA run but are not product bugs:

- **T3.4** Quill dark theme — blocked by M3 (theme toggle). Re-test once M3 is fixed.
- **T4.7** Summary fallback to Description — blocked by agent-side keystroke binding limits, not a product issue. Have a human manually clear Summary on a published post and confirm `/blog` card falls back to the truncated Description.
- **T7.2** Direct-upload UI on `/admin/media` — per the QA script's own escape hatch. Not a bug. Could be a future feature if useful.
