// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/StartScheduledTaskCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Immediately runs a scheduled task on Linux.</para>
/// </summary>
[Cmdlet(VerbsLifecycle.Start, "ScheduledTask", SupportsShouldProcess = true)]
public sealed class StartScheduledTaskCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string TaskName { get; set; } = string.Empty;

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string TaskPath { get; set; } = @"\";

    protected override void ProcessRecord()
    {
        if (!ShouldProcess(TaskName, "Start-ScheduledTask")) return;
        SystemdHelpers.ControlService(TaskName, TaskPath, "start");
    }
}
