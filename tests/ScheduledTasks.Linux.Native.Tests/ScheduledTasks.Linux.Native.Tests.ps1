# ScheduledTasks.Linux.Native.Tests.ps1
# Pester 5 integration test suite for ScheduledTasks.Linux.Native.
# Must run on Linux as root (systemd available).
# Test timers are prefixed 'stn_test_' to avoid clashing with real units.

BeforeDiscovery {
    $script:isRoot = $IsLinux -and (
        [System.IO.File]::ReadAllText('/proc/self/status') -match '(?m)^Uid:\s+(\d+)' -and
        $Matches[1] -eq '0')
    $script:onLinux = $IsLinux
    $script:prefix = 'stn_test'
    $script:taskName = "$($script:prefix)_daily"
    # systemd is active only when PID 1 is systemd (not in most Docker containers)
    $script:hasSystemd = $IsLinux -and (Test-Path '/run/systemd/private')
}

# ---------------------------------------------------------------------------
# Module surface tests (run everywhere)
# ---------------------------------------------------------------------------
Describe 'Module surface' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'exports Get-ScheduledTask' {
        Get-Command Get-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Get-ScheduledTaskInfo' {
        Get-Command Get-ScheduledTaskInfo -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports New-ScheduledTaskAction' {
        Get-Command New-ScheduledTaskAction -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports New-ScheduledTaskTrigger' {
        Get-Command New-ScheduledTaskTrigger -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports New-ScheduledTaskPrincipal' {
        Get-Command New-ScheduledTaskPrincipal -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports New-ScheduledTaskSettingsSet' {
        Get-Command New-ScheduledTaskSettingsSet -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports New-ScheduledTask' {
        Get-Command New-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Register-ScheduledTask' {
        Get-Command Register-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Unregister-ScheduledTask' {
        Get-Command Unregister-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Enable-ScheduledTask' {
        Get-Command Enable-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Disable-ScheduledTask' {
        Get-Command Disable-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Start-ScheduledTask' {
        Get-Command Start-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Stop-ScheduledTask' {
        Get-Command Stop-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Set-ScheduledTask' {
        Get-Command Set-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
    It 'exports Export-ScheduledTask' {
        Get-Command Export-ScheduledTask -Module ScheduledTasks.Linux.Native | Should -Not -BeNullOrEmpty
    }
}

# ---------------------------------------------------------------------------
# Factory cmdlet tests (run everywhere — pure in-memory)
# ---------------------------------------------------------------------------
Describe 'New-ScheduledTaskAction' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'returns a TaskAction with correct Execute' {
        $a = New-ScheduledTaskAction -Execute '/usr/bin/pwsh'
        $a.Execute | Should -Be '/usr/bin/pwsh'
    }
    It 'stores Arguments' {
        $a = New-ScheduledTaskAction -Execute '/usr/bin/pwsh' -Argument '-NoProfile -File /opt/job.ps1'
        $a.Arguments | Should -Be '-NoProfile -File /opt/job.ps1'
    }
    It 'stores WorkingDirectory' {
        $a = New-ScheduledTaskAction -Execute '/usr/bin/pwsh' -WorkingDirectory '/opt'
        $a.WorkingDirectory | Should -Be '/opt'
    }
    It 'Execute is mandatory' {
        { New-ScheduledTaskAction } | Should -Throw
    }
}

Describe 'New-ScheduledTaskTrigger' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'Daily trigger has correct OnCalendar' {
        $t = New-ScheduledTaskTrigger -Daily -At '08:30'
        $t.OnCalendar | Should -Be '*-*-* 08:30:00'
    }
    It 'Weekly trigger includes day names' {
        $t = New-ScheduledTaskTrigger -Weekly -At '09:00' -DaysOfWeek Monday, Friday
        $t.OnCalendar | Should -Match 'Mon'
        $t.OnCalendar | Should -Match 'Fri'
    }
    It 'AtStartup produces boot' {
        $t = New-ScheduledTaskTrigger -AtStartup
        $t.OnCalendar | Should -Be 'boot'
    }
    It 'AtLogOn produces boot' {
        $t = New-ScheduledTaskTrigger -AtLogOn
        $t.OnCalendar | Should -Be 'boot'
    }
    It 'Once trigger includes date' {
        $t = New-ScheduledTaskTrigger -Once -At '2030-01-01 12:00'
        $t.OnCalendar | Should -Match '2030-01-01'
    }
}

