---
name: SHOULD — LINQ used in ListTasks filtering
labels: [enhancement, SHOULD]
---

## Rule violated
- **Rule number:** Rule 17
- **Rule name:** Avoid LINQ and params arrays in performance-sensitive code

## Location
- **File:** `src/ScheduledTasks.Linux.Native/Helpers/SystemdHelpers.cs`, lines 124-130

## What's wrong
`ListTasks()` uses `.Where()` and `.Any()` LINQ for filtering. Rule 17 requires foreach loops.

## How to fix
Replace LINQ with foreach loops for filtering.

## Severity
- [ ] SHOULD — should be fixed before merge
