namespace NukeTaskRunner.TaskRunner;

using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TaskRunnerExplorer;

public class TaskNode : TaskRunnerNode
{
    public TaskNode(string name, bool invokable, bool isNuke)
        : base(name, invokable)
        => _isNuke = isNuke;

    public override Task<ITaskRunnerCommandResult> Invoke(
        ITaskRunnerCommandContext context)
    {
        // if the CLI is Yarn and the Verbose option is enabled, set the verbose option correctly
        if (!_isNuke)
        {
            return Task.FromResult(
                new DefaultTaskRunnerCommandResult(
                    0,
                    string.Empty,
                    string.Empty) as ITaskRunnerCommandResult
            );
        }

        return base.Invoke(context);
    }

    private readonly bool _isNuke;
}
