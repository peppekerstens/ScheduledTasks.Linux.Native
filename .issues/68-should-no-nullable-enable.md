---
name: SHOULD — No #nullable enable on source files
labels: [enhancement, SHOULD]
---

## Rule violated
- **Rule number:** Rule 22
- **Rule name:** Nullable annotations must be consistent across all files

## Location
- **Files:** All `.cs` files in `src/ScheduledTasks.Linux.Native/`

## What's wrong
No source file has `#nullable enable`.

## How to fix
Add `#nullable enable` at the top of each `.cs` file.

## Severity
- [ ] SHOULD — should be fixed before merge
