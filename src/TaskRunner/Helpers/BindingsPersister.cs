namespace NukeTaskRunner.TaskRunner.Helpers;

using System;
using System.Linq;
using System.Xml.Linq;

using Microsoft.VisualStudio.TextManager.Interop;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class BindingsPersister
{
    public static void GetExtentInfo(
        this ITextUtil textUtil,
        int startIndex,
        int length,
        out int startLine,
        out int startLineOffset,
        out int endLine,
        out int endLineOffset
    )
    {
        textUtil.Reset();
        int lineNumber = 0,
            charCount = 0,
            lineCharCount = 0;
        string line;

        while (textUtil.TryReadLine(out line) &&
               (charCount < startIndex))
        {
            ++lineNumber;
            charCount += line.Length;
            lineCharCount = line.Length;
        }

        //We passed the line we want to be on, so back up
        int positionAtEndOfPreviousLine = charCount - lineCharCount;
        startLineOffset = startIndex - positionAtEndOfPreviousLine;
        startLine = lineNumber - 1;

        while (textUtil.TryReadLine(out line) &&
               (charCount < startIndex + length))
        {
            ++lineNumber;
            charCount += line.Length;
            lineCharCount = line.Length;
        }

        if (line != null)
        {
            positionAtEndOfPreviousLine = charCount - lineCharCount;
            endLineOffset = startIndex + length - positionAtEndOfPreviousLine;
        }
        else
        {
            endLineOffset = lineCharCount;
        }

        endLine = lineNumber - 1;
    }

    public static string Load(string configPath)
    {
        IVsTextView configTextView = TextViewUtil.FindTextViewFor(configPath);
        ITextUtil textUtil;

        if (configTextView != null)
        {
            textUtil = new VsTextViewTextUtil(configTextView);
        }
        else
        {
            textUtil = new FileTextUtil(configPath);
        }

        string fileText = textUtil.ReadAllText();
        JObject body = JObject.Parse(fileText);

        JObject bindings = body[BINDINGS_NAME] as JObject;

        if (bindings != null)
        {
            XElement bindingsElement = XElement.Parse("<binding />");

            foreach (JProperty property in bindings.Properties())
            {
                string[] tasks = property.Value.Values<string>()
                    .ToArray();
                bindingsElement.SetAttributeValue(property.Name, string.Join(",", tasks));
            }

            return bindingsElement.ToString();
        }

        return "<binding />";
    }

    public static bool Save(string configPath, string bindingsXml)
    {
        XElement bindingsXmlObject = XElement.Parse(bindingsXml);
        JObject bindingsXmlBody = JObject.Parse(@"{}");
        var anyAdded = false;

        foreach (XAttribute attribute in bindingsXmlObject.Attributes())
        {
            JArray type = new();
            bindingsXmlBody[attribute.Name.LocalName] = type;
            string[] tasks = attribute.Value.Split(',');

            foreach (string task in tasks)
            {
                anyAdded = true;
                type.Add(task.Trim());
            }
        }

        IVsTextView configTextView = TextViewUtil.FindTextViewFor(configPath);
        ITextUtil textUtil;

        if (configTextView != null)
        {
            textUtil = new VsTextViewTextUtil(configTextView);
        }
        else
        {
            textUtil = new FileTextUtil(configPath);
        }

        string currentContents = textUtil.ReadAllText();
        JObject fileModel = JObject.Parse(currentContents);
        bool commaRequired = fileModel.Properties()
            .Any();
        JProperty currentBindings = fileModel.Property(BINDINGS_NAME);
        bool insert = currentBindings == null;
        fileModel[BINDINGS_NAME] = bindingsXmlBody;

        JProperty property = fileModel.Property(BINDINGS_NAME);
        var bindingText = property.ToString(Formatting.None);
        textUtil.Reset();

        if (!anyAdded)
        {
            insert = false;
        }

        if (insert)
        {
            if (commaRequired)
            {
                bindingText = ", " + bindingText;
            }

            int lineNumber = 0,
                candidateLine = -1,
                lastBraceIndex = -1,
                characterCount = 0;

            while (textUtil.TryReadLine(out string line))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    int brace = line.LastIndexOf('}');

                    if (brace >= 0)
                    {
                        lastBraceIndex = brace;
                        candidateLine = lineNumber;
                    }
                }

                characterCount += line?.Length ?? 0;
                ++lineNumber;
            }

            if ((candidateLine >= 0) &&
                (lastBraceIndex >= 0))
            {
                if (textUtil.Insert(
                        new Range
                        {
                            _lineNumber = candidateLine,
                            _lineRange = new LineRange
                            {
                                _start = lastBraceIndex,
                                _length = bindingText.Length,
                            },
                        },
                        bindingText,
                        true
                    ))
                {
                    textUtil.FormatRange(
                        new LineRange
                        {
                            _start = characterCount,
                            _length = bindingText.Length,
                        }
                    );

                    return true;
                }
            }

            return false;
        }

        int bindingsIndex = currentContents.IndexOf(
            @"""" + BINDINGS_NAME + @"""",
            StringComparison.Ordinal
        );
        int closeBindingsBrace = currentContents.IndexOf('}', bindingsIndex) + 1;
        int length = closeBindingsBrace - bindingsIndex;

        textUtil.GetExtentInfo(
            bindingsIndex,
            length,
            out int startLine,
            out int startLineOffset,
            out int endLine,
            out int endLineOffset
        );

        if (!anyAdded)
        {
            int previousComma = currentContents.LastIndexOf(',', bindingsIndex);
            var tail = 0;

            if (previousComma > -1)
            {
                tail += bindingsIndex - previousComma;
                textUtil.GetExtentInfo(
                    previousComma,
                    length,
                    out startLine,
                    out startLineOffset,
                    out endLine,
                    out endLineOffset
                );
            }

            if (textUtil.Delete(
                    new Range
                    {
                        _lineNumber = startLine,
                        _lineRange = new LineRange
                        {
                            _start = startLineOffset,
                            _length = length + tail,
                        },
                    }
                ))
            {
                textUtil.Reset();
                textUtil.FormatRange(
                    new LineRange
                    {
                        _start = bindingsIndex,
                        _length = 2,
                    }
                );

                return true;
            }

            return false;
        }

        bool success = textUtil.Replace(
            new Range
            {
                _lineNumber = startLine,
                _lineRange = new LineRange
                {
                    _start = startLineOffset,
                    _length = length,
                },
            },
            bindingText
        );

        if (success)
        {
            textUtil.FormatRange(
                new LineRange
                {
                    _start = bindingsIndex,
                    _length = bindingText.Length,
                }
            );
        }

        return success;
    }

    private const string BINDINGS_NAME = "-vs-binding";
}
