# ScheduledTasks.Linux.Native — Contributor Guide

## What this module is

A C# binary PowerShell module providing Linux `*-ScheduledTask` cmdlets via `systemctl` subprocess and file I/O. 13 cmdlets + 2 stubs. Maps systemd timers to Windows Task Scheduler concepts. Designed as a drop-in replacement for Windows `ScheduledTasks` on Linux.

Part of the [PowerShell Linux Commands](https://github.com/peppekerstens/opencode) project.

---

## Quick Start

```bash
# Build
dotnet build -c Release

# Run tests (requires pwsh)
pwsh -c "Import-Module ./src/ScheduledTasks.Linux.Native/bin/Release/net8.0/ScheduledTasks.Linux.Native.dll; Invoke-Pester ./tests/"
```

---

## Architecture

```
src/ScheduledTasks.Linux.Native/
├── Commands/          # Cmdlet implementations (13 + 2 stubs)
│   ├── GetScheduledTaskCommand.cs
│   ├── RegisterScheduledTaskCommand.cs
│   ├── UnregisterScheduledTaskCommand.cs
│   ├── EnableScheduledTaskCommand.cs
│   ├── DisableScheduledTaskCommand.cs
│   ├── StartScheduledTaskCommand.cs
│   ├── StopScheduledTaskCommand.cs
│   ├── ExportScheduledTaskCommand.cs
│   ├── NewScheduledTaskCommand.cs
│   ├── NewScheduledTaskActionCommand.cs
│   ├── NewScheduledTaskPrincipalCommand.cs
│   ├── NewScheduledTaskSettingsSetCommand.cs
│   ├── NewScheduledTaskTriggerCommand.cs
│   ├── SetScheduledTaskCommand.cs       # Stub
│   └── StopScheduledTaskCommand.cs      # Stub
├── Helpers/
│   └── SystemdHelpers.cs     # Timer unit parsing, context detection
└── Models/
    └── ScheduledTaskModels.cs    # RegisteredTask, TaskInfo, TaskAction, TaskTrigger, TaskPrincipal, TaskSettings
```

### Key design decisions

- **systemd timers** — System tasks via `systemctl` + `/etc/systemd/system/`. User tasks via `--user` + `~/.config/systemd/user/`.
- **Non-root fallback** — `IsSystemContext()` returns false for non-root, uses `--user` flag instead of elevation error.
- **Type alignment (Rule 9)** — `TaskRunLevel` and `TaskTriggerType` are enums (not strings). POCOs match Windows COM property names.
- **File I/O** — Timer `.timer` and `.service` unit files parsed directly for task metadata.

---

## C# Conventions

| Rule | Detail |
|---|---|
| **Target** | `net8.0`, `TreatWarningsAsErrors=true`, `Deterministic=true` |
| **SMA** | Pinned to `7.4.6` exactly |
| **Namespaces** | File-scoped (`namespace Foo;`) |
| **Process** | `ProcessStartInfo.ArgumentList` only, `ReadToEndAsync()` on stdout/stderr |
| **Cmdlets** | `SupportsShouldProcess` on write cmdlets only, stubs throw `NotImplementedException` |
| **Async** | `ConfigureAwait(false)` on all async methods |
| **Errors** | `ErrorRecord` with `UnauthorizedAccess` ID, `SecurityError` category |
| **Copyright** | `// Copyright (c) peppekerstens. All rights reserved.` |

Full rules: `docs/linux-rules.md`

### Version alignment
- **Single source of truth:** `<Version>` in `.csproj`
- **Must match:** `STATUS.md` `**Version:**` line, README.md version history table (latest entry)
- **Bump rule:** `.csproj` first, then `STATUS.md`, then README.md — in that order

---

## Testing

- **Framework:** Pester 5
- **Runner:** `pwsh -c "Invoke-Pester ./tests/"`
- **GHA:** 5-distro matrix + Windows
- **Test file:** `tests/ScheduledTasks.Linux.Native.Tests.ps1`

---

## Current State

See `STATUS.md` for module state, open issues, and next steps.

**Open issues:** 0 — fully compliant with all rules.

---

## Boundaries

### What lives in this repo
- Source code (`src/ScheduledTasks.Linux.Native/`)
- Pester tests (`tests/`)
- CI/CD (`.github/workflows/`)
- Module status (`STATUS.md`)
- Contributor guide (`AGENTS.md`)
- Development rules (`docs/linux-rules.md`)
- OpenCode config (`.opencode/`)

### What lives elsewhere
- Cross-repo planning, status aggregation, project plan → https://github.com/peppekerstens/opencode
- Other modules → https://github.com/peppekerstens/
- Blog posts → https://peppekerstens.github.io

### What to do when
| Scenario | Where |
|---|---|
| Bug in this module | File issue in **this repo** |
| Feature request for this module | File issue in **this repo** |
| Cross-module convention change | File issue in **opencode** |

---

## Coordination

This module is part of a larger project. Cross-repo planning lives at:
- **Coordination repo:** https://github.com/peppekerstens/opencode
- **Project plan:** https://github.com/peppekerstens/opencode/blob/main/plan.md
