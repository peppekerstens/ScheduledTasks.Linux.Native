// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/StopScheduledTaskCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Stops a running scheduled task on Linux.</para>
/// </summary>
[Cmdlet(VerbsLifecycle.Stop, "ScheduledTask", SupportsShouldProcess = true)]
public sealed class StopScheduledTaskCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string TaskName { get; set; } = string.Empty;

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string TaskPath { get; set; } = @"\";

    protected override void ProcessRecord()
    {
        if (!ShouldProcess(TaskName, "Stop-ScheduledTask")) return;
        SystemdHelpers.ControlService(TaskName, TaskPath, "stop");
    }
}
