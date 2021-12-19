namespace NukeTaskRunner.TaskRunner.Helpers;

public interface ITextUtil
{
    Range CurrentLineRange { get; }

    bool Delete(Range range);

    void FormatRange(LineRange range);

    bool Insert(Range position, string text, bool addNewline);

    string ReadAllText();

    void Reset();

    bool TryReadLine(out string line);
}
