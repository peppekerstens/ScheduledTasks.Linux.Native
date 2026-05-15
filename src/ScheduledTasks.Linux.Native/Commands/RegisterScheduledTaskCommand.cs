// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Commands/RegisterScheduledTaskCommand.cs
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands;

/// <summary>
/// <para type="synopsis">Registers a scheduled task on Linux using systemd timer units.</para>
/// </summary>
[Cmdlet(VerbsLifecycle.Register, "ScheduledTask",
    DefaultParameterSetName = "Components", SupportsShouldProcess = true)]
[OutputType(typeof(RegisteredTask))]
public sealed class RegisterScheduledTaskCommand : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)]
    public string TaskName { get; set; } = string.Empty;

    [Parameter]
    public string TaskPath { get; set; } = @"\";

    [Parameter(ParameterSetName = "InputObject", ValueFromPipeline = true)]
    public RegisteredTask? InputObject { get; set; }

    [Parameter(ParameterSetName = "Components")]
    public TaskAction[]? Action { get; set; }

    [Parameter(ParameterSetName = "Components")]
    public TaskTrigger[]? Trigger { get; set; }

    [Parameter(ParameterSetName = "Components")]
    public TaskPrincipal? Principal { get; set; }

    [Parameter(ParameterSetName = "Components")]
    public TaskSettings? Settings { get; set; }

    [Parameter]
    public string Description { get; set; } = string.Empty;

    [Parameter]
    public SwitchParameter Force { get; set; }

    protected override void ProcessRecord()
    {
        TaskAction[]  actions;
        TaskTrigger[] triggers;
        TaskPrincipal? principal;
        TaskSettings?  settings;
        string desc;

        if (ParameterSetName == "InputObject" && InputObject != null)
        {
            actions   = InputObject.Actions;
            triggers  = InputObject.Triggers;
            principal = InputObject.Principal;
            settings  = InputObject.Settings;
            desc      = string.IsNullOrEmpty(Description) ? InputObject.Description : Description;
        }
        else
        {
            actions   = Action   ?? [];
            triggers  = Trigger  ?? [];
            principal = Principal;
            settings  = Settings;
            desc      = Description;
        }

        if (!ShouldProcess(TaskName, "Register-ScheduledTask")) return;

        try
        {
            var task = SystemdHelpers.RegisterTask(
                TaskName, TaskPath, actions, triggers, principal, settings, desc, Force.IsPresent);
            WriteObject(task);
        }
        catch (UnauthorizedAccessException)
        {
            WriteError(new ErrorRecord(
                new InvalidOperationException("Register-ScheduledTask requires root privileges."),
                "ElevationRequired", ErrorCategory.PermissionDenied, TaskName));
            return;
        }
        catch (InvalidOperationException ex)
        {
            WriteError(new ErrorRecord(ex, "RegisterFailed", ErrorCategory.InvalidOperation, TaskName));
        }
    }
}
