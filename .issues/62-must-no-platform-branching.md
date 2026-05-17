---
name: MUST — No OperatingSystem.IsWindows() guard on any cmdlet
labels: [bug, MUST]
---

## Rule violated
- **Rule number:** Rule 8
- **Rule name:** Platform branching at the top of ProcessRecord()

## Location
- **Files:** All 15 cmdlets in `src/ScheduledTasks.Linux.Native/Commands/`

## What's wrong
No cmdlet has `OperatingSystem.IsWindows()` branching. Windows has a built-in `ScheduledTasks` module — without branching, importing this module on Windows could cause conflicts.

## How to fix
Add at the top of each `ProcessRecord()`:
```csharp
if (OperatingSystem.IsWindows())
{
    string cmdletName = MyInvocation.MyCommand.Name;
    InvokeCommand.InvokeScript($"Microsoft.PowerShell.Management\\{cmdletName}");
    return;
}
```

## Severity
- [x] MUST — blocks merge
