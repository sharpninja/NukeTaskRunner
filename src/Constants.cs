namespace NukeTaskRunner;

using System.Linq;

internal class Constants
{
    public const string FILENAME = "Build.cs";

    public static readonly string[] NukeAlwaysTasks =
    {
        "build", "clean", "test",
    };

    public static readonly string[] NukeDefaultTasks =
    {
        "start", "version",
    };

    public static readonly string[] NukeAllDefaultTasks
        = NukeAlwaysTasks
            .Union(NukeDefaultTasks)
            .ToArray();

    public const string NUKE_CLI_COMMAND = "nuke";

    public const string NUKE_VERBOSE_OPTION = "-verbose";
    public const string POST_SCRIPT_PREFIX = "post";
    public const string PRE_SCRIPT_PREFIX = "pre";
}
