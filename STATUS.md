# ScheduledTasks.Linux.Native — Module Status

**Last updated:** 2026-05-16
**Version:** 1.0.0
**GHA Build:** ✅ green
**GHA Pester:** ✅ green (5-distro + Windows)

---

## Current State

13 cmdlets + 2 stubs implemented via `systemctl` subprocess and file I/O. All write cmdlets fall back to user-context timers (`~/.config/systemd/user/`) for non-root users.

### Output Types

| Type | Inherits | Windows Counterpart | Rule 9 Status |
|---|---|---|---|
| `RegisteredTask` | `object` | `IRegisteredTask` (COM) | ✅ Compliant (property names match) |
| `TaskInfo` | `object` | `ITask` (COM) | ✅ Compliant (property names match) |
| `TaskAction` | `object` | `IExecAction` (COM) | ✅ Compliant |
| `TaskTrigger` | `object` | `ITrigger` (COM) | ✅ Compliant |
| `TaskPrincipal` | `object` | `IPrincipal` (COM) | ✅ Compliant |
| `TaskSettings` | `object` | `ITaskSettings` (COM) | ✅ Compliant |

### Rule 9 Compliance

Windows ScheduledTasks returns COM interop objects — no .NET class hierarchy to inherit from. Linux POCOs already match property names and types closely.

**Fixed (2026-05-16, commit `030313e`):**
- `TaskPrincipal.RunLevel` changed from `string` to `TaskRunLevel` enum
- `TaskTrigger.TriggerType` changed from `string` to `TaskTriggerType` enum
- Updated cmdlet parameters (`New-ScheduledTaskPrincipal -RunLevel`, `New-ScheduledTaskTrigger`) to use enum types
- Updated `IsSystemContext()` helper to compare against `TaskRunLevel.Highest`
- Updated Pester test assertion for default RunLevel (`'LeastPrivilege'` instead of `'Limited'`)

---

## Known Issues

| Issue | Severity | Status |
|---|---|---|
| Non-root users can manage user-context timers without elevation error | ℹ️ By design | `IsSystemContext()` returns false for non-root, uses `--user` flag |

## Next Steps

No critical Rule 9 gaps — fully compliant.

---

## Reference

| Resource | Location |
|---|---|
| Source code | `src/ScheduledTasks.Linux.Native/` |
| Tests | `tests/ScheduledTasks.Linux.Native.Tests/` |
| Linux rules | `docs/linux-rules.md` |
| Coordination repo | `https://github.com/peppekerstens/opencode` |
