// ReSharper disable RedundantVerbatimStringPrefix
namespace NukeTaskRunner.TaskRunner.Helpers;

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.VisualStudio.TaskRunnerExplorer;

public class TaskRunnerConfig : TaskRunnerConfigBase
{
    public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy)
        : base(context, hierarchy)
    {
    }

    public TaskRunnerConfig(
        ITaskRunnerCommandContext context,
        ITaskRunnerNode hierarchy,
        string cliCommandName
    )
        : this(context, hierarchy)
        => _cliCommandName = cliCommandName;

    protected override ImageSource LoadRootNodeIcon()
    {
        try
        {
            return new BitmapImage(
                new(
                    $@"pack://application:,,,/NukeTaskRunner;component/Resources/{_cliCommandName}.png"
                )
            );
        }
        catch
        {
            return default;
        }
    }

    private readonly string _cliCommandName;
}
