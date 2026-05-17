---
name: MUST — No proactive elevation check on write cmdlets
labels: [bug, MUST]
---

## Rule violated
- **Rule number:** Rule 1
- **Rule name:** Elevation checks are mandatory for write operations

## Location
- **Files:** All write cmdlets in `src/ScheduledTasks.Linux.Native/Commands/` (Register, Unregister, Enable, Disable, Start, Stop, Export)

## What's wrong
Write cmdlets don't check elevation before acting. System-context timer operations require root but no check is performed.

## How to fix
Add `Utils.IsAdministrator()` check at the start of each write cmdlet's `ProcessRecord()` method.

## Severity
- [x] MUST — blocks merge
