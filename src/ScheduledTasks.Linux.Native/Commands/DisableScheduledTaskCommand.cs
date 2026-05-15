// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/DisableScheduledTaskCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Disables a scheduled task on Linux.</para>
/// </summary>
[Cmdlet(VerbsLifecycle.Disable, "ScheduledTask", SupportsShouldProcess = true)]
[OutputType(typeof(RegisteredTask))]
public sealed class DisableScheduledTaskCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string TaskName { get; set; } = string.Empty;

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string TaskPath { get; set; } = @"\";

    protected override void ProcessRecord()
    {
        if (!ShouldProcess(TaskName, "Disable-ScheduledTask")) return;
        SystemdHelpers.DisableTask(TaskName, TaskPath);
        var tasks = SystemdHelpers.ListTasks([TaskName], TaskPath);
        foreach (var t in tasks) WriteObject(t);
    }
}
