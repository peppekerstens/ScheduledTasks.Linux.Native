// Commands/NewScheduledTaskCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Creates a scheduled task definition object (in memory).</para>
/// </summary>
[Cmdlet(VerbsCommon.New, "ScheduledTask")]
[OutputType(typeof(RegisteredTask))]
public sealed class NewScheduledTaskCommand : PSCmdlet
{
    [Parameter] public TaskAction[]?   Action      { get; set; }
    [Parameter] public TaskTrigger[]?  Trigger     { get; set; }
    [Parameter] public TaskPrincipal?  Principal   { get; set; }
    [Parameter] public TaskSettings?   Settings    { get; set; }
    [Parameter] public string          Description { get; set; } = string.Empty;

    protected override void ProcessRecord()
    {
        WriteObject(new RegisteredTask
        {
            Actions     = Action    ?? [],
            Triggers    = Trigger   ?? [],
            Principal   = Principal,
            Settings    = Settings,
            Description = Description,
        });
    }
}
