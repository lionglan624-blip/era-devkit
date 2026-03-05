# Feature 837: Enable EnforceCodeStyleInBuild for IDE-prefix Rule Enforcement

## Status: [DRAFT]

## Type: infra

## Background

`EnforceCodeStyleInBuild` is absent from all three repos (`devkit`, `core`, `engine`) `Directory.Build.props` files. Without this property, IDE-prefix rules (including IDE0060: Remove unused parameter) are silently ignored during `dotnet build` regardless of `.editorconfig` severity settings. Enabling requires a staged approach — first scan the codebase at `suggestion` severity, remediate violations, then promote to `warning` — to avoid widespread build breaks under `TreatWarningsAsErrors=true`. This feature was created as a Mandatory Handoff from F831 (Roslynator Analyzers Investigation, Task 5).
