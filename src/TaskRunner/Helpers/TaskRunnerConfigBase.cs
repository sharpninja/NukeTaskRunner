namespace NukeTaskRunner.TaskRunner.Helpers;

using System;
using System.Windows.Media;

using Microsoft.VisualStudio.TaskRunnerExplorer;

public abstract class TaskRunnerConfigBase : ITaskRunnerConfig
{
    protected internal ITaskRunnerCommandContext Context
    {
        get;
    }

    /// <summary>
    ///     TaskRunner icon
    /// </summary>
    public virtual ImageSource Icon => TaskRunnerConfigBase._sharedIcon ??= LoadRootNodeIcon();

    public ITaskRunnerNode TaskHierarchy { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string LoadBindings(string configPath)
    {
        try
        {
            return BindingsPersister.Load(configPath);
        }
        catch
        {
            return "<binding />";
        }
    }

    public bool SaveBindings(string configPath, string bindingsXml)
    {
        try
        {
            return BindingsPersister.Save(configPath, bindingsXml);
        }
        catch
        {
            return false;
        }
    }

    protected TaskRunnerConfigBase(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy)
    {
        Context = context;
        TaskHierarchy = hierarchy;
    }

    protected virtual void Dispose(bool isDisposing)
    {
    }

    protected virtual ImageSource LoadRootNodeIcon()
        => null;

    private static ImageSource _sharedIcon;
}
