// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/UnregisterScheduledTaskCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Deletes a scheduled task on Linux.</para>
/// </summary>
[Cmdlet(VerbsLifecycle.Unregister, "ScheduledTask",
    SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
public sealed class UnregisterScheduledTaskCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string TaskName { get; set; } = string.Empty;

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string TaskPath { get; set; } = @"\";

    protected override void ProcessRecord()
    {
        if (!ShouldProcess(TaskName, "Unregister-ScheduledTask")) return;
        try
        {
            SystemdHelpers.UnregisterTask(TaskName, TaskPath);
        }
        catch (UnauthorizedAccessException)
        {
            WriteError(new ErrorRecord(
                new InvalidOperationException("Unregister-ScheduledTask requires root privileges."),
                "ElevationRequired", ErrorCategory.PermissionDenied, TaskName));
            return;
        }
    }
}
