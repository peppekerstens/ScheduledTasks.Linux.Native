---
name: SHOULD — No XML documentation on public types
labels: [enhancement, SHOULD]
---

## Rule violated
- **Rule number:** Rule 18
- **Rule name:** Public output types must have XML documentation comments

## Location
- **File:** `src/ScheduledTasks.Linux.Native/Models/ScheduledTaskModels.cs`

## What's wrong
`RegisteredTask`, `TaskInfo`, `TaskAction`, `TaskTrigger`, `TaskPrincipal`, `TaskSettings` and their properties have no `<summary>` XML docs.

## How to fix
Add `<summary>` XML docs to all public types and properties.

## Severity
- [ ] SHOULD — should be fixed before merge
