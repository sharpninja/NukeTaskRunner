namespace NukeTaskRunner.TaskRunner.Helpers;

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

public static class TextViewUtil
{
    public static IVsTextView FindTextViewFor(string filePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        IVsWindowFrame frame = FindWindowFrame(filePath);

        if (frame == null)
        {
            return null;
        }

        return GetTextViewFromFrame(frame, out IVsTextView textView)
            ? textView
            : null;
    }

    private static IEnumerable<IVsWindowFrame> EnumerateDocumentWindowFrames()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (Package.GetGlobalService(typeof(SVsUIShell)) is not IVsUIShell shell)
        {
            yield break;
        }

        int hr = shell.GetDocumentWindowEnum(out IEnumWindowFrames framesEnum);

        if ((hr != VSConstants.S_OK) ||
            (framesEnum == null))
        {
            yield break;
        }

        var frames = new IVsWindowFrame[1];

        while ((framesEnum.Next(1, frames, out uint fetched) == VSConstants.S_OK) &&
               (fetched == 1))
        {
            yield return frames[0];
        }
    }

    private static IVsWindowFrame FindWindowFrame(string filePath)
    {
        foreach (IVsWindowFrame currentFrame in EnumerateDocumentWindowFrames())
        {
            if (IsFrameForFilePath(currentFrame, filePath))
            {
                return currentFrame;
            }
        }

        return null;
    }

    private static bool GetPhysicalPathFromFrame(IVsWindowFrame frame, out string frameFilePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        int hr = frame.GetProperty(
            (int)__VSFPROPID.VSFPROPID_pszMkDocument,
            out object propertyValue
        );

        if ((hr == VSConstants.S_OK) &&
            (propertyValue != null))
        {
            frameFilePath = propertyValue.ToString();

            return true;
        }

        frameFilePath = null;

        return false;
    }

    private static bool GetTextViewFromFrame(IVsWindowFrame frame, out IVsTextView textView)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        textView = VsShellUtilities.GetTextView(frame);

        return textView is not null;
    }

    private static bool IsFrameForFilePath(IVsWindowFrame frame, string filePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        return GetPhysicalPathFromFrame(frame, out string frameFilePath) &&
               string.Equals(filePath, frameFilePath, StringComparison.OrdinalIgnoreCase);
    }
}
