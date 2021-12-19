namespace NukeTaskRunner.TaskRunner;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Helpers;

using Microsoft.VisualStudio.TaskRunnerExplorer;

using static System.StringComparison;

using static Constants;

[TaskRunnerExport(FILENAME)]
internal class TaskRunnerProvider : ITaskRunner
{
    public List<ITaskRunnerOption> Options
    {
        get
        {
            if (_options == null)
            {
                InitializeNukeTaskRunnerOptions();
            }

            return _options;
        }
    }

    public Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
    {
        ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

        if (!hierarchy.Children.Any() ||
            !hierarchy.Children.First()
                .Children.Any())
        {
            return Task.FromResult<ITaskRunnerConfig>(null);
        }

        return Task.FromResult<ITaskRunnerConfig>(
            new TaskRunnerConfig(context, hierarchy, _cliCommandName)
        );
    }

    private string _cliCommandName;

    //private ImageSource _icon;
    private List<ITaskRunnerOption> _options;

    private void AddCommands(
        string configPath,
        SortedList<string, string> scripts,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> commands,
        TaskNode tasks,
        bool isNuke
    )
    {
        string cwd = Path.GetDirectoryName(configPath);

        foreach (KeyValuePair<string, IEnumerable<string>> parent in commands)
        {
            TaskNode parentTask = CreateTask(
                cwd,
                parent.Key,
                scripts[parent.Key],
                isNuke
            );

            foreach (string child in parent.Value)
            {
                TaskNode childTask = CreateTask(
                    cwd,
                    child,
                    scripts[child],
                    isNuke
                );
                parentTask.Children.Add(childTask);
            }

            tasks.Children.Add(parentTask);
        }
    }

    private static TaskNode CreateTask(
        string cwd,
        string name,
        string cmd,
        bool isNuke
    )
        => new(name, cmd is not (null or ""), isNuke)
        {
            Command = new TaskRunnerCommand(
                cwd,
                "cmd.exe",
                $"/c {cmd}"),
            Description = $"Runs the '{name}' command",
        };

    private static IEnumerable<string> GetChildScripts(
        string parent, IEnumerable<string> events)
    {
        IEnumerable<string> candidates
            = events.Where(
                e => e.EndsWith(parent, OrdinalIgnoreCase));

        foreach (string candidate in candidates)
        {
            if (candidate.StartsWith(POST_SCRIPT_PREFIX, Ordinal) &&
                (POST_SCRIPT_PREFIX + parent == candidate))
            {
                yield return candidate;
            }

            if (candidate.StartsWith(PRE_SCRIPT_PREFIX, Ordinal) &&
                (PRE_SCRIPT_PREFIX + parent == candidate))
            {
                yield return candidate;
            }
        }
    }

    private static string GetCliCommandName(string configPath)
    {
        string cwd = Path.GetDirectoryName(configPath);

        return cwd is null
            ? Constants.NUKE_CLI_COMMAND
            : Path.Combine(cwd, Constants.NUKE_CLI_COMMAND);
    }

    private SortedList<string, IEnumerable<string>> GetHierarchy(
        IEnumerable<string> enumerableTasks)
    {
        List<string> allTasks = enumerableTasks?.ToList();

        if ((allTasks == null) ||
            !allTasks.Any())
        {
            return null;
        }

        List<string> events = allTasks.Where(
            static t => t.StartsWith(PRE_SCRIPT_PREFIX, Ordinal) ||
                        t.StartsWith(POST_SCRIPT_PREFIX, Ordinal)
        ).ToList();

        List<string> parents = allTasks.Except(events)
            .ToList();

        var hierarchy =
            new SortedList<string, IEnumerable<string>>();

        foreach (string parent in parents)
        {
            ImmutableSortedSet<string> children =
                GetChildScripts(parent, events)
                    .OrderByDescending(static child => child)
                    .ToImmutableSortedSet();

            hierarchy.Add(parent, children);
            events =
                events
                    .Except(children)
                    .ToList();
        }

        foreach (string child in events)
        {
            hierarchy.Add(child, Enumerable.Empty<string>());
        }

        return hierarchy;
    }

    private void InitializeNukeTaskRunnerOptions()
    {
        _options = new List<ITaskRunnerOption>
        {
            new TaskRunnerOption(
                "Verbose",
                PackageIds.cmdVerbose,
                PackageGuids.guidVSPackageCmdSet,
                false,
                NUKE_VERBOSE_OPTION
            ),};
    }

    private ITaskRunnerNode LoadHierarchy(string configPath)
    {
        _cliCommandName = GetCliCommandName(configPath);
        bool isNuke = _cliCommandName == NUKE_CLI_COMMAND;

        ITaskRunnerNode root =
            new TaskNode(
                Vsix.Name,
                false,
                isNuke);

        SortedList<string, string> scripts =
            TaskParser.LoadTasks(
                configPath,
                _cliCommandName);

        SortedList<string, IEnumerable<string>> hierarchy =
            GetHierarchy(scripts.Keys);

        Dictionary<string, IEnumerable<string>> defaults
            = hierarchy
                .Where(static h => NukeAllDefaultTasks.Contains(h.Key))
                .ToDictionary(
                    static pair => pair.Key,
                    static pair => pair.Value);

        TaskNode defaultTasks = new("Defaults", false, isNuke)
        {
            Description = $"Default predefined {_cliCommandName} commands.",
        };

        root.Children.Add(defaultTasks);

        AddCommands(
            configPath,
            scripts,
            defaults,
            defaultTasks,
            isNuke
        );

        if (hierarchy.Count == defaults.Count)
        {
            return root;
        }

        List<KeyValuePair<string, IEnumerable<string>>> customs
            = hierarchy
                .Except(defaults)
                .ToList();

        TaskNode customTasks = new("Custom", false, isNuke)
        {
            Description = $"Custom {_cliCommandName} commands.",
        };

        root.Children.Add(customTasks);

        AddCommands(
            configPath,
            scripts,
            customs,
            customTasks,
            isNuke
        );

        return root;
    }
}
