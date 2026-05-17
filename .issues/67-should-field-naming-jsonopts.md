---
name: SHOULD — Field naming: JsonOpts should be s_jsonOpts
labels: [enhancement, SHOULD]
---

## Rule violated
- **Rule number:** Rule 19
- **Rule name:** Field naming — s_ for static, _ for instance

## Location
- **File:** `src/ScheduledTasks.Linux.Native/Helpers/SystemdHelpers.cs`, line 309

## What's wrong
`JsonOpts` is a static readonly field but lacks the `s_` prefix. Rule 19 requires: "s_ for static fields."

```csharp
private static readonly JsonSerializerOptions JsonOpts = new() { ... };  // ❌ should be s_jsonOpts
```

## How to fix
Rename to `s_jsonOpts`.

## Severity
- [ ] SHOULD — should be fixed before merge
