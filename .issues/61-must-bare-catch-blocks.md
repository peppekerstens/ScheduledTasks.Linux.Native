---
name: MUST — Bare catch blocks swallow errors silently
labels: [bug, MUST]
---

## Rule violated
- **Rule number:** Rule 7
- **Rule name:** No silent error swallowing

## Location
- **File:** `src/ScheduledTasks.Linux.Native/Helpers/SystemdHelpers.cs`, lines 63-64, 79

## What's wrong
Bare `catch { /* swallow */ }` and `catch { return []; }` silently swallow errors. Rule 7 requires every catch to do one of: `throw`, `WriteError()`, `ThrowTerminatingError()`, or have a justified comment.

```csharp
try { timers = JsonSerializer.Deserialize<...>(timersJson, JsonOpts); }
catch { return []; }  // ❌ bare catch, silent swallow

try { ... }
catch { /* swallow */ }  // ❌ bare catch, silent swallow
```

## How to fix
Replace with `WriteWarning()` or justified comments explaining why the error is non-critical.

## Severity
- [x] MUST — blocks merge
