# ScheduledTasks.Linux.Native

PowerShell C# binary module providing Windows-compatible `ScheduledTasks` cmdlets for Linux via systemd timer units.

## Cmdlets

| Cmdlet | Status | Notes |
|---|---|---|
| `Get-ScheduledTask` | ✅ Full | Reads system + user timers via `systemctl list-timers --output=json` |
| `Get-ScheduledTaskInfo` | ✅ Full | LastTriggerUSec / NextElapseUSecRealtime from `systemctl show` |
| `New-ScheduledTaskAction` | ✅ Full | In-memory factory |
| `New-ScheduledTaskTrigger` | ✅ Full | Once / Daily / Weekly / AtStartup / AtLogOn → OnCalendar expression |
| `New-ScheduledTaskPrincipal` | ✅ Full | In-memory factory |
| `New-ScheduledTaskSettingsSet` | ✅ Full | In-memory factory |
| `New-ScheduledTask` | ✅ Full | In-memory task object |
| `Register-ScheduledTask` | ✅ Full | Writes .service + .timer; enables unit |
| `Unregister-ScheduledTask` | ✅ Full | Disables + removes unit files |
| `Enable-ScheduledTask` | ✅ Full | `systemctl enable + start` |
| `Disable-ScheduledTask` | ✅ Full | `systemctl stop + disable` |
| `Start-ScheduledTask` | ✅ Full | `systemctl start <name>.service` |
| `Stop-ScheduledTask` | ✅ Full | `systemctl stop <name>.service` |
| `Set-ScheduledTask` | ⚠️ Stub | Writes error; use Unregister + Register |
| `Export-ScheduledTask` | ⚠️ Stub | Writes error |

## Usage

```powershell
# Build
dotnet build src/ScheduledTasks.Linux.Native --configuration Release

# Import
Import-Module ./src/ScheduledTasks.Linux.Native/bin/Release/net8.0/ScheduledTasks.Linux.Native.dll

# Register a daily task
$action  = New-ScheduledTaskAction -Execute '/usr/bin/pwsh' -Argument '-File /opt/scripts/backup.ps1'
$trigger = New-ScheduledTaskTrigger -Daily -At '02:00'
Register-ScheduledTask -TaskName 'NightlyBackup' -Action $action -Trigger $trigger -Description 'Nightly backup'

# List tasks
Get-ScheduledTask

# Get timing info
Get-ScheduledTaskInfo -TaskName 'NightlyBackup'

# Remove
Unregister-ScheduledTask -TaskName 'NightlyBackup' -Confirm:$false
```

## Requirements

- Linux with systemd
- .NET 8 SDK (build) / Runtime (run)
- PowerShell 7.2+
- Root for system-wide tasks (`/etc/systemd/system`); non-root for user timers (`~/.config/systemd/user`)

## Stage

Part of the [opencode](https://github.com/peppekerstens/opencode) multi-stage project:
Tier 2 Priority 2 — C# binary module porting the Stage 1 PowerShell implementation.
