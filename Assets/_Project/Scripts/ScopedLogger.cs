using System;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class ScopedLogger : ILogger
{
    private readonly ILogger _baseLogger;
    public bool logEnabled { get; set; } = true;
    public LogType filterLogType { get; set; } = LogType.Log | LogType.Warning | LogType.Exception | LogType.Error | LogType.Assert;

    public Color TagColor { get; set; } = new Color(0.7f, 0.8f, 1f); // Light blue by default

    private string _tag;
    public string Tag
    {
        get => _tag;
        set
        {
            _tag = value;
            UpdateColoredTag();
        }
    }

    private string _coloredTag = "";

    public ScopedLogger(ILogger baseLogger, string tag = null)
    {
        _baseLogger = baseLogger ?? throw new ArgumentNullException(nameof(baseLogger));
        Tag = tag;
    }

    private void UpdateColoredTag()
    {
        if (string.IsNullOrEmpty(_tag))
        {
            _coloredTag = "";
            return;
        }

        string hex = ColorUtility.ToHtmlStringRGB(TagColor);
        _coloredTag = $"<color=#{hex}>[{_tag}]</color>";
    }

    private string FormatMessage(LogType logType, object message)
    {
        if (!logEnabled) return null;

        string msg = message?.ToString() ?? "Null";

        // Format: [LogType] <color>[Tag]</color> message
        if (string.IsNullOrEmpty(_coloredTag))
            return msg;
        else
            return $"{_coloredTag} {msg}";
    }

    public void Log(LogType logType, object message)
    {
        if (logEnabled) _baseLogger.Log(logType, FormatMessage(logType, message));
    }

    public void Log(LogType logType, object message, Object context)
    {
        if (logEnabled) _baseLogger.Log(logType, message: FormatMessage(logType, message), context);
    }

    public void Log(LogType logType, string tag, object message)
    {
        if (logEnabled) _baseLogger.Log(logType, FormatMessage(logType, message));
    }

    public void Log(LogType logType, string tag, object message, Object context)
    {
        if (logEnabled) _baseLogger.Log(logType, message: FormatMessage(logType, message), context);
    }

    public void Log(object message) => Log(LogType.Log, message);
    public void Log(string tag, object message) => Log(LogType.Log, message);
    public void Log(string tag, object message, Object context) => Log(LogType.Log, message, context);

    public void LogWarning(string tag, object message) => Log(LogType.Warning, message);
    public void LogWarning(string tag, object message, Object context) => Log(LogType.Warning, message, context);

    public void LogError(string tag, object message) => Log(LogType.Error, message);
    public void LogError(string tag, object message, Object context) => Log(LogType.Error, message, context);

    public void LogException(Exception exception) => Log(LogType.Exception, exception);
    public void LogException(Exception exception, Object context) => Log(LogType.Exception, exception, context);

    public ILogHandler logHandler
    {
        get => _baseLogger.logHandler;
        set => _baseLogger.logHandler = value;
    }

    public bool IsLogTypeAllowed(LogType logType)
    {
        return logEnabled && (filterLogType.HasFlag(logType) || _baseLogger.IsLogTypeAllowed(logType));
    }

    public void LogFormat(LogType logType, string format, params object[] args)
    {
        if (!logEnabled) return;
        string message = string.Format(format, args);
        Log(logType, message);
    }

    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        if (!logEnabled) return;
        string message = string.Format(format, args);
        Log(logType, message: message, context);
    }
}