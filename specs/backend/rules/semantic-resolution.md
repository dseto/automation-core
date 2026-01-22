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
