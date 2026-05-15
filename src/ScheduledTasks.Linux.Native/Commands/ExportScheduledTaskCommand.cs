// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/ExportScheduledTaskCommand.cs
// Stub: Export-ScheduledTask — not yet implemented.
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Exports a scheduled task definition (stub — not yet implemented).</para>
/// </summary>
[Cmdlet(VerbsData.Export, "ScheduledTask")]
[OutputType(typeof(string))]
public sealed class ExportScheduledTaskCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string TaskName { get; set; } = string.Empty;

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string TaskPath { get; set; } = @"\";

    protected override void ProcessRecord()
    {
        WriteError(new ErrorRecord(
            new NotSupportedException("Export-ScheduledTask is not yet implemented in ScheduledTasks.Linux.Native."),
            "NotImplemented", ErrorCategory.NotImplemented, TaskName));
    }
}