Describe 'New-ScheduledTaskPrincipal' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'defaults RunLevel to LeastPrivilege' {
        $p = New-ScheduledTaskPrincipal -UserId 'testuser'
        $p.RunLevel | Should -Be 'LeastPrivilege'
    }
    It 'accepts Highest RunLevel' {
        $p = New-ScheduledTaskPrincipal -UserId 'root' -RunLevel Highest
        $p.RunLevel | Should -Be 'Highest'
    }
    It 'rejects invalid RunLevel' {
        { New-ScheduledTaskPrincipal -UserId 'x' -RunLevel 'Admin' } | Should -Throw
    }
}

Describe 'New-ScheduledTaskSettingsSet' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'defaults to Enabled=true' {
        $s = New-ScheduledTaskSettingsSet
        $s.Enabled | Should -Be $true
    }
    It '-Disable sets Enabled=false' {
        $s = New-ScheduledTaskSettingsSet -Disable
        $s.Enabled | Should -Be $false
    }
    It 'stores RestartCount' {
        $s = New-ScheduledTaskSettingsSet -RestartCount 3 -RestartInterval ([timespan]::FromSeconds(30))
        $s.RestartCount    | Should -Be 3
        $s.RestartInterval | Should -Be ([timespan]::FromSeconds(30))
    }
}

Describe 'New-ScheduledTask' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'combines Action + Description' {
        $a = New-ScheduledTaskAction -Execute '/bin/true'
        $t = New-ScheduledTask -Action $a -Description 'Test task'
        $t.Actions[0].Execute | Should -Be '/bin/true'
        $t.Description        | Should -Be 'Test task'
    }
}

# ---------------------------------------------------------------------------
# WhatIf safety (run everywhere)
# ---------------------------------------------------------------------------
Describe 'WhatIf safety' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'Register-ScheduledTask -WhatIf does not write files' {
        $a = New-ScheduledTaskAction -Execute '/bin/true'
        Register-ScheduledTask -TaskName 'stn_whatif' -Action $a -WhatIf
        '/etc/systemd/system/stn_whatif.timer' | Should -Not -Exist
    }
    It 'Unregister-ScheduledTask -WhatIf does not throw' {
        { Unregister-ScheduledTask -TaskName 'stn_whatif_nonexistent' -WhatIf -Confirm:$false } | Should -Not -Throw
    }
    It 'Enable-ScheduledTask -WhatIf does not throw' {
        { Enable-ScheduledTask -TaskName 'stn_whatif_ne' -WhatIf } | Should -Not -Throw
    }
    It 'Disable-ScheduledTask -WhatIf does not throw' {
        { Disable-ScheduledTask -TaskName 'stn_whatif_ne' -WhatIf } | Should -Not -Throw
    }
    It 'Start-ScheduledTask -WhatIf does not throw' {
        { Start-ScheduledTask -TaskName 'stn_whatif_ne' -WhatIf } | Should -Not -Throw
    }
    It 'Stop-ScheduledTask -WhatIf does not throw' {
        { Stop-ScheduledTask -TaskName 'stn_whatif_ne' -WhatIf } | Should -Not -Throw
    }
}

# ---------------------------------------------------------------------------
# Stub behaviour (run everywhere)
# ---------------------------------------------------------------------------
Describe 'Stub cmdlets' {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'Set-ScheduledTask throws NotImplementedException' {
        { Set-ScheduledTask -TaskName 'dummy' } | Should -Throw -ExceptionType ([System.NotImplementedException])
    }
    It 'Export-ScheduledTask writes an error record' {
        $err = @()
        Export-ScheduledTask -TaskName 'dummy' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
    }
}

