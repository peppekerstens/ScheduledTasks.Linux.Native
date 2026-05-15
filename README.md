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

## Manual Testing

For a detailed, step-by-step guide on setting up your environment and testing these modules, see the blog post: [Testing the native layer](https://peppekerstens.github.io/blog/testing-the-native-layer).

### Option 1: Interactive Container (Recommended)
Use the pre-built CI images to avoid dependency issues.

```powershell
# Start an interactive shell in the Ubuntu 24.04 test container
docker compose -f docker-compose.test.yml run ubuntu-24 pwsh
```
Once inside:
```powershell
Import-Module /module/bin/Release/net8.0/publish/ScheduledTasks.Linux.Native.dll
Get-ScheduledTask
```

### Option 2: Bare WSL
Test directly in your WSL distro (requires `.NET 8 SDK`).

```powershell
dotnet publish src/ScheduledTasks.Linux.Native --configuration Release --output bin/Release/net8.0/publish
pwsh
Import-Module ./bin/Release/net8.0/publish/ScheduledTasks.Linux.Native.dll
Get-ScheduledTask
```

---

## CI / Testing


| Distro | Image |
|---|---|
| Ubuntu 24.04 | `ghcr.io/peppekerstens/pwsh-pester-ubuntu:24.04` |
| Debian 12 | `ghcr.io/peppekerstens/pwsh-pester-debian:12` |
| Fedora 40 | `ghcr.io/peppekerstens/pwsh-pester-fedora:40` |
| openSUSE Tumbleweed | `ghcr.io/peppekerstens/pwsh-pester-opensuse:tumbleweed` |
| Arch Linux | `ghcr.io/peppekerstens/pwsh-pester-arch:latest` |

### Test scenarios

| Describe block | Scope | Tests |
|---|---|---|
| Module surface | everywhere | 15 cmdlet export checks |
| New-ScheduledTaskAction | everywhere | Execute, Arguments, WorkingDirectory, mandatory check |
| New-ScheduledTaskTrigger | everywhere | Once, Daily, Weekly (Mon/Wed/Fri), AtStartup, AtLogOn |
| New-ScheduledTaskPrincipal | everywhere | RunLevel default, Highest, invalid rejection |
| New-ScheduledTaskSettingsSet | everywhere | Enabled default, -Disable, RestartCount/Interval |
| New-ScheduledTask | everywhere | Combined Action + Description |
| WhatIf safety | everywhere | Register/Unregister/Enable/Disable/Start/Stop -WhatIf |
| Stub cmdlets | everywhere | Set-ScheduledTask, Export-ScheduledTask error records |
| Register-ScheduledTask / Get-ScheduledTask | Linux + root | Register, wildcard filter, Force overwrite, duplicate throws, unit files on disk, OnCalendar content |
| Enable/Disable-ScheduledTask | Linux + root | Enable/Disable return RegisteredTask |
| Unregister-ScheduledTask | Linux + root | Unit files removed, task gone from Get-ScheduledTask |
| Get-ScheduledTaskInfo | Linux + root | TaskName match, NextRunTime type |
| New-ScheduledTask pipeline | Linux + root | New-ScheduledTask \| Register-ScheduledTask |
| **Weekly trigger** | **Linux + root** | **Timer file contains Mon/Wed/Fri, Get-ScheduledTaskInfo NextRunTime** |
| **AtStartup trigger** | **Linux + root** | **Timer file contains boot, task visible** |
| **Pipeline disable** | **Linux + root** | **Get-ScheduledTask wildcard \| Disable/Enable-ScheduledTask** |
| **Start-ScheduledTask runs** | **Linux + root + systemd** | **Marker file created, Get-ScheduledTaskInfo LastRunTime** |
| Get-ScheduledTask read-only | Linux (any user) | Returns array or empty, nonexistent name returns empty |

Run locally (requires systemd):

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
| 0.2.0 | Test expansion. Weekly/AtStartup trigger integration tests; pipeline bulk disable/enable; `Start-ScheduledTask` marker-file run test; `Get-ScheduledTaskInfo` LastRunTime assertion; `$script:hasSystemd` guard for start-service tests. |
| 0.3.0 | GHA all-green. Fixed `$script:isLinux` collision → `$script:onLinux`; `fail-fast: false`; all 8 integration `Describe` blocks now guard on `$script:hasSystemd` (not just Start); `BeOfType` null quirk → `Should -Not -Throw`; openSUSE image now includes `gawk`+`findutils`. All 5 distros pass. |

---

## Related

- [`ScheduledTasks.Linux`](https://github.com/peppekerstens/ScheduledTasks.Linux) — the Stage 1 PowerShell script wrapper this module replaces
- [opencode project plan](https://github.com/peppekerstens/opencode) — multi-stage project tracking
- [Blog series](https://peppekerstens.github.io) — write-up of the full journey

---

## License

[GNU General Public License v3](LICENSE)
