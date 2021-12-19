namespace NukeTaskRunner;

using System.Linq;

internal class Constants
{
    public const string FILENAME = "Build.cs";

    public static readonly string[] NukeAlwaysTasks =
    {
        "Clean", "Restore", "Compile", "Test",
    };

    public static readonly string[] NukeDefaultTasks =
    {
        "Start",
    };

    public static readonly string[] NukeAllDefaultTasks
        = NukeAlwaysTasks
            .Union(NukeDefaultTasks)
            .ToArray();

    public const string NUKE_CLI_COMMAND = @"..\build.cmd";

    public const string NUKE_VERBOSE_OPTION = "-verbose";
    public const string POST_SCRIPT_PREFIX = "post";
    public const string PRE_SCRIPT_PREFIX = "pre";
}
