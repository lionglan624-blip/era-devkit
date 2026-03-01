# CCS (Claude Code Switch) Reference

**Package**: `@kaitranntt/ccs`
**Version**: 7.31.1+
**Docs**: https://github.com/kaitranntt/ccs

## Overview

CCS is a CLI wrapper for Claude Code that provides:
- Multi-account profile switching
- OAuth provider integration (Gemini, Codex, etc.)
- API key model delegation (GLM, Kimi, Ollama)
- Default profile management

## Installation

```bash
npm install -g @kaitranntt/ccs
```

## Core Usage

```bash
ccs                     # Use default Claude account
ccs <profile>           # Use specific profile
ccs <profile> "prompt"  # Execute with prompt
```

## Account Management

```bash
ccs auth create <name>    # Create new account profile
ccs auth list             # List all profiles
ccs auth default <name>   # Set default profile
ccs auth reset-default    # Restore original default
```

## Profile Persistence

```bash
ccs persist <profile>        # Write profile env to ~/.claude/settings.json
ccs persist --list-backups   # List backups
ccs persist --restore        # Restore from backup
```

## Configuration Files

| Path | Purpose |
|------|---------|
| `~/.ccs/config.yaml` | Main configuration |
| `~/.ccs/profiles.json` | Account profiles |
| `~/.ccs/instances/` | Profile instances |
| `~/.ccs/*.settings.json` | Provider settings |

## Dashboard Integration

When spawning Claude from Feature Dashboard:
- Use `ccs` instead of `claude` to apply default profile automatically
- Arguments pass through to Claude CLI
- Example: `ccs -p "/run 123" --output-format stream-json`

## Delegation (Inside Claude Code)

```
/ccs "task"                 # Auto-select profile
/ccs --glm "task"           # Force GLM-4.6
/ccs --kimi "task"          # Force Kimi
/ccs:continue "follow-up"   # Continue last session
```

## OAuth Providers

| Command | Provider |
|---------|----------|
| `ccs gemini` | Google Gemini |
| `ccs codex` | OpenAI Codex |
| `ccs agy` | Antigravity |
| `ccs qwen` | Qwen Code |
| `ccs kiro` | Kiro (AWS) |
| `ccs ghcp` | GitHub Copilot |

## API Key Models

| Command | Model |
|---------|-------|
| `ccs glm` | GLM 4.6 |
| `ccs glmt` | GLM with thinking |
| `ccs kimi` | Kimi for Coding |
| `ccs ollama` | Local Ollama |

## Diagnostics

```bash
ccs doctor    # Health check
ccs config    # Web dashboard (localhost:3000)
ccs update    # Update to latest
```
