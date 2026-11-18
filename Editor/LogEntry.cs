#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnhancedConsole.Editor
{
    [Serializable]
    internal class LogEntry
    {
        [FormerlySerializedAs("Message")]
        public string message;
        [FormerlySerializedAs("StackTrace")]
        public string stackTrace;
        [FormerlySerializedAs("Type")]
        public LogType type;
        [FormerlySerializedAs("RealtimeSinceStartup")]
        public float realtimeSinceStartup;
        [FormerlySerializedAs("Count")]
        public int count;
        [FormerlySerializedAs("ContextGameObjectInstanceId")]
        public int contextGameObjectInstanceId;
        [FormerlySerializedAs("WallClockTicks")]
        public long wallClockTicks;
        [FormerlySerializedAs("FrameCount")]
        public int frameCount;

        // Path of the assembly whose compilation reported this message; empty for regular logs.
        [FormerlySerializedAs("CompilerAssembly")]
        public string compilerAssembly;

        // Derived values, computed once per entry (and per display setting) instead of on every GUI event.
        [NonSerialized] private string displayText;
        [NonSerialized] private int displayTextLines;
        [NonSerialized] private string timeText;
        [NonSerialized] private TimeDisplayMode timeTextMode;

        // Section match cache. Valid while SectionMatchVersion equals SectionedLogOrganizer.MatchVersion.
        [NonSerialized] internal int sectionIndex;
        [NonSerialized] internal int sectionMatchVersion;

        internal string GetDisplayText(int maxLines)
        {
            if (displayText == null || displayTextLines != maxLines)
            {
                displayText = LogEntryFormatter.TruncateMessage(message, maxLines);
                displayTextLines = maxLines;
            }

            return displayText;
        }

        internal string GetTimeText(TimeDisplayMode mode)
        {
            if (timeText == null || timeTextMode != mode)
            {
                timeText = LogEntryFormatter.FormatTime(this, mode);
                timeTextMode = mode;
            }

            return timeText;
        }

        internal static LogEntry FromCapturedLog(LogManagement.CapturedLog log)
        {
            return new LogEntry
            {
                message = log.message,
                stackTrace = log.stackTrace,
                type = log.type,
                realtimeSinceStartup = log.realtimeSinceStartup,
                wallClockTicks = log.wallClockTicks,
                frameCount = log.frameCount,
                count = 1,
                contextGameObjectInstanceId = log.contextInstanceId
            };
        }

        internal bool IsError => type is LogType.Error or LogType.Exception or LogType.Assert;

        internal bool IsCompilerMessage => !string.IsNullOrEmpty(compilerAssembly);

        internal bool IsCompilerError => IsCompilerMessage && type == LogType.Error;
    }
}
#endif
