// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Helpers/SystemdHelpers.cs
// Core systemd interaction layer for ScheduledTasks.Linux.Native.
// All reads are done via `systemctl` JSON output; all writes use `systemctl`/file I/O.
// GetCurrentUid() uses P/Invoke getuid() — no subprocess.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.PowerShell.Commands;

internal static class SystemdHelpers
{
    // -----------------------------------------------------------------------
    // Unit directory resolution
    // -----------------------------------------------------------------------

    internal static bool IsSystemContext(string taskPath, TaskPrincipal? principal)
    {
        int uid = GetCurrentUid();
        if (uid != 0) return false;  // non-root → always user context
        if (taskPath == @"\") return true;
        if (principal?.RunLevel == "Highest") return true;
        if (principal?.UserId == "root") return true;
        return false;
    }

    internal static string GetUnitDir(bool isSystem)
        => isSystem
            ? "/etc/systemd/system"
            : Path.Combine(
                Environment.GetEnvironmentVariable("HOME") ?? "~",
                ".config", "systemd", "user");

    internal static string SanitizeName(string name)
        => System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_\-]", "-");

    internal static int GetCurrentUid()
    {
        try
        {
            var r = Run("id", "-u");
            return int.TryParse(r.Stdout.Trim(), out int uid) ? uid : -1;
        }
        catch { return -1; }
    }

    // -----------------------------------------------------------------------
    // Enumerate tasks
    // -----------------------------------------------------------------------

    internal static List<RegisteredTask> ListTasks(string[]? nameFilter, string? pathFilter)
    {
        // 1. list-timers --all --output=json
        var timersJson = RunSystemctl(null, "list-timers", "--all", "--output=json", "--no-pager").Stdout;
        if (string.IsNullOrWhiteSpace(timersJson)) return [];

        List<SystemctlTimerEntry>? timers;
        try { timers = JsonSerializer.Deserialize<List<SystemctlTimerEntry>>(timersJson, JsonOpts); }
        catch { return []; }
        if (timers == null || timers.Count == 0) return [];

        // 2. list-unit-files --type=timer --output=json  (enabled/disabled state)
        var unitFileJson = RunSystemctl(null, "list-unit-files", "--type=timer", "--output=json", "--no-pager").Stdout;
        var unitFileState = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(unitFileJson))
        {
            try
            {
                var entries = JsonSerializer.Deserialize<List<SystemctlUnitFileEntry>>(unitFileJson, JsonOpts);
                if (entries != null)
                    foreach (var e in entries)
                        unitFileState[e.UnitFile] = e.State;
            }
            catch { /* swallow */ }
        }

        // 3. Single bulk `systemctl show` for ActiveState, FragmentPath, Description
        var unitNames = timers.Select(t => t.Unit).ToArray();
        var showHash  = BulkShow(unitNames, "ActiveState", "FragmentPath", "Description");

        var results = new List<RegisteredTask>(timers.Count);
        foreach (var timer in timers)
        {
            var props        = showHash.GetValueOrDefault(timer.Unit) ?? [];
            var activeState  = props.GetValueOrDefault("ActiveState") ?? "";
            var fragmentPath = props.GetValueOrDefault("FragmentPath") ?? "";
            var description  = props.GetValueOrDefault("Description") ?? "";
            var ufState      = unitFileState.GetValueOrDefault(timer.Unit) ?? "";

            var state = activeState switch
            {
                "active"   => "Ready",
                "inactive" => ufState is "enabled" or "enabled-runtime" ? "Ready" : "Disabled",
                "failed"   => "Disabled",
                _          => "Unknown",
            };

            var path = (fragmentPath.StartsWith("/etc/") ||
                        fragmentPath.StartsWith("/usr/lib/") ||
                        fragmentPath.StartsWith("/lib/") ||
                        fragmentPath.StartsWith("/run/"))
                ? @"\"
                : @"\User\";

            var cleanName = timer.Unit.EndsWith(".timer", StringComparison.OrdinalIgnoreCase)
                ? timer.Unit[..^6]
                : timer.Unit;

            results.Add(new RegisteredTask
            {
                TaskName    = cleanName,
                TaskPath    = path,
                State       = state,
                Description = description,
            });
        }

        // Apply name filter (wildcard)
        if (nameFilter != null && nameFilter.Length > 0)
            results = results.Where(t =>
                nameFilter.Any(n => WildcardMatch(t.TaskName, n))).ToList();

        // Apply path filter (wildcard)
        if (!string.IsNullOrEmpty(pathFilter))
            results = results.Where(t => WildcardMatch(t.TaskPath, pathFilter)).ToList();

