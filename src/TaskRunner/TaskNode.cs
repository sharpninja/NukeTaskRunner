// ReSharper disable NotAccessedField.Local
namespace NukeTaskRunner.TaskRunner;

using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TaskRunnerExplorer;

public class TaskNode : TaskRunnerNode
{
    private readonly bool _isNuke;

    public TaskNode(string name, bool invokable, bool isNuke)
        : base(name, invokable)
        => _isNuke = isNuke;

    public override Task<ITaskRunnerCommandResult> Invoke(
        ITaskRunnerCommandContext context)
        => base.Invoke(context);
}
