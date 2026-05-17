---
name: SHOULD — Parameters lack Position, ValidateNotNullOrEmpty, and Alias
labels: [enhancement, SHOULD]
---

## Rule violated
- **Rule number:** Rule 14
- **Rule name:** Parameter attributes must include Position, ValidateNotNullOrEmpty, and Alias

## Location
- **Files:** All cmdlets in `src/ScheduledTasks.Linux.Native/Commands/`

## What's wrong
Name/TaskName parameters lack `Position = 0`, `[ValidateNotNullOrEmpty]`, and `[Alias()]`.

## How to fix
Add all three attributes to primary name parameters on all cmdlets.

## Severity
- [ ] SHOULD — should be fixed before merge
