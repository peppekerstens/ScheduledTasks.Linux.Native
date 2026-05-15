// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Models/ScheduledTaskModels.cs
// In-memory data types for ScheduledTasks.Linux.Native.
// These mirror the Windows ScheduledTasks module's object shapes.

namespace Microsoft.PowerShell.Commands;

/// <summary>Represents a registered systemd timer task.</summary>
public sealed class RegisteredTask
{
    public string TaskName    { get; set; } = string.Empty;
    public string TaskPath    { get; set; } = @"\";
    public string State       { get; set; } = "Unknown";
    public string Description { get; set; } = string.Empty;
    public TaskAction[]   Actions   { get; set; } = [];
    public TaskTrigger[]  Triggers  { get; set; } = [];
    public TaskPrincipal? Principal { get; set; }
    public TaskSettings?  Settings  { get; set; }

    public override string ToString() => TaskName;
}

/// <summary>Represents a task action (ExecStart in systemd terms).</summary>
public sealed class TaskAction
{
    public string Execute          { get; set; } = string.Empty;
    public string Arguments        { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;

    public override string ToString() => Execute;
}

/// <summary>Represents a task trigger (OnCalendar / OnBootSec).</summary>
public sealed class TaskTrigger
{
    public string    TriggerType  { get; set; } = "Daily";
    public DateTime? At           { get; set; }
    public string[]  DaysOfWeek   { get; set; } = [];
    public TimeSpan  RandomDelay  { get; set; } = TimeSpan.Zero;
    /// <summary>systemd OnCalendar expression, or "boot" for AtStartup/AtLogOn.</summary>
    public string    OnCalendar   { get; set; } = string.Empty;

    public override string ToString() => OnCalendar;
}

/// <summary>Represents the principal (user) under which a task runs.</summary>
public sealed class TaskPrincipal
{
    public string Id       { get; set; } = "Author";
    public string UserId   { get; set; } = string.Empty;
    public string RunLevel { get; set; } = "Limited";

    public override string ToString() => UserId;
}

/// <summary>Settings for a scheduled task.</summary>
public sealed class TaskSettings
{
    public bool     Enabled         { get; set; } = true;
    public bool     Hidden          { get; set; }
    public int      RestartCount    { get; set; }
    public TimeSpan RestartInterval { get; set; } = TimeSpan.Zero;

    public override string ToString() => Enabled ? "Enabled" : "Disabled";
}

/// <summary>Run-time / history information for a registered task.</summary>
public sealed class TaskInfo
{
    public string    TaskName           { get; set; } = string.Empty;
    public string    TaskPath           { get; set; } = @"\";
    public DateTime? LastRunTime        { get; set; }
    public int       LastTaskResult     { get; set; }
    public DateTime? NextRunTime        { get; set; }
    public int       NumberOfMissedRuns { get; set; }

    public override string ToString() => TaskName;
}
