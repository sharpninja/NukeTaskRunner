namespace NukeTaskRunner.TaskRunner.Helpers;

public static class TextUtilExtensions
{
    public static bool Replace(this ITextUtil util, Range range, string text)
        //Deletes the current bindings elements and move it to the top of the file
        => util.Delete(range) && util.Insert(range, text, false);
}
