namespace NukeTaskRunner;

using System.Linq;

internal class Constants
{
    public const string FILENAME = "Build.cs";

    public static string[] NukeAlwaysTasks { get; } =
    {
        "Clean", "Restore", "Compile", "Test",
    };

    public static string[] NukeDefaultTasks { get; } =
    {
        "Start",
    };

    public static string[] NukeAllDefaultTasks { get; }
        = NukeAlwaysTasks
            .Union(NukeDefaultTasks)
            .ToArray();

    // ReSharper disable once StringLiteralTypo
    public const string NUKE_CLI_COMMAND = @"..\build.cmd";

    public const string NUKE_VERBOSE_OPTION = "-verbose";
    public const string POST_SCRIPT_PREFIX = "post";
    public const string PRE_SCRIPT_PREFIX = "pre";
}
