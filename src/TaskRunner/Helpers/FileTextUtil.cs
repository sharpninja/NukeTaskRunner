namespace NukeTaskRunner.TaskRunner.Helpers;

using System;
using System.IO;

internal class FileTextUtil : ITextUtil
{
    public Range CurrentLineRange => new()
    {
        _lineNumber = _lineNumber,
        _lineRange = new LineRange
        {
            _start = 0,
            _length = _currentLineLength,
        },
    };

    public bool Delete(Range range)
    {
        if (range._lineRange._length == 0)
        {
            return true;
        }

        string fileContents = File.ReadAllText(_filename);

        using StringReader reader = new(fileContents);

        using TextWriter writer = new StreamWriter(File.Open(_filename, FileMode.Create));

        if (SeekTo(
                reader,
                writer,
                range,
                out string lineText
            ))
        {
            writer.WriteLine(
                lineText.Substring(0, range._lineRange._start) +
                lineText.Substring(range._lineRange._start + range._lineRange._length)
            );
        }

        lineText = reader.ReadLine();

        while (lineText != null)
        {
            writer.WriteLine(lineText);
            lineText = reader.ReadLine();
        }

        return true;
    }

    public void FormatRange(LineRange range)
    {
    }

    public bool Insert(Range range, string text, bool addNewline)
    {
        if (text.Length == 0)
        {
            return true;
        }

        string fileContents = File.ReadAllText(_filename);

        using StringReader reader = new(fileContents);

        using TextWriter writer = new StreamWriter(File.Open(_filename, FileMode.Create));

        if (SeekTo(
                reader,
                writer,
                range,
                out string lineText
            ))
        {
            writer.WriteLine(
                lineText.Substring(0, range._lineRange._start) +
                text +
                (addNewline
                    ? Environment.NewLine
                    : string.Empty) +
                lineText.Substring(range._lineRange._start)
            );
        }

        lineText = reader.ReadLine();

        while (lineText != null)
        {
            writer.WriteLine(lineText);
            lineText = reader.ReadLine();
        }

        return true;
    }

    public string ReadAllText()
        => File.ReadAllText(_filename);

    public void Reset()
    {
        _lineNumber = 0;
    }

    public bool TryReadLine(out string line)
    {
        line = null;
        Stream stream = File.OpenRead(_filename);

        using TextReader reader = new StreamReader(stream);

        int lineCount = _lineNumber;

        for (var i = 0; i < lineCount + 1; ++i)
        {
            line = reader.ReadLine();
        }

        if (line != null)
        {
            _currentLineLength = line.Length;
            ++_lineNumber;

            return true;
        }

        _currentLineLength = 0;

        return false;
    }

    public FileTextUtil(string filename)
        => _filename = filename;

    private int _currentLineLength;
    private readonly string _filename;
    private int _lineNumber;

    private bool SeekTo(
        TextReader reader,
        TextWriter writer,
        Range range,
        out string lineText
    )
    {
        var success = true;

        for (var lineNumber = 0; lineNumber < range._lineNumber; ++lineNumber)
        {
            string line = reader.ReadLine();

            if (line != null)
            {
                writer.WriteLine(line);
            }
            else
            {
                success = false;

                break;
            }
        }

        lineText = reader.ReadLine();

        if (!success)
        {
            return false;
        }

        if ((lineText == null) || (lineText.Length >= range._lineRange._start))
        {
            return true;
        }

        writer.WriteLine(lineText);

        return false;
    }
}