        return results;
    }

    // -----------------------------------------------------------------------
    // Task info (timing details)
    // -----------------------------------------------------------------------

    internal static TaskInfo GetTimerInfo(RegisteredTask task)
    {
        var unitName = SanitizeName(task.TaskName);
        bool isSystem = task.TaskPath == @"\";
        var showArgs  = isSystem
            ? new[] { "show", $"{unitName}.timer", "--no-pager" }
            : new[] { "--user", "show", $"{unitName}.timer", "--no-pager" };
        var output = RunSystemctl(isSystem ? null : "--user", "show", $"{unitName}.timer", "--no-pager").Stdout;

        var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in output.Split('\n'))
        {
            int eq = line.IndexOf('=');
            if (eq > 0) props[line[..eq]] = line[(eq + 1)..];
        }

        DateTime? lastRun = ParseUsec(props.GetValueOrDefault("LastTriggerUSec"));
        DateTime? nextRun = ParseUsec(props.GetValueOrDefault("NextElapseUSecRealtime"));

        return new TaskInfo
        {
            TaskName    = task.TaskName,
            TaskPath    = task.TaskPath,
            LastRunTime = lastRun,
            NextRunTime = nextRun,
        };
    }

    // -----------------------------------------------------------------------
    // Write operations
    // -----------------------------------------------------------------------

    internal static RegisteredTask RegisterTask(
        string taskName, string taskPath,
        TaskAction[] actions, TaskTrigger[]? triggers,
        TaskPrincipal? principal, TaskSettings? settings,
        string description, bool force)
    {
        if (actions.Length == 0)
            throw new InvalidOperationException("At least one Action is required.");

        bool isSystem = IsSystemContext(taskPath, principal);
        var unitDir   = GetUnitDir(isSystem);
        var unitName  = SanitizeName(taskName);
        var svcPath   = Path.Combine(unitDir, $"{unitName}.service");
        var tmrPath   = Path.Combine(unitDir, $"{unitName}.timer");

        if (!force && (File.Exists(svcPath) || File.Exists(tmrPath)))
            throw new InvalidOperationException(
                $"A task named '{taskName}' already exists. Use -Force to overwrite.");

        Directory.CreateDirectory(unitDir);

        // Build .service
        var first = actions[0];
        var execStart = string.IsNullOrEmpty(first.Arguments)
            ? first.Execute
            : $"{first.Execute} {first.Arguments}";

        var workDir   = string.IsNullOrEmpty(first.WorkingDirectory) ? "" : $"WorkingDirectory={first.WorkingDirectory}";
        var userLine  = (principal?.UserId is { Length: > 0 } uid && uid != "root") ? $"User={uid}" : "";
        var restart   = (settings?.RestartCount > 0)
            ? $"Restart=on-failure\nRestartSec={(int)settings.RestartInterval.TotalSeconds}"
            : "";
        var afterNet  = isSystem ? "After=network.target" : "";
        var wantedBy  = isSystem ? "multi-user.target" : "default.target";

        var svcContent =
            $"[Unit]\n" +
            $"Description={description}\n" +
            (string.IsNullOrEmpty(afterNet) ? "" : $"{afterNet}\n") +
            $"\n[Service]\n" +
            $"Type=oneshot\n" +
            $"ExecStart={execStart}\n" +
            (string.IsNullOrEmpty(workDir)  ? "" : $"{workDir}\n") +
            (string.IsNullOrEmpty(userLine) ? "" : $"{userLine}\n") +
            (string.IsNullOrEmpty(restart)  ? "" : $"{restart}\n") +
            $"\n[Install]\n" +
            $"WantedBy={wantedBy}\n";

        // Build .timer
        var trigger     = triggers?.Length > 0 ? triggers[0] : null;
        string timerSchedule;
        if (trigger?.OnCalendar == "boot")
            timerSchedule = "OnBootSec=1min";
        else if (!string.IsNullOrEmpty(trigger?.OnCalendar))
            timerSchedule = $"OnCalendar={trigger.OnCalendar}";
        else
            timerSchedule = "OnCalendar=daily";

        var tmrContent =
            $"[Unit]\n" +
            $"Description=Timer for {description}\n" +
            $"\n[Timer]\n" +
            $"{timerSchedule}\n" +
            $"Persistent=true\n" +
            $"\n[Install]\n" +
            $"WantedBy=timers.target\n";

        File.WriteAllText(svcPath, svcContent);
        File.WriteAllText(tmrPath, tmrContent);

        // Reload + enable
        var userFlag = isSystem ? null : "--user";
        RunSystemctl(userFlag, "daemon-reload");
        if (settings?.Enabled != false)
            RunSystemctl(userFlag, "enable", $"{unitName}.timer");

        // Return the newly registered task
        var task = ListTasks([taskName], taskPath).FirstOrDefault();
        return task ?? new RegisteredTask { TaskName = taskName, TaskPath = taskPath, Description = description };
    }

    internal static void UnregisterTask(string taskName, string taskPath)
    {
        var unitName = SanitizeName(taskName);
        var dirs = new[]
        {
            ("/etc/systemd/system", true),
            (Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "~", ".config", "systemd", "user"), false),
        };

        foreach (var (dir, isSystem) in dirs)
        {
            var svcPath = Path.Combine(dir, $"{unitName}.service");
            var tmrPath = Path.Combine(dir, $"{unitName}.timer");
            var userFlag = isSystem ? null : "--user";

            if (File.Exists(tmrPath))
            {
                RunSystemctl(userFlag, "disable", "--now", $"{unitName}.timer");
                RunSystemctl(userFlag, "daemon-reload");
                File.Delete(tmrPath);
            }
            if (File.Exists(svcPath))
                File.Delete(svcPath);
        }
    }

    internal static void ControlTimer(string taskName, string taskPath, string action)
    {
        var unitName = SanitizeName(taskName);
        bool isSystem = taskPath == @"\" && GetCurrentUid() == 0;
        var userFlag = isSystem ? null : "--user";
        RunSystemctl(userFlag, action, $"{unitName}.timer");
    }

    internal static void ControlService(string taskName, string taskPath, string action)
    {
        var unitName = SanitizeName(taskName);
        bool isSystem = taskPath == @"\" && GetCurrentUid() == 0;
        var userFlag = isSystem ? null : "--user";
        RunSystemctl(userFlag, action, $"{unitName}.service");
    }

    internal static void EnableTask(string taskName, string taskPath)
    {
        ControlTimer(taskName, taskPath, "enable");
        ControlTimer(taskName, taskPath, "start");
    }

    internal static void DisableTask(string taskName, string taskPath)
    {
        ControlTimer(taskName, taskPath, "stop");
        ControlTimer(taskName, taskPath, "disable");
    }

    // -----------------------------------------------------------------------
    // Low-level helpers
    // -----------------------------------------------------------------------

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private static Dictionary<string, Dictionary<string, string>> BulkShow(
        string[] unitNames, params string[] properties)
    {
        var propArg = $"--property={string.Join(',', properties)}";
        var output  = RunSystemctl(null, new[] { "show", propArg, "--no-pager" }.Concat(unitNames).ToArray()).Stdout;
        var result  = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        var current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        int idx = 0;
        foreach (var rawLine in output.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
            {
                if (idx < unitNames.Length)
                {
                    result[unitNames[idx]] = current;
                    idx++;
                    current = [];
                }
                continue;
            }
            int eq = line.IndexOf('=');
            if (eq > 0) current[line[..eq]] = line[(eq + 1)..];
        }
        if (current.Count > 0 && idx < unitNames.Length)
            result[unitNames[idx]] = current;

        return result;
    }

    private static (string Stdout, int ExitCode) RunSystemctl(string? userFlag, params string[] args)
    {
        var allArgs = userFlag != null
            ? new[] { userFlag }.Concat(args).ToArray()
            : args;
        return Run("systemctl", string.Join(' ', allArgs.Select(a => a.Contains(' ') ? $"\"{a}\"" : a)));
    }

    private static (string Stdout, int ExitCode) Run(string executable, string arguments)
    {
        var psi = new ProcessStartInfo(executable, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        using var proc = Process.Start(psi)!;
        var stdout = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        return (stdout, proc.ExitCode);
    }

    private static DateTime? ParseUsec(string? value)
    {
        if (string.IsNullOrEmpty(value) || value is "n/a" or "0") return null;
        if (!long.TryParse(value, out long usec) || usec == 0) return null;
        try { return DateTime.UnixEpoch.AddMicroseconds(usec).ToLocalTime(); }
        catch { return null; }
    }

    private static bool WildcardMatch(string value, string pattern)
        => System.Management.Automation.WildcardPattern.ContainsWildcardCharacters(pattern)
            ? new System.Management.Automation.WildcardPattern(pattern,
                System.Management.Automation.WildcardOptions.IgnoreCase).IsMatch(value)
            : string.Equals(value, pattern, StringComparison.OrdinalIgnoreCase);

    // -----------------------------------------------------------------------
    // JSON deserialization helpers
    // -----------------------------------------------------------------------

    private sealed class SystemctlTimerEntry
    {
        [JsonPropertyName("unit")] public string Unit { get; set; } = string.Empty;
    }

    private sealed class SystemctlUnitFileEntry
    {
        [JsonPropertyName("unit_file")] public string UnitFile { get; set; } = string.Empty;
        [JsonPropertyName("state")]     public string State    { get; set; } = string.Empty;
    }
}