# ---------------------------------------------------------------------------
# Elevation errors (run when not root)
# ---------------------------------------------------------------------------
Describe 'Elevation errors' -Skip:($script:isRoot -or -not $script:onLinux) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'Register-ScheduledTask writes a meaningful error when not root' {
        $err = @()
        $a = New-ScheduledTaskAction -Execute '/bin/true'
        Register-ScheduledTask -TaskName 'stn_elev' -Action $a -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Register-ScheduledTask requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.RegisterScheduledTaskCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'Unregister-ScheduledTask writes a meaningful error when not root' {
        $err = @()
        Unregister-ScheduledTask -TaskName 'stn_elev_nonexistent' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Unregister-ScheduledTask requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.UnregisterScheduledTaskCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'Enable-ScheduledTask writes a meaningful error when not root' {
        $err = @()
        Enable-ScheduledTask -TaskName 'stn_elev_ne' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Enable-ScheduledTask requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.EnableScheduledTaskCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'Disable-ScheduledTask writes a meaningful error when not root' {
        $err = @()
        Disable-ScheduledTask -TaskName 'stn_elev_ne' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Disable-ScheduledTask requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.DisableScheduledTaskCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'Start-ScheduledTask writes a meaningful error when not root' {
        $err = @()
        Start-ScheduledTask -TaskName 'stn_elev_ne' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Start-ScheduledTask requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.StartScheduledTaskCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }

    It 'Stop-ScheduledTask writes a meaningful error when not root' {
        $err = @()
        Stop-ScheduledTask -TaskName 'stn_elev_ne' -ErrorVariable err -ErrorAction SilentlyContinue
        $err.Count | Should -BeGreaterThan 0
        $err[0].Exception.Message | Should -Be 'Stop-ScheduledTask requires root privileges.'
        $err[0].FullyQualifiedErrorId | Should -Be 'ElevationRequired,Microsoft.PowerShell.Commands.StopScheduledTaskCommand'
        $err[0].CategoryInfo.Category | Should -Be 'PermissionDenied'
    }
}

# ---------------------------------------------------------------------------
# Integration tests — Linux + root only
# ---------------------------------------------------------------------------
Describe 'Register-ScheduledTask / Get-ScheduledTask integration' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:taskName = "$($script:prefix)_daily"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        # Ensure clean slate
        Unregister-ScheduledTask -TaskName $script:taskName -Confirm:$false -ErrorAction SilentlyContinue

        $action = New-ScheduledTaskAction -Execute '/bin/true'
        $trigger = New-ScheduledTaskTrigger -Daily -At '03:00'
        $script:registered = Register-ScheduledTask -TaskName $script:taskName `
            -Action $action -Trigger $trigger -Description 'Pester test daily task' -Force
    }

    AfterAll {
        Unregister-ScheduledTask -TaskName $script:taskName -Confirm:$false -ErrorAction SilentlyContinue
    }

    It 'Register-ScheduledTask returns a RegisteredTask' {
        $script:registered | Should -Not -BeNullOrEmpty
        $script:registered.TaskName | Should -Be $script:taskName
    }
    It 'task appears in Get-ScheduledTask' {
        $t = Get-ScheduledTask -TaskName $script:taskName
        $t | Should -Not -BeNullOrEmpty
    }
    It 'task has correct TaskPath' {
        $t = Get-ScheduledTask -TaskName $script:taskName
        $t.TaskPath | Should -Be '\'
    }
    It 'unit files exist on disk' {
        "/etc/systemd/system/$($script:taskName).service" | Should -Exist
        "/etc/systemd/system/$($script:taskName).timer"   | Should -Exist
    }
    It 'timer file contains OnCalendar=*-*-* 03:00:00' {
        $content = Get-Content "/etc/systemd/system/$($script:taskName).timer" -Raw
        $content | Should -Match 'OnCalendar=\*-\*-\* 03:00:00'
    }
    It '-Force overwrites without error' {
        $action2 = New-ScheduledTaskAction -Execute '/bin/false'
        { Register-ScheduledTask -TaskName $script:taskName -Action $action2 -Force } | Should -Not -Throw
    }
    It 'duplicate registration without -Force throws' {
        $action3 = New-ScheduledTaskAction -Execute '/bin/true'
        { Register-ScheduledTask -TaskName $script:taskName -Action $action3 } | Should -Throw
    }
    It 'wildcard name filter works' {
        $tasks = Get-ScheduledTask -TaskName "$($script:prefix)*"
        $tasks | Should -Not -BeNullOrEmpty
    }
}

Describe 'Enable/Disable-ScheduledTask integration' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:enName = "$($script:prefix)_endis"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        Unregister-ScheduledTask -TaskName $script:enName -Confirm:$false -ErrorAction SilentlyContinue
        $action = New-ScheduledTaskAction -Execute '/bin/true'
        $trigger = New-ScheduledTaskTrigger -Daily -At '04:00'
        Register-ScheduledTask -TaskName $script:enName -Action $action -Trigger $trigger `
            -Description 'Pester enable/disable test' -Force
    }

    AfterAll {
        Unregister-ScheduledTask -TaskName $script:enName -Confirm:$false -ErrorAction SilentlyContinue
    }

    It 'Disable-ScheduledTask returns a RegisteredTask' {
        $t = Disable-ScheduledTask -TaskName $script:enName
        $t | Should -Not -BeNullOrEmpty
    }
    It 'Enable-ScheduledTask returns a RegisteredTask' {
        $t = Enable-ScheduledTask -TaskName $script:enName
        $t | Should -Not -BeNullOrEmpty
    }
}

Describe 'Unregister-ScheduledTask integration' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:delName = "$($script:prefix)_del"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        Unregister-ScheduledTask -TaskName $script:delName -Confirm:$false -ErrorAction SilentlyContinue
        $action = New-ScheduledTaskAction -Execute '/bin/true'
        $trigger = New-ScheduledTaskTrigger -AtStartup
        Register-ScheduledTask -TaskName $script:delName -Action $action -Trigger $trigger `
            -Description 'Pester unregister test' -Force
    }

    It 'unit files removed after Unregister' {
        Unregister-ScheduledTask -TaskName $script:delName -Confirm:$false
        "/etc/systemd/system/$($script:delName).service" | Should -Not -Exist
        "/etc/systemd/system/$($script:delName).timer"   | Should -Not -Exist
    }
    It 'task gone from Get-ScheduledTask after Unregister' {
        $t = Get-ScheduledTask -TaskName $script:delName
        $t | Should -BeNullOrEmpty
    }
}

Describe 'Get-ScheduledTaskInfo integration' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:infoName = "$($script:prefix)_info"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        Unregister-ScheduledTask -TaskName $script:infoName -Confirm:$false -ErrorAction SilentlyContinue
        $action = New-ScheduledTaskAction -Execute '/bin/true'
        $trigger = New-ScheduledTaskTrigger -Daily -At '05:00'
        Register-ScheduledTask -TaskName $script:infoName -Action $action -Trigger $trigger `
            -Description 'Pester info test' -Force
    }

    AfterAll {
        Unregister-ScheduledTask -TaskName $script:infoName -Confirm:$false -ErrorAction SilentlyContinue
    }

    It 'returns a TaskInfo object' {
        $info = Get-ScheduledTaskInfo -TaskName $script:infoName
        $info | Should -Not -BeNullOrEmpty
    }
    It 'TaskInfo.TaskName matches' {
        $info = Get-ScheduledTaskInfo -TaskName $script:infoName
        $info.TaskName | Should -Be $script:infoName
    }
    It 'NextRunTime is a DateTime or null' {
        $info = Get-ScheduledTaskInfo -TaskName $script:infoName
        if ($null -ne $info.NextRunTime) {
            $info.NextRunTime | Should -BeOfType [datetime]
        }
        else {
            $info.NextRunTime | Should -BeNullOrEmpty
        }
    }
}

