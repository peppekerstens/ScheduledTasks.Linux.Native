// Commands/GetScheduledTaskCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Gets scheduled tasks on Linux (systemd timers).</para>
/// <para type="description">
/// Returns registered task objects from systemd timer units.
/// Covers both system timers (/etc/systemd/system/, /usr/lib/systemd/system/)
/// and user timers (~/.config/systemd/user/).
/// </para>
/// </summary>
[Cmdlet(VerbsCommon.Get, "ScheduledTask")]
[OutputType(typeof(RegisteredTask))]
public sealed class GetScheduledTaskCommand : PSCmdlet
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
            WriteObject(t);
    }
}
