# Feature 841: Build CWD Cross-Repo Resolution in ac-static-verifier

## Status: [DRAFT]

## Type: infra

## Background

### Problem (Current Issue)
`verify_build_ac` in `ac-static-verifier.py` uses `self.repo_root` for the working directory when executing build commands. This is independent of `_expand_glob_path` (which was fixed in F838 for file/code path resolution). When a build-type AC targets an engine or core project, the build command runs from the devkit root instead of the correct repo root. Currently no cross-repo features use build-type ACs, so this is a latent issue.

### Goal (What to Achieve)
Add cross-repo CWD resolution to `verify_build_ac` so that build commands for engine/core/game/dashboard projects execute from the correct repo root directory. Reuse the `_CROSS_REPO_PREFIX_MAP` mapping added in F838.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F838 | [DONE] | Provides `_CROSS_REPO_PREFIX_MAP` used for CWD resolution |

## Links

[Predecessor: F838](feature-838.md) - Cross-repo prefix mapping (prerequisite)
