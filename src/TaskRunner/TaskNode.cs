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
        => base.Invoke(context);

    private readonly bool _isNuke;
}
