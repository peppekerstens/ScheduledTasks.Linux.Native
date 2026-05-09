// Commands/GetScheduledTaskInfoCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Gets run history and next run time for a scheduled task on Linux.</para>
/// </summary>
[Cmdlet(VerbsCommon.Get, "ScheduledTaskInfo")]
[OutputType(typeof(TaskInfo))]
public sealed class GetScheduledTaskInfoCommand : PSCmdlet
{
    [Parameter(Position = 0)]
    [SupportsWildcards]
    public string[]? TaskName { get; set; }

    [Parameter]
    [SupportsWildcards]
    public string? TaskPath { get; set; }

    protected override void ProcessRecord()
    {
        var tasks = SystemdHelpers.ListTasks(TaskName, TaskPath);
        foreach (var t in tasks)
            WriteObject(SystemdHelpers.GetTimerInfo(t));
    }
}
