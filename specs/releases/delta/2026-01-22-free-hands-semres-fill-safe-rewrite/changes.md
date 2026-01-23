# Changes

## Summary
Corrects unsafe semantic rewriting that replaced literal fill values.

## Normative Changes
- RF-SR-40: Semantic Resolver MUST preserve literal fill values.
- RF-RN-01: Recorder/Route Normalizer MUST map an initial navigation to root when the recorded URL is exactly the configured `BASE_URL` (i.e., emit `route: "/"`). This prevents propagation of the application base path into recorded `route` values and avoids downstream Gherkin generation errors.
