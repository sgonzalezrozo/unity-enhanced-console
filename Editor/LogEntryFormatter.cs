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

        // The first maxLines lines of a message, each cut at 200 characters.
        public static string TruncateMessage(string message, int maxLines = 1)
        {
            if (string.IsNullOrEmpty(message))
                return "";

            maxLines = Math.Max(1, maxLines);
            StringBuilder builder = null;
            int start = 0;

            for (int line = 0; line < maxLines && start <= message.Length; line++)
            {
                int end = message.IndexOfAny(NEWLINE_CHARS, start);
                bool isLast = end < 0;
                if (isLast)
                    end = message.Length;

                string text = message.Substring(start, end - start);
                if (text.Length > MAX_DISPLAY_LENGTH)
                    text = text.Substring(0, MAX_DISPLAY_LENGTH) + "...";

                if (line == 0 && (isLast || maxLines == 1))
                    return text;

                builder ??= new StringBuilder();
                if (line > 0)
                    builder.Append('\n');
                builder.Append(text);

                if (isLast)
                    break;

                // Treat \r\n as one line break.
                start = end + 1;
                if (message[end] == '\r' && start < message.Length && message[start] == '\n')
                    start++;
            }

            return builder?.ToString() ?? "";
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
