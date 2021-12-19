namespace NukeTaskRunner.TaskRunner.Helpers;

using System;
using System.Text;

using EnvDTE;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

internal class VsTextViewTextUtil : ITextUtil
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
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            GetEditPointForRange(range)
                ?.Delete(range._lineRange._length);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void FormatRange(LineRange range)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        Reset();
        this.GetExtentInfo(
            range._start,
            range._length,
            out int startLine,
            out int startLineOffset,
            out int endLine,
            out int endLineOffset
        );

        _view.GetSelection(
            out int oldStartLine,
            out int oldStartLineOffset,
            out int oldEndLine,
            out int oldEndLineOffset
        );
        _view.SetSelection(
            startLine,
            startLineOffset,
            endLine,
            endLineOffset
        );
        IOleCommandTarget target
            = (IOleCommandTarget)ServiceProvider.GlobalProvider.GetService(
                typeof(SUIHostCommandDispatcher)
            );

        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        Guid cmdid = VSConstants.VSStd2K;

        _view.SendExplicitFocus();
        target.Exec(
            ref cmdid,
            (uint)VSConstants.VSStd2KCmdID.FORMATSELECTION,
            0,
            IntPtr.Zero,
            IntPtr.Zero
        );

        _view.SetSelection(
            oldStartLine,
            oldStartLineOffset,
            oldEndLine,
            oldEndLineOffset
        );
    }

    public bool Insert(Range position, string text, bool addNewline)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            if (text is not null)
            {
                GetEditPointForRange(position)
                    ?.Insert(
                        text +
                        (addNewline
                            ? Environment.NewLine
                            : string.Empty)
                    );
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string ReadAllText()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        StringBuilder text = new();

        while (TryReadLine(out string line))
        {
            text.Append(line);
        }

        return text.ToString();
    }

    public void Reset()
    {
        _currentLineLength = 0;
        _lineNumber = 0;
    }

    public bool TryReadLine(out string line)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        int hr = _view.GetBuffer(out IVsTextLines textLines);

        if ((hr != VSConstants.S_OK) ||
            (textLines == null))
        {
            line = null;

            return false;
        }

        hr = textLines.GetLineCount(out int lineCount);

        if ((hr != VSConstants.S_OK) ||
            (_lineNumber == lineCount))
        {
            line = null;

            return false;
        }

        int lineNumber = _lineNumber++;
        hr = textLines.GetLengthOfLine(lineNumber, out _currentLineLength);

        if (hr != VSConstants.S_OK)
        {
            line = null;

            return false;
        }

        hr = textLines.GetLineText(
            lineNumber,
            0,
            lineNumber,
            _currentLineLength,
            out line
        );

        if (hr != VSConstants.S_OK)
        {
            line = null;

            return false;
        }

        var lineData = new LINEDATA[1];
        textLines.GetLineData(lineNumber, lineData, null);

        if (lineData[0]
                .iEolType !=
            EOLTYPE.eolNONE)
        {
            line += "\n";
        }

        return true;
    }

    public VsTextViewTextUtil(IVsTextView view)
        => _view = view;

    private int _currentLineLength;
    private int _lineNumber;
    private readonly IVsTextView _view;

    private EditPoint GetEditPointForRange(Range range)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        int hr = _view.GetBuffer(out IVsTextLines textLines);

        if ((hr != VSConstants.S_OK) ||
            (textLines == null))
        {
            return null;
        }

        hr = textLines.CreateEditPoint(
            range._lineNumber,
            range._lineRange._start,
            out object editPointObject
        );

        if ((hr != VSConstants.S_OK) ||
            editPointObject is not EditPoint editPoint)
        {
            return null;
        }

        return editPoint;
    }
}