Describe 'New-ScheduledTask pipeline integration' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:pipeName = "$($script:prefix)_pipe"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        Unregister-ScheduledTask -TaskName $script:pipeName -Confirm:$false -ErrorAction SilentlyContinue
    }

    AfterAll {
        Unregister-ScheduledTask -TaskName $script:pipeName -Confirm:$false -ErrorAction SilentlyContinue
    }

    It 'New-ScheduledTask | Register-ScheduledTask works' {
        $task = New-ScheduledTask `
            -Action  (New-ScheduledTaskAction -Execute '/bin/true') `
            -Trigger (New-ScheduledTaskTrigger -Daily -At '06:00') `
            -Description 'Pester pipeline test'
        { $task | Register-ScheduledTask -TaskName $script:pipeName -Force } | Should -Not -Throw
        Get-ScheduledTask -TaskName $script:pipeName | Should -Not -BeNullOrEmpty
    }
}

# ---------------------------------------------------------------------------
# Weekly trigger real-world scenario — Linux + root only
# ---------------------------------------------------------------------------
Describe 'Weekly trigger integration' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:weeklyName = "$($script:prefix)_weekly"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        Unregister-ScheduledTask -TaskName $script:weeklyName -Confirm:$false -ErrorAction SilentlyContinue

        $action = New-ScheduledTaskAction -Execute '/bin/true'
        $trigger = New-ScheduledTaskTrigger -Weekly -At '02:30' -DaysOfWeek Monday, Wednesday, Friday
        Register-ScheduledTask -TaskName $script:weeklyName -Action $action -Trigger $trigger `
            -Description 'Pester weekly trigger test' -Force
    }

    AfterAll {
        Unregister-ScheduledTask -TaskName $script:weeklyName -Confirm:$false -ErrorAction SilentlyContinue
    }

    It 'timer file contains Mon' {
        $content = Get-Content "/etc/systemd/system/$($script:weeklyName).timer" -Raw
        $content | Should -Match 'Mon'
    }
    It 'timer file contains Wed' {
        $content = Get-Content "/etc/systemd/system/$($script:weeklyName).timer" -Raw
        $content | Should -Match 'Wed'
    }
    It 'timer file contains Fri' {
        $content = Get-Content "/etc/systemd/system/$($script:weeklyName).timer" -Raw
        $content | Should -Match 'Fri'
    }
    It 'task appears in Get-ScheduledTask' {
        Get-ScheduledTask -TaskName $script:weeklyName | Should -Not -BeNullOrEmpty
    }
    It 'Get-ScheduledTaskInfo returns NextRunTime as DateTime or null' {
        $info = Get-ScheduledTaskInfo -TaskName $script:weeklyName
        $info | Should -Not -BeNullOrEmpty
        if ($null -ne $info.NextRunTime) {
            $info.NextRunTime | Should -BeOfType [datetime]
        }
    }
}

# ---------------------------------------------------------------------------
# AtStartup trigger real-world scenario — Linux + root only
# ---------------------------------------------------------------------------
Describe 'AtStartup trigger integration' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:bootName = "$($script:prefix)_boot"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        Unregister-ScheduledTask -TaskName $script:bootName -Confirm:$false -ErrorAction SilentlyContinue

        $action = New-ScheduledTaskAction -Execute '/bin/true'
        $trigger = New-ScheduledTaskTrigger -AtStartup
        Register-ScheduledTask -TaskName $script:bootName -Action $action -Trigger $trigger `
            -Description 'Pester boot trigger test' -Force
    }

    AfterAll {
        Unregister-ScheduledTask -TaskName $script:bootName -Confirm:$false -ErrorAction SilentlyContinue
    }

    It 'timer file contains boot' {
        $content = Get-Content "/etc/systemd/system/$($script:bootName).timer" -Raw
        $content | Should -Match 'boot'
    }
    It 'task appears in Get-ScheduledTask' {
        Get-ScheduledTask -TaskName $script:bootName | Should -Not -BeNullOrEmpty
    }
}

# ---------------------------------------------------------------------------
# Pipeline disable scenario — Linux + root only
# ---------------------------------------------------------------------------
Describe 'Pipeline Get-ScheduledTask | Disable-ScheduledTask' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:pipDisA = "$($script:prefix)_pdis_a"
        $script:pipDisB = "$($script:prefix)_pdis_b"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        foreach ($n in @($script:pipDisA, $script:pipDisB)) {
            Unregister-ScheduledTask -TaskName $n -Confirm:$false -ErrorAction SilentlyContinue
            $action = New-ScheduledTaskAction -Execute '/bin/true'
            $trigger = New-ScheduledTaskTrigger -Daily -At '01:00'
            Register-ScheduledTask -TaskName $n -Action $action -Trigger $trigger -Force
        }
    }

    AfterAll {
        foreach ($n in @($script:pipDisA, $script:pipDisB)) {
            Unregister-ScheduledTask -TaskName $n -Confirm:$false -ErrorAction SilentlyContinue
        }
    }

    It 'Get-ScheduledTask wildcard | Disable-ScheduledTask disables all matches' {
        { Get-ScheduledTask -TaskName "$($script:prefix)_pdis*" | Disable-ScheduledTask } | Should -Not -Throw
    }
    It 'Get-ScheduledTask wildcard | Enable-ScheduledTask re-enables all matches' {
        { Get-ScheduledTask -TaskName "$($script:prefix)_pdis*" | Enable-ScheduledTask } | Should -Not -Throw
    }
}

# ---------------------------------------------------------------------------
# Start-ScheduledTask actually runs the service — Linux + root only
# ---------------------------------------------------------------------------
Describe 'Start-ScheduledTask runs the service' -Skip:(-not ($script:onLinux -and $script:isRoot -and $script:hasSystemd)) {
    BeforeAll {
        $script:prefix = 'stn_test'
        $script:runName = "$($script:prefix)_run"

        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force

        Unregister-ScheduledTask -TaskName $script:runName -Confirm:$false -ErrorAction SilentlyContinue

        # Use a fast-completing command so the service exits quickly
        $tmpFile = "/tmp/$($script:runName).marker"
        Remove-Item $tmpFile -Force -ErrorAction SilentlyContinue
        $action = New-ScheduledTaskAction -Execute '/bin/touch' -Argument $tmpFile
        $trigger = New-ScheduledTaskTrigger -Daily -At '23:59'
        Register-ScheduledTask -TaskName $script:runName -Action $action -Trigger $trigger -Force
        $script:markerFile = $tmpFile
    }

    AfterAll {
        Unregister-ScheduledTask -TaskName $script:runName -Confirm:$false -ErrorAction SilentlyContinue
        Remove-Item $script:markerFile -Force -ErrorAction SilentlyContinue
    }

    It 'Start-ScheduledTask does not throw' {
        { Start-ScheduledTask -TaskName $script:runName } | Should -Not -Throw
    }
    It 'marker file created within 5 seconds' {
        # Give systemd up to 5 s to launch and complete the service
        $deadline = (Get-Date).AddSeconds(5)
        while (-not (Test-Path $script:markerFile) -and (Get-Date) -lt $deadline) {
            Start-Sleep -Milliseconds 200
        }
        $script:markerFile | Should -Exist
    }
    It 'Get-ScheduledTaskInfo LastRunTime is a recent DateTime' {
        $info = Get-ScheduledTaskInfo -TaskName $script:runName
        $info | Should -Not -BeNullOrEmpty
        if ($null -ne $info.LastRunTime) {
            $info.LastRunTime | Should -BeOfType [datetime]
            $info.LastRunTime | Should -BeGreaterThan (Get-Date).AddMinutes(-2)
        }
    }
}

Describe 'Get-ScheduledTask read-only (Linux, any user)' -Skip:(-not ($script:onLinux -and $script:hasSystemd)) {
    BeforeAll {
        $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Release\net8.0\ScheduledTasks.Linux.Native.dll'
        if (-not (Test-Path $dllPath)) {
            $dllPath = Join-Path $PSScriptRoot '..\..\src\ScheduledTasks.Linux.Native\bin\Debug\net8.0\ScheduledTasks.Linux.Native.dll'
        }
        Import-Module $dllPath -Force
    }

    It 'Get-ScheduledTask returns an array or empty' {
        { Get-ScheduledTask | Out-Null } | Should -Not -Throw
    }
    It 'Get-ScheduledTask -TaskName nonexistent returns empty' {
        $result = Get-ScheduledTask -TaskName 'stn_definitely_not_there_xyzzy'
        $result | Should -BeNullOrEmpty
    }
}
