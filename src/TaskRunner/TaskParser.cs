// ReSharper disable UnusedParameter.Local
namespace NukeTaskRunner.TaskRunner;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using static System.Text.RegularExpressions.RegexOptions;

using static Constants;

internal class TaskParser
{
    public static SortedList<string, string> LoadTasks(string configPath, string cliCommandName)
    {
        var list = new SortedList<string, string>();

        try
        {
            string document = File.ReadAllText(configPath);

            Regex regex = new(@"\bTarget (?<Target>[^\s=]+)\b",
                Multiline |
                Compiled |
                ExplicitCapture);

            Match[] targets = regex.Matches(document).OfType<Match>().ToArray();

            if (targets.Any())
            {
                foreach (Match target in targets)
                {
                    string targetName = target.Groups.Cast<Group>().LastOrDefault()?.Value ?? "<<null>>";
                    if (!list.ContainsKey(targetName))
                    {
                        list.Add(targetName, $"{cliCommandName} {targetName}");
                    }
                }
            }

            bool isNuke = cliCommandName == NUKE_CLI_COMMAND;
            string[] alwaysTasks = NukeAlwaysTasks;

            // Only fill default tasks if any scripts are found
            foreach (string reserved in alwaysTasks)
            {
                if (!list.ContainsKey(reserved))
                {
                    list.Add(reserved, $"{cliCommandName} {reserved}");
                }
            }

            AddMissingDefaultParents(list, cliCommandName, isNuke);
        }
        catch (Exception ex)
        {
            Debug.Write(ex);
        }

        return list;
    }

    private static void AddMissingDefaultParents(
        SortedList<string, string> list,
        string cliCommandName,
        bool isNuke
    )
    {
        string[] defaultTasks = NukeDefaultTasks;

        string[] prefixes =
        {
            PRE_SCRIPT_PREFIX,
            POST_SCRIPT_PREFIX,
        };

        var newParents = new List<string>();

        foreach (string task in list.Keys)
        foreach (string prefix in prefixes)
        {
            if (task.Length <= prefix.Length)
            {
                continue;
            }

            string parent = task.Substring(prefix.Length);

            if (!newParents.Contains(parent) &&
                task.StartsWith(prefix, StringComparison.Ordinal) &&
                !list.ContainsKey(parent) &&
                defaultTasks.Contains(parent))
            {
                newParents.Add(parent);
            }
        }

        foreach (string parent in newParents)
        {
            string cmd = parent == "version"
                ? null
                : $"{cliCommandName} {parent}";
            list.Add(parent, cmd);
        }
    }
}
