// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/NewScheduledTaskActionCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Creates a scheduled task action object.</para>
/// </summary>
[Cmdlet(VerbsCommon.New, "ScheduledTaskAction")]
[OutputType(typeof(TaskAction))]
public sealed class NewScheduledTaskActionCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)]
    public string Execute { get; set; } = string.Empty;

    [Parameter(Position = 1)]
    [Alias("Arguments")]
    public string? Argument { get; set; }

    [Parameter]
    public string? WorkingDirectory { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject(new TaskAction
        {
            Execute          = Execute,
            Arguments        = Argument        ?? string.Empty,
            WorkingDirectory = WorkingDirectory ?? string.Empty,
        });
    }
}
