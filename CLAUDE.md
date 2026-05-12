# Claude Code – Project Rules for RSD

These rules apply to **all code** in this repository and override Claude's defaults.
Hardware-specific exceptions are noted explicitly.

---

## 0. Azure DevOps Integration

### Scope
- ONLY operate in the `rsd site` project under `remsoftdev` org
- NEVER list, read, or modify work items in any other project
- Always confirm the target project before creating or updating work items

### Work Item Hierarchy
Follow this strict hierarchy when creating items:
1. **Epic** → high-level business goal or feature area
2. **Feature** → a deliverable chunk under an Epic
3. **User Story** → user-facing behavior under a Feature (format: "As a [role], I want [goal], so that [benefit]")
4. **Task** → implementation step under a User Story (include estimated hours when possible)
5. **Bug** → defect linked to the relevant User Story or Feature

Always set parent-child links when creating items. Never create orphan work items.

### Creating Work Items from Figma Designs
When decomposing Figma designs into work items:
1. First review the Figma file to identify all screens/sections
2. Propose the epic → feature → story → task breakdown to me BEFORE pushing to ADO
3. Wait for my approval
4. Only then create the items in ADO
5. Include acceptance criteria on every User Story derived from what's visible in the design

### Naming Conventions
- Epics: `[Area] - Short Description` (e.g., `Homepage - Redesign`)
- Features: descriptive noun phrase (e.g., `Hero Section`, `Navigation Bar`)
- User Stories: `As a...` format
- Tasks: verb-first (e.g., `Implement hero CTA button`, `Add responsive breakpoints`)

### Safety
- NEVER bulk-delete work items
- NEVER change work item state to "Removed" or "Closed" without my explicit approval
- Before updating an existing item, show me the current vs proposed changes

## 1. Language & Runtime Versions

Always use the **newest available syntax and APIs**. No exceptions.

### .NET 9 / C#
- Primary constructors for DI: `public partial class Foo(IService Svc)`
- Collection expressions: `[]`, `[..a, ..b]`
- `await cts.CancelAsync()` – never `cts.Cancel()` in async contexts
- `PeriodicTimer` instead of `System.Threading.Timer` where possible
- `required` keyword where applicable
- `record` / `record class` for all data containers (see §3)

### JavaScript
- ES modules (`import`/`export`) – no CommonJS
- `AbortController` for cancellation
- `IntersectionObserver`, `ResizeObserver`, `MutationObserver` over polling
- `structuredClone` instead of `JSON.parse(JSON.stringify(...))`
- Private class fields (`#field`)

### HTML / Razor
- Semantic HTML5: `<article>`, `<header>`, `<nav>`, `<aside>`, `<footer>`, `<time>`, `<dl>/<dt>/<dd>`, `<form>`
- Full WAI-ARIA: `role`, `aria-label`, `aria-expanded`, `aria-pressed`, `aria-selected`, `aria-current`, `aria-live`, `aria-hidden`
- ARIA boolean values as lowercase strings in Blazor: `"@(flag ? "true" : "false")"`
- `type="button"` on every non-submit button
- `<time datetime="...">` for all date/time output
- `scope="col"` on all `<th>`; `<caption class="sr-only">` on tables

---

## 2. Razor Components

- **Split every component**: `.razor` = markup only, `.razor.cs` = logic only
- No `@code` blocks in `.razor` files
- No `@inject` / `@implements` / `@inherits` in `.razor` files (exception: `@inherits LayoutComponentBase` must stay in `.razor`)
- Every `.razor.cs` starts with `#pragma warning disable S1144, S4487, S2933`
- **Primary constructors for injection** – never `[Inject]` properties:
  ```csharp
  // correct
  public partial class MyComponent(IMyService MyService) { }

  // wrong
  [Inject] private IMyService MyService { get; set; } = default!;
  ```

---

## 3. Records for DTOs and Value Objects

Use `record` (immutable) or `record class` (mutable) for **all data containers**.
Use `class` only for stateful services, components, and types with complex inheritance.

```csharp
// Immutable DTO – positional record
public record SensorReadingViewModel(
    string Label  = "",
    string Value  = "–",
    string Unit   = "",
    string Status = "unknown");

// Mutable form/config model – record class
public record class LocationSettings
{
    public string Name { get; set; } = "";
    // ...
}

// Stateful service – stays as class
public class DashboardStateService { }
```

---

## 4. No Primitive Obsession

Avoid raw `string` / `int` for business concepts that carry domain meaning.
Wrap them in typed Value Objects or records with validation.

```csharp
// wrong
void Process(string email) { }

// correct
public record Email(string Value)
{
    public Email(string value) : this(value)
    {
        if (!value.Contains('@')) throw new ArgumentException("Invalid email");
    }
}
void Process(Email email) { }
```

Exception: IDs used directly in HTTP / JSON / JS interop contexts may stay as `string`
for serialization compatibility, but should not leak across service boundaries without a type.

---

## 5. No Null in Business Logic

| Situation | Wrong | Correct |
|---|---|---|
| Method returns "nothing" | `return null;` | `return [];` / `return string.Empty;` / empty object |
| Field before first load | `string? _error;` | `string _error = string.Empty;` |
| Filtering a projection | `.Where(x => x != null).Select(x => x!)` | `TryMapValue()` returning `IEnumerable<T>` (empty = skip) via `SelectMany` |
| Nullable accumulator | `List<T>? _items;` + loading flag mixed | `List<T> _items = [];` + `bool _loading = true;` |

Allowed nullable (`?`) locations:
- `IJSObjectReference?`, `DotNetObjectReference<T>?` – JS interop lifetime
- `CancellationTokenSource?` – optional debounce/cancel tokens
- `RenderFragment?` – optional slot content in components

---

## 6. Pure Functions

Extract logic into `private static` methods wherever possible.

```csharp
// wrong – reads no fields, has no side effects, but not static
private string FormatAge(TimeSpan ago) => ago.TotalSeconds < 60 ? "just now" : "...";

// correct
private static string FormatAge(TimeSpan ago) => ago.TotalSeconds < 60 ? "just now" : "...";
```

Blazor lifecycle methods (`OnInitialized`, `OnParametersSet`, etc.) are exempt.

---

## 7. Cyclomatic Complexity ≤ 4

No method may exceed a cyclomatic complexity of 4.
Extract sub-methods, use switch expressions, or restructure to stay within the limit.
