// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/NewScheduledTaskPrincipalCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Creates a scheduled task principal object.</para>
/// </summary>
[Cmdlet(VerbsCommon.New, "ScheduledTaskPrincipal")]
[OutputType(typeof(TaskPrincipal))]
public sealed class NewScheduledTaskPrincipalCommand : PSCmdlet
{
    [Parameter(Position = 0)]
    public string UserId { get; set; } =
        Environment.GetEnvironmentVariable("USER") ?? Environment.UserName;

    [Parameter(Position = 1)]
    public TaskRunLevel RunLevel { get; set; } = TaskRunLevel.LeastPrivilege;

    [Parameter]
    public string Id { get; set; } = "Author";

    protected override void ProcessRecord()
    {
        WriteObject(new TaskPrincipal
        {
            Id       = Id,
            UserId   = UserId,
            RunLevel = RunLevel,
        });
    }
}
