## Confidence Calculation (Normative)

- Exact UIMap key match → confidence = 1.0 → resolved
- Unique testId match → confidence = 1.0 → resolved
- Multiple candidates → confidence = 1 / nCandidates → partial (always)
- Zero candidates → confidence = 0.0 → unresolved

- Navigation steps: when a draft step is a navigation (e.g., `Dado que estou na página "<route>"`), the resolver attempts to map the route to a UiMap page in two ways:
  - By page key (page name)
  - By page `__meta.route` value
  If the route maps to a page, the resolver marks the step as **resolved** at page-level (chosen.pageKey set, special element key `__page__` is used to indicate page-level resolution). If the route is not found in the UiMap, the resolver emits an **info** finding with code `UIGAP_ROUTE_NOT_MAPPED` (non-fatal visibility).

Policy: the UiMap is expected to include a root route mapping for `/` (i.e., at least one page with `__meta.route = '/'`). This repository enforces this via the `UiMapValidator` which will fail validation if `/` is missing.

Thresholds do not reclassify partial when candidates exist.

## Navigation Route Mapping — Deterministic Policy (Normative)

This section refines the navigation mapping rules to remove ambiguity.

### Inputs
- Draft navigation step: `Dado que estou na página "<route>"`
- `<route>` is expected to originate from `event.route` recorded in `session.json`.

### Deterministic algorithm (mandatory order)
1) Exact match by **page key** (page name)
   - If `<route>` equals a UiMap `pageKey`, resolve to that page.

2) Exact match by UiMap `__meta.route` using fragment
   - If `<route>` contains `#`, extract the fragment (including `#`, e.g., `#/dashboard`).
   - Derive `routeKey`:
     - If fragment starts with `#/`, then `routeKey = fragment[1..]` (drop the leading `#`) → `/dashboard`.
     - Else, `routeKey = fragment` without the leading `#`.
   - If any page has `__meta.route == routeKey` (exact string equality), resolve to that page.

3) Exact match by UiMap `__meta.route` using full route
   - If step 2 did not resolve, attempt exact equality: `__meta.route == <route>`.

### No-guessing rule
- The resolver MUST NOT normalize or guess (e.g., strip `.html`, remove prefixes, join segments).

### Not found behavior
- When no mapping occurs, emit finding:
  - code: `UIGAP_ROUTE_NOT_MAPPED`
  - severity: `info`
  - The step remains unresolved at page-level.