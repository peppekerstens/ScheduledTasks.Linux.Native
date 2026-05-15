// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/NewScheduledTaskSettingsSetCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Creates a scheduled task settings object.</para>
/// </summary>
[Cmdlet(VerbsCommon.New, "ScheduledTaskSettingsSet")]
[OutputType(typeof(TaskSettings))]
public sealed class NewScheduledTaskSettingsSetCommand : PSCmdlet
{
    [Parameter] public SwitchParameter Disable         { get; set; }
    [Parameter] public SwitchParameter Hidden          { get; set; }
    [Parameter] public int             RestartCount    { get; set; } = 0;
    [Parameter] public TimeSpan        RestartInterval { get; set; } = TimeSpan.Zero;

    protected override void ProcessRecord()
    {
        WriteObject(new TaskSettings
        {
            Enabled         = !Disable.IsPresent,
            Hidden          = Hidden.IsPresent,
            RestartCount    = RestartCount,
            RestartInterval = RestartInterval,
        });
    }
}
