#if UNITY_EDITOR
using System;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Formats log entries for display and clipboard operations.
    /// </summary>
    internal static class LogEntryFormatter
    {
        private const int MAX_DISPLAY_LENGTH = 200;
        private static readonly char[] NEWLINE_CHARS = { '\r', '\n' };

        private static readonly CachedIcon ERROR_ICON = new("console.erroricon");
        private static readonly CachedIcon WARNING_ICON = new("console.warnicon");
        private static readonly CachedIcon INFO_ICON = new("console.infoicon");

        public static string FormatMessage(LogEntry log)
        {
            return $"[{log.type}] {log.message}";
        }

        public static string FormatWithStackTrace(LogEntry log)
        {
            if (string.IsNullOrEmpty(log.stackTrace))
                return FormatMessage(log);

            return FormatMessage(log) + Environment.NewLine + log.stackTrace;
        }

        public static string FormatTime(LogEntry log, TimeDisplayMode mode)
        {
            switch (mode)
            {
                case TimeDisplayMode.WallClock:
                    // Logs restored from before this field existed have no wall-clock time.
                    return log.wallClockTicks > 0
                        ? new DateTime(log.wallClockTicks).ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)
                        : "--:--:--.---";
                case TimeDisplayMode.Frame:
                    return "#" + log.frameCount.ToString(CultureInfo.InvariantCulture);
                default:
                    return FormatTimeDisplay(log.realtimeSinceStartup);
            }
        }

        public static string GetTimeSample(TimeDisplayMode mode)
        {
            return mode switch
            {
                TimeDisplayMode.WallClock => "00:00:00.000",
                TimeDisplayMode.Frame => "#0000000",
                _ => "00:00.000"
            };
        }

        public static string FormatTimeDisplay(float realtimeSinceStartup)
        {
            int minutes = (int)(realtimeSinceStartup / 60f);
            int seconds = (int)(realtimeSinceStartup % 60f);
            int milliseconds = (int)((realtimeSinceStartup * 1000f) % 1000f);
            return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }

        // The first maxLines lines of a message, each cut at 200 characters. A \r\n pair is one line break.
        public static string TruncateMessage(string message, int maxLines = 1)
        {
            if (string.IsNullOrEmpty(message))
                return "";

            int firstLineEnd = message.IndexOfAny(NEWLINE_CHARS);
            if (firstLineEnd < 0)
                return TruncateLine(message, 0, message.Length);

            return maxLines <= 1 ? TruncateLine(message, 0, firstLineEnd) : JoinLines(message, maxLines);
        }

        private static string JoinLines(string message, int maxLines)
        {
            StringBuilder builder = new();
            int start = 0;
            for (int line = 0; line < maxLines; line++)
            {
                int end = message.IndexOfAny(NEWLINE_CHARS, start);
                if (line > 0)
                    builder.Append('\n');
                builder.Append(TruncateLine(message, start, end < 0 ? message.Length : end));
                if (end < 0)
                    break;

                start = GetNextLineStart(message, end);
            }

            return builder.ToString();
        }

        private static int GetNextLineStart(string message, int lineBreak)
        {
            bool isCrLf = message[lineBreak] == '\r' && lineBreak + 1 < message.Length &&
                          message[lineBreak + 1] == '\n';
            return isCrLf ? lineBreak + 2 : lineBreak + 1;
        }

        private static string TruncateLine(string message, int start, int end)
        {
            int length = end - start;
            return length > MAX_DISPLAY_LENGTH
                ? message.Substring(start, MAX_DISPLAY_LENGTH) + "..."
                : message.Substring(start, length);
        }

        public static Texture2D GetIconForLogType(LogType type)
        {
            return type switch
            {
                LogType.Error or LogType.Exception or LogType.Assert => ERROR_ICON.Get(),
                LogType.Warning => WARNING_ICON.Get(),
                _ => INFO_ICON.Get()
            };
        }
    }
}
#endif
