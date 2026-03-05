# Feature 836: Enable CA1502 and CA1506 via .editorconfig

## Status: [DRAFT]

## Type: infra

## Background

CA1502 (cyclomatic complexity) and CA1506 (class coupling) are Microsoft.CodeAnalysis.NetAnalyzers rules that exist in the built-in analyzer set but are set to `severity=none` at `AnalysisLevel=latest-recommended` (`Directory.Build.props:4`). They do not require the Roslynator.Analyzers package. Enabling them requires explicit `.editorconfig` overrides (e.g., `dotnet_diagnostic.CA1502.severity = suggestion`) and a staged remediation plan given `TreatWarningsAsErrors=true` in `Directory.Build.props:3`. This feature was created as a Mandatory Handoff from F831 (Roslynator Analyzers Investigation, Task 5).
