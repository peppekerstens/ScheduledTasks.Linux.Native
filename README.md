# ScheduledTasks.Linux.Native

[![Pester Tests](https://github.com/peppekerstens/ScheduledTasks.Linux.Native/actions/workflows/pester.yml/badge.svg)](https://github.com/peppekerstens/ScheduledTasks.Linux.Native/actions/workflows/pester.yml)

> Native C# binary module implementing Windows-compatible `ScheduledTasks` cmdlets for Linux via systemd timer units.

This is the Tier 2 (C# native) successor to [`ScheduledTasks.Linux`](https://github.com/peppekerstens/ScheduledTasks.Linux), part of Stage 5 of the [PowerShell Linux Commands](https://peppekerstens.github.io) project.

---

## What it does

Maps the Windows `ScheduledTasks` cmdlet surface to systemd `.service` + `.timer` unit files. `Register-ScheduledTask` writes the unit files and enables the timer. `Get-ScheduledTask` and `Get-ScheduledTaskInfo` read live state from `systemctl`. Factory cmdlets (`New-ScheduledTaskAction`, `New-ScheduledTaskTrigger`, etc.) create in-memory objects that are pipeline-compatible with `Register-ScheduledTask`, matching the Windows workflow.

| Cmdlet | Status | Notes |
|---|---|---|
| `Get-ScheduledTask` | Full | Reads system + user timers via `systemctl list-timers` |
| `Get-ScheduledTaskInfo` | Full | Last/next run times from `systemctl show` |
| `New-ScheduledTaskAction` | Full | In-memory factory; maps to `ExecStart=` in the `.service` unit |
| `New-ScheduledTaskTrigger` | Full | Once / Daily / Weekly / AtStartup / AtLogOn → `OnCalendar=` expression |
| `New-ScheduledTaskPrincipal` | Full | In-memory factory; maps to `User=` in the `.service` unit |
| `New-ScheduledTaskSettingsSet` | Full | In-memory factory |
| `New-ScheduledTask` | Full | In-memory task object; pipeline-compatible with `Register-ScheduledTask` |
| `Register-ScheduledTask` | Full | Writes `.service` + `.timer`; calls `systemctl enable --now` |
| `Unregister-ScheduledTask` | Full | `systemctl disable --now`; removes unit files |
| `Enable-ScheduledTask` | Full | `systemctl enable --now` |
| `Disable-ScheduledTask` | Full | `systemctl stop` + `systemctl disable` |
| `Start-ScheduledTask` | Full | `systemctl start <name>.service` |
| `Stop-ScheduledTask` | Full | `systemctl stop <name>.service` |
| `Set-ScheduledTask` | Stub | Writes `NotSupportedException`; use `Unregister-ScheduledTask` + `Register-ScheduledTask` |
| `Export-ScheduledTask` | Stub | Writes `NotSupportedException` |

All write cmdlets support `-WhatIf` and `-Confirm`.

---

## Requirements

- Linux with systemd
- PowerShell 7.4+, .NET 8
- `systemctl` on PATH
- Root for system-wide tasks (`/etc/systemd/system`); non-root for user timers (`~/.config/systemd/user`)

---

## Installation

```powershell
git clone https://github.com/peppekerstens/ScheduledTasks.Linux.Native
dotnet build ScheduledTasks.Linux.Native/src/ScheduledTasks.Linux.Native --configuration Release
Import-Module ./ScheduledTasks.Linux.Native/src/ScheduledTasks.Linux.Native/bin/Release/net8.0/ScheduledTasks.Linux.Native.dll
```

---

## Usage

```powershell
# Register a daily backup task
$action  = New-ScheduledTaskAction -Execute '/usr/bin/pwsh' -Argument '-File /opt/scripts/backup.ps1'
$trigger = New-ScheduledTaskTrigger -Daily -At '02:00'
Register-ScheduledTask -TaskName 'NightlyBackup' -Action $action -Trigger $trigger -Description 'Nightly backup'

# List all tasks
Get-ScheduledTask

# Get timing info for a specific task
Get-ScheduledTaskInfo -TaskName 'NightlyBackup'

# Run a task immediately
Start-ScheduledTask -TaskName 'NightlyBackup'

# Disable without removing
Disable-ScheduledTask -TaskName 'NightlyBackup'

# Remove permanently
Unregister-ScheduledTask -TaskName 'NightlyBackup' -Confirm:$false
```

---

## CI / Testing

Tested across 5 Linux distributions in containers on every push:

| Distro | Image |
|---|---|
| Ubuntu 24.04 | `ghcr.io/peppekerstens/pwsh-pester-ubuntu:24.04` |
| Debian 12 | `ghcr.io/peppekerstens/pwsh-pester-debian:12` |
| Fedora 40 | `ghcr.io/peppekerstens/pwsh-pester-fedora:40` |
| openSUSE Tumbleweed | `ghcr.io/peppekerstens/pwsh-pester-opensuse:tumbleweed` |
| Arch Linux | `ghcr.io/peppekerstens/pwsh-pester-arch:latest` |

Run locally (requires Docker or a Linux system with systemd):

```powershell
Invoke-Pester -Path tests/ScheduledTasks.Linux.Native.Tests/ -Output Detailed
```

---

## Implementation Notes

- **No P/Invoke for systemd**: systemd has no stable C API. `systemctl` is the correct interface; `Process.Start(systemctl)` is intentional for write paths. The only P/Invoke is `getuid()` to determine whether to target system or user scope.
- **Unit file location**: system-scope tasks go to `/etc/systemd/system/`; user-scope (non-root) go to `~/.config/systemd/user/`. The module detects this automatically.
- **`RegisteredTask` is pipeline-compatible**: `New-ScheduledTask` returns a `RegisteredTask` object that `Register-ScheduledTask` accepts directly via the pipeline, matching Windows behaviour.
- **Trigger mapping**: `OnCalendar=` expressions cover Once, Daily, Weekly, AtStartup (`OnActiveSec=`), and AtLogOn. There is no direct systemd equivalent for AtLogOn — the module approximates it.
- **`Set-ScheduledTask` is a stub**: modifying an existing timer in-place requires reading, merging, and rewriting unit files. Use `Unregister-ScheduledTask` + `Register-ScheduledTask` instead.
- **`ConfirmImpact.High`** on all destructive cmdlets, matching Windows built-in behaviour.

---

## Version history

| Version | Changes |
|---|---|
| 0.1.0 | Initial release. 13 full cmdlets, 2 stubs. 63 Pester tests. `getuid()` P/Invoke replaces `id -u` subprocess. |

---

## Related

- [`ScheduledTasks.Linux`](https://github.com/peppekerstens/ScheduledTasks.Linux) — the Stage 1 PowerShell script wrapper this module replaces
- [opencode project plan](https://github.com/peppekerstens/opencode) — multi-stage project tracking
- [Blog series](https://peppekerstens.github.io) — write-up of the full journey

---

## License

[GNU General Public License v3](LICENSE)
