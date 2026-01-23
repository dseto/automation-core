# Validation

## V-SR-40
Resolved fill steps MUST preserve literal values verbatim.
## V-RN-01
- When a recorded navigation event's `url` equals the configured `BASE_URL`, the resolver must record `route: "/"` (root). This prevents the base path from being emitted as a navigation route in the session and breaking downstream draft/generation steps.