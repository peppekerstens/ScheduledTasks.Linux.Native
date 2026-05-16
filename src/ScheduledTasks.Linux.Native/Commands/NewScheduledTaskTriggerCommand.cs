// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/NewScheduledTaskTriggerCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Creates a scheduled task trigger object.</para>
/// </summary>
[Cmdlet(VerbsCommon.New, "ScheduledTaskTrigger", DefaultParameterSetName = "Once")]
[OutputType(typeof(TaskTrigger))]
public sealed class NewScheduledTaskTriggerCommand : PSCmdlet
{
    [Parameter(ParameterSetName = "Once",      Mandatory = true)] public SwitchParameter Once      { get; set; }
    [Parameter(ParameterSetName = "Daily",     Mandatory = true)] public SwitchParameter Daily     { get; set; }
    [Parameter(ParameterSetName = "Weekly",    Mandatory = true)] public SwitchParameter Weekly    { get; set; }
    [Parameter(ParameterSetName = "AtStartup", Mandatory = true)] public SwitchParameter AtStartup { get; set; }
    [Parameter(ParameterSetName = "AtLogOn",   Mandatory = true)] public SwitchParameter AtLogOn   { get; set; }

    [Parameter(ParameterSetName = "Once",   Mandatory = true)]
    [Parameter(ParameterSetName = "Daily",  Mandatory = true)]
    [Parameter(ParameterSetName = "Weekly", Mandatory = true)]
    public DateTime At { get; set; }

    [Parameter(ParameterSetName = "Weekly", Mandatory = true)]
    public DayOfWeek[]? DaysOfWeek { get; set; }

    [Parameter]
    public TimeSpan RandomDelay { get; set; } = TimeSpan.Zero;

    protected override void ProcessRecord()
    {
        string onCalendar = ParameterSetName switch
        {
            "Once"      => At.ToString("yyyy-MM-dd HH:mm:ss"),
            "Daily"     => $"*-*-* {At.Hour:D2}:{At.Minute:D2}:00",
            "Weekly"    => BuildWeekly(),
            "AtStartup" => "boot",
            "AtLogOn"   => "boot",
            _           => "daily",
        };

        TaskTriggerType triggerType = ParameterSetName switch
        {
            "Once"      => TaskTriggerType.Time,
            "Daily"     => TaskTriggerType.Daily,
            "Weekly"    => TaskTriggerType.Weekly,
            "AtStartup" => TaskTriggerType.Boot,
            "AtLogOn"   => TaskTriggerType.Logon,
            _           => TaskTriggerType.Daily,
        };

        WriteObject(new TaskTrigger
        {
            TriggerType = triggerType,
            At          = ParameterSetName is "Once" or "Daily" or "Weekly" ? At : null,
            DaysOfWeek  = DaysOfWeek?.Select(d => d.ToString()[..3]).ToArray() ?? [],
            RandomDelay = RandomDelay,
            OnCalendar  = onCalendar,
        });
    }

    private string BuildWeekly()
    {
        var days = DaysOfWeek?.Select(d => d.ToString()[..3]) ?? [];
        return $"{string.Join(',', days)} {At.Hour:D2}:{At.Minute:D2}:00";
    }
}
