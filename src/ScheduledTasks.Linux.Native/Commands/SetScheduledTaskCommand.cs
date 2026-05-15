// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/SetScheduledTaskCommand.cs
// Stub: Set-ScheduledTask — not yet implemented; throws a NotSupportedException.
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Modifies a scheduled task on Linux (stub — not yet implemented).</para>
/// </summary>
[Cmdlet(VerbsCommon.Set, "ScheduledTask")]
[OutputType(typeof(RegisteredTask))]
public sealed class SetScheduledTaskCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string TaskName { get; set; } = string.Empty;

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string TaskPath { get; set; } = @"\";

    [Parameter] public TaskAction[]?  Action    { get; set; }
    [Parameter] public TaskTrigger[]? Trigger   { get; set; }
    [Parameter] public TaskPrincipal? Principal { get; set; }
    [Parameter] public TaskSettings?  Settings  { get; set; }

    protected override void ProcessRecord()
    {
        throw new NotImplementedException("Set-ScheduledTask is not supported on Linux.");
    }
}
