# SOLID, Validation & Sanitization Assessment

## SOLID Principles

### Where We Follow SOLID

- **DIP (Dependency Inversion)**  
  - API and Application layers depend on interfaces (`ISubscriberService`, `ILookupService`, `IEmailService`, etc.).  
  - Services and repositories are injected via DI; no direct `new` of concrete types in page models or controllers.

- **SRP (Single Responsibility)**  
  - Domain: entities and repository interfaces.  
  - Application: DTOs, service interfaces, and service implementations.  
  - Infrastructure: repositories and DbContext.  
  - API: controllers and Razor page models orchestrate use cases and delegate to services.

- **OCP / LSP**  
  - `BasePaginatedPageModel` and `BaseFilteredPageModel` exist for extension.  
  - Not all listing pages inherit them yet; Subscribers/Admins/Newsletters use their own pagination. Consider inheriting for consistency.

- **ISP**  
  - Interfaces are focused (e.g. `ILookupService`, `ISubscriberService`).  
  - No “god” interfaces.

### Gaps

- **Deleted/Index**  
  - Page model uses `NewsletterDbContext` directly instead of a service.  
  - **Recommendation:** Introduce something like `IDeletedItemsService` (or extend an existing service) and move restore/replace/delete logic there so the page only calls the service (better DIP and testability).

- **Razor reuse**  
  - Shared `_Alerts` partial is in place and used via layout.  
  - Pagination and filter bars are still duplicated across Subscribers, Newsletters, Admins, etc.  
  - **Recommendation:** Add `_Pagination.cshtml` and optionally `_FilterBar.cshtml` and use them from listing pages to reduce duplication and keep behavior consistent.

---

## Input Validation

### Backend (API & Razor)

- **DTOs**  
  - `CreateSubscriberDto` / `UpdateSubscriberDto`: `[Required]`, `[StringLength]`, `[EmailAddress]`, `[MinLength]` on lists.  
  - `CreateLookupDto` / `UpdateLookupDto`: `[Required]` on key fields.  
  - `UnsubscribeDto`: `[Required]`, `[EmailAddress]`.  
  - Admin Create: `[Required]`, `[EmailAddress]`, `[MinLength(6)]` for password.

- **Usage**  
  - Controllers: `if (!ModelState.IsValid) return BadRequest(ModelState)`.  
  - Razor handlers: `if (!ModelState.IsValid) return Page()` (or redirect with error).

- **Gaps**  
  - Some Razor POST handlers do minimal validation (e.g. only check one field).  
  - **Recommendation:** Prefer binding to a model with data annotations and use `ModelState.IsValid` on every POST so all rules run.

### Frontend

- **NewsletterForm**  
  - Client-side checks for required fields, email format, and at least one option for communication methods and interests.

- **Recommendation**  
  - Keep treating client-side validation as UX only; server remains the source of truth.

---

## Sanitization & XSS

- **Razor output**  
  - By default `@variable` is HTML-encoded.  
  - **Metadata/Index:** Value and Label from the database are no longer rendered with `Html.Raw`; they are rendered as plain text (`@cell.Value` when `IsHtml` is false), so they are encoded and safe.

- **Where Html.Raw is still used**  
  - Only for server-built markup (e.g. status badges “ACTIVE” / “DISABLED”) that does not include user input.  
  - **Rule:** Do not put user- or DB-sourced content into strings rendered with `Html.Raw`; use plain `@` output so it is encoded.

- **React**  
  - React escapes content by default; no `dangerouslySetInnerHTML` with user input in this app.

- **APIs**  
  - Input is validated via DTOs; persistence uses EF (parameterized), so SQL injection is mitigated.  
  - No custom HTML sanitization library is used; relying on encoding on output is sufficient for the current usage.

---

## Summary

| Area              | Status | Notes |
|-------------------|--------|--------|
| SOLID (DIP, SRP)  | Good   | Services and DI used; Deleted page still uses DbContext directly. |
| SOLID (reuse)     | Improved | `_Alerts` partial in layout; more partials (e.g. pagination) would help. |
| Validation (API)  | Good   | DTOs + ModelState used in controllers. |
| Validation (Razor)| OK     | Some handlers could lean more on model binding + ModelState. |
| XSS (Razor)       | Good   | User-derived values (e.g. Metadata Value/Label) no longer in Html.Raw. |
| XSS (React)       | Good   | Default escaping; no unsafe HTML from user input. |
