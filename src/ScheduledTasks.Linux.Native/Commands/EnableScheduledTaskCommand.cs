// Commands/EnableScheduledTaskCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Enables a scheduled task on Linux.</para>
/// </summary>
[Cmdlet(VerbsLifecycle.Enable, "ScheduledTask", SupportsShouldProcess = true)]
[OutputType(typeof(RegisteredTask))]
public sealed class EnableScheduledTaskCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string TaskName { get; set; } = string.Empty;

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string TaskPath { get; set; } = @"\";

    protected override void ProcessRecord()
    {
        if (!ShouldProcess(TaskName, "Enable-ScheduledTask")) return;
        SystemdHelpers.EnableTask(TaskName, TaskPath);
        var tasks = SystemdHelpers.ListTasks([TaskName], TaskPath);
        foreach (var t in tasks) WriteObject(t);
    }
}
