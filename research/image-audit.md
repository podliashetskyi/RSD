# RSD — Image audit (TASK-004)

Snapshot of every admin editor that exposes an image field and every public-site `<img>` tag, as of 2026-05-14. Produced as the scoping doc for [TASK-005](tasks-and-bugs.md#task-005--replace-image-path-text-inputs-with-a-real-image-upload-control-across-the-admin) (image-upload control rollout + alt-text gaps).

---

## 1. Admin image-field inventory

| Editor | Field | Image type | Current control | Subfolder | Alt source (target) |
|---|---|---|---|---|---|
| `RSD.Web/Components/Admin/Pages/Blog/BlogEdit.razor` | `CoverImagePath` | cover | `ImageUploader` ✅ | `blog` | new column `CoverImageAlt` (T32) |
| `RSD.Web/Components/Admin/Pages/Cases/CaseEdit.razor` | `CoverImagePath` | cover | `ImageUploader` ✅ | `cases` | new column `CoverImageAlt` (T32) |
| `RSD.Web/Components/Admin/Pages/Products/ProductEdit.razor` | `CoverImagePath` | cover | `ImageUploader` ✅ | `products` | new column `CoverImageAlt` (T32) |
| `RSD.Web/Components/Admin/Pages/Services/ServiceEdit.razor` | `CoverImagePath` | cover | `ImageUploader` ✅ | `services` | new column `CoverImageAlt` (T32) |
| `RSD.Web/Components/Admin/Pages/Team/TeamEdit.razor` | `AvatarPath` | avatar | `InputText (plain path)` ❌ | `team` | derived from `Name` |
| `RSD.Web/Components/Admin/Pages/Testimonials/TestimonialEdit.razor` | `AvatarPath` | avatar | `InputText (plain path)` ❌ | `testimonials` | derived from `AuthorName` |
| `RSD.Web/Components/Admin/Pages/Cases/CaseBodyEditor.razor` (embedded testimonial block) | `AvatarPath` | avatar | `InputText (plain path)` ❌ | `testimonials` | derived from `AuthorName` |
| `RSD.Web/Components/Admin/Pages/Partners/PartnerEdit.razor` | `PhotoPath` | logo | `InputText (plain path)` ❌ | `partners` | derived from `Name` |
| `RSD.Web/Components/Admin/Pages/Tech/TechStackItemEdit.razor` | `LogoPath` | logo | `InputText (plain path)` ❌ | `tech` | derived from `Label` |
| `RSD.Web/Components/Admin/Pages/Values/ValueEdit.razor` | `IconPath` | icon | `InputText (plain path)` ❌ | `values` | decorative (`alt=""` + `aria-hidden="true"`) |
| `RSD.Web/Components/Admin/Pages/SocialLinks/SocialLinkEdit.razor` | `IconPath` | icon | `InputText (plain path)` ❌ | `social-links` | decorative (`alt=""` + `aria-hidden="true"`) |
| `RSD.Web/Components/Admin/Pages/MessengerLinks/MessengerLinkEdit.razor` | `LargeIconPath` | icon | `InputText (plain path)` ❌ | `messenger-links` | decorative |
| `RSD.Web/Components/Admin/Pages/MessengerLinks/MessengerLinkEdit.razor` | `SmallIconPath` | icon | `InputText (plain path)` ❌ | `messenger-links` | decorative |
| `RSD.Web/Components/Admin/Shared/SeoMetaPanel.razor` (used by ~14 editors) | `OgImagePath` | OG meta | `InputText (plain path)` ❌ | `seo` | new column `SeoOgImageAlt` on owned `SeoMetadata` (T32) |

**Summary:** 4 editors already use `ImageUploader`; **9 editors + 1 shared panel still expose a plain text path input**. None of the editors currently capture an explicit alt-text value.

**Public-site headline:** the hero decoration is mostly fine — every decorative `<img>` already sits inside a `<div aria-hidden="true">` (or carries its own `aria-hidden`), so it's already invisible to screen readers. Only one stray case (`Blog/HeroSection.razor:16`, the search icon) was missing it; T30 fixes it. The real public-side a11y work is the small set of **content** images with the wrong `alt=""` — author avatars, partner logos, detail-page hero — all of which need a real alt string sourced from the CMS in T32.

---

## 2. Public-site `<img>` alt-text gap list

Grouped by recommended treatment. File:line references point at the offending tag.

### 2a. Decorative — `alt=""` with **no** parent `aria-hidden="true"` (mechanical fix in T30)

Strictly speaking, only `<img>` tags whose ancestor chain doesn't already carry `aria-hidden="true"` need img-level `aria-hidden`. After walking each hero section, **only one** image qualifies:

| File:line | Why |
|---|---|
| `RSD.Web/Components/Sections/Blog/HeroSection.razor:16` | Decorative search icon sitting in a `<span>` next to an `<input aria-label="Search articles">`. The icon adds no information, but its parent span has no `aria-hidden`, so screen readers see it. |

T30 adds `aria-hidden="true"` here and nowhere else.

### 2b. Already hidden — keep as-is

Two reasons an `<img alt="">` doesn't need img-level `aria-hidden`:

**(i) Wrapped in a `<div aria-hidden="true">` ancestor** — already transparent to assistive tech.

- `Contact/HeroSection.razor:5, 9, 13, 17, 21` (mobile tiles, parent at line 2) and `:70, 74, 78` (desktop tiles, each in its own aria-hidden parent)
- `Products/HeroSection.razor:5, 9, 13, 17, 21, 25` (mobile tiles, parent at line 2)
- `Cases/HeroSection.razor:5, 9, 13, 17, 21, 25` (mobile tiles, parent at line 2) and `:32, 36, 40, 44, 63, 71, 79` (named case screenshots inside the `lg:block` hero composition — parent at line 29; these are decorative composition pieces, not entry points)
- `Services/HeroSection.razor:5, 9, 13, 17, 21, 25` (mobile tiles, parent at line 2) and `:30, 35, 39, 43, 47, 51, 55` (desktop tiles + connectors-bg, parent at line 29)
- `About/HeroSection.razor:4, 7, 10, 13, 17, 22, 27` (mobile decorative overlays, parent at line 2) and `:78, 82, 86, 92, 98` (desktop floating overlays — each `<div aria-hidden="true">`)

**(ii) Already carrying explicit `aria-hidden="true"`** — belt-and-suspenders fine.

- `Contact/HeroSection.razor:51, 56, 60, 64` · `Contact/ContactForm.razor:66` · `Contact/ContactSection.razor:2, 47, 61, 76, 97`
- `About/HeroSection.razor:55` · `About/ManagementSection.razor:10, 31` · `About/PartnersSection.razor:24`
- `Services/FeaturesSection.razor:19` · `Services/HeroSection.razor:82, 87, 91, 95, 99, 103, 107`
- `Blog/PostsGridSection.razor:12` · `Shared/CasesGridSection.razor:46`

**(iii) Already carrying a meaningful `alt`** — content image, no action.

- `About/HeroSection.razor:60, 64, 68, 72` — named team-member avatars (David Chen, Anna Williams, Maria Johnson, Alexander Smith)
- `Article/FeaturedImageSection.razor:4` (alt bound to `@Caption`)

### 2c. Content images needing real alt text (handled in T32)

These render meaningful content; `alt=""` is wrong and `aria-hidden` would hide them from screen readers. Each needs a real alt sourced from the CMS or the surrounding model.

| File:line | Image | Target alt source |
|---|---|---|
| `Sections/Article/ArticleHeaderSection.razor:34` | Author avatar | bind to author `Name` (parameter `AuthorName`) |
| `Sections/Detail/HeroSection.razor:2` | Case/Service/Blog detail hero | bind to the entity's new `CoverImageAlt` (with `?? Title` fallback) |
| `Sections/Cases/HeroSection.razor:32, 36, 40, 44, 63, 71, 79` | Named case screenshots in the index hero composition (financehub / healthcare / ecologistics / shopflow) | Each is a single hard-coded image with a clear identity — set alt to the case name (these are currently hard-coded into the hero; if they become CMS-driven later, source from `Case.Title`). |
| `Sections/Home/HeroSection.razor:104` | Partner logo strip — `@($"images/partners/partner-{i}.svg")` | Currently hard-coded indices; T32 either binds to the eventually-CMS-driven `Partner.Name` or — if the strip stays a static decorative band — switches to `aria-hidden="true"`. Decision in T32. |

### 2d. Out of scope (separate codepath)

- Admin previews via `RSD.Web/Components/Admin/Shared/ResponsiveImage.razor`. Component accepts `Alt` — already correct API; admin call sites just pass the filename today. Improvable but not covered by this audit (it's admin-only, not public-facing).
- Inline images authored through the Quill rich-body editor — uploaded via a separate flow (not `<InputText>` paths). The block-editor image flow already exists; admin can already upload there.

---

## Decisions baked in

- **Path stays a string column.** The control swap in T31 doesn't change schemas — entities keep `XxxPath` as a `string`. The new `ImageUploader` writes to disk and returns the path; the editor binds it to the existing field.
- **Alt is hybrid.** Auto-derived (`Name` / `AuthorName` / `Label` / `Title`) where it's natural. Explicit alt column **only** for the four cover images and the SEO `OgImage`, where the visual content can't be reliably summarised by the existing fields.
- **No new storage backend.** Filesystem under `wwwroot/uploads/{subfolder}/{yyyy}/{MM}/` — same as today.

## Phase 9+ follow-ups (deferred)

- Re-hydrate the existing image preview when re-opening an editor (BlogEdit etc. don't currently look up the `UploadedFile` by path).
- Lightweight in-app media library (browse / reuse previously uploaded images).
- Migrate file storage to a blob store (S3 / Azure Blob).
- Audit the admin-only `ResponsiveImage.razor` callers — make the `Alt` parameter required and source it sensibly per call site.
