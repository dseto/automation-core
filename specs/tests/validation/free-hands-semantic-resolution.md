## Gate: Confidence Semantics

- Resolved requires confidence == 1.0
- Partial requires candidates.length > 0
- Unresolved requires candidates.length == 0

## Gate: Navigation Route Mapping Determinism (RF39)

Input:
- draft.feature containing: `Dado que estou na página "<route>"`

Expected:
- If UiMap contains a page with matching `__meta.route` (per deterministic algorithm), the resolver must:
  - set chosen.pageKey
  - set confidence = 1.0
  - mark status = resolved
- If no match exists, the resolver must:
  - emit finding code `UIGAP_ROUTE_NOT_MAPPED` with severity `info`
  - not choose any page

Additional rule:
- When a `RecorderSession` is provided, the resolver MUST prefer the `route` present in the session event (mapping.eventIndex) over the draft literal; no guessing or inference should be performed.

Signals of failure:
- Resolver “fixes” a route (e.g., strips `.html` or concatenates segments) without an exact match.
- Missing `UIGAP_ROUTE_NOT_MAPPED` when unmapped.


## V-SR-40
- Literal values unchanged.
