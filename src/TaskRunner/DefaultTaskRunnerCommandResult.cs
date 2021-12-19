namespace NukeTaskRunner.TaskRunner;

using Microsoft.VisualStudio.TaskRunnerExplorer;

public class DefaultTaskRunnerCommandResult : ITaskRunnerCommandResult
{
    public DefaultTaskRunnerCommandResult(
        int exitCode,
        string standardOutput,
        string standardError)
    {
        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    public int ExitCode
    {
        get;
    }

    public string StandardOutput
    {
        get;
    }

    public string StandardError
    {
        get;
    }
}
