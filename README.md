# NUKE Task Runner

Download the extension at the
[VS Gallery](https://visualstudiogallery.msdn.microsoft.com/)
or get the
[nightly build](http://vsixgallery.com/)

---------------------------------------------------------

Adds support for nuke scripts defined in Build.cs 
directly in Visual Studio's Task Runner Explorer.

## nuke scripts

Inside Build.cs it is possible to define custom targets inside
the `Build.cs` file.

```csharp
private Target Publish => _ => _.DependsOn(Compile)
    .Executes(
        () =>
        {
            DotNetPublishSettings Configurator(DotNetPublishSettings s)
                => s.SetProject(Solution)
                    .SetOutput(Build.PublishDirectory)
                    .SetPublishSingleFile(SingleFile)
                    .SetConfiguration(Configuration);

            DotNetTasks.DotNetPublish(Configurator);
        }
    );
```

## Task Runner Explorer

Open Task Runner Explorer by right-clicking the `Build.cs`
file and select **Task Runner Explorer** from the context menu:

![Open Task Runner Explorer](art/open-trx.png)

### Execute Targets

When scripts are specified, the Task Runner Explorer
will show those scripts.

![Task list](art/task-list.png)

Each script can be executed by double-clicking the task.

![Console](art/console.png)

### Verbose output

A button for turning verbose output on and off is located
at the left toolbar.

![Verbose Output](art/verbose-output.png)

The button is a toggle button that can be left
on or off for as long as needed.

### Bindings

Script bindings make it possible to associate individual scripts
with Visual Studio events such as "After build" etc.

![Visual Studio bindings](art/bindings.png)

## License

[Apache 2.0](LICENSE)
