#if UNITY_EDITOR
using System;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    [Serializable]
    internal class LogEntry
    {
        public string Message;
        public string StackTrace;
        public LogType Type;
        public float RealtimeSinceStartup;
        public int Count;
        public int ContextGameObjectInstanceId;
        public long WallClockTicks;
        public int FrameCount;

        /// <summary>Path of the assembly whose compilation reported this message; empty for regular logs.</summary>
        public string CompilerAssembly;

        // Derived values, computed once per entry (and per display setting) instead of on every GUI event.
        [NonSerialized] private string displayText;
        [NonSerialized] private int displayTextLines;
        [NonSerialized] private string timeText;
        [NonSerialized] private TimeDisplayMode timeTextMode;

        // Section match cache. Valid while SectionMatchVersion equals SectionedLogOrganizer.MatchVersion.
        [NonSerialized] internal int SectionIndex;
        [NonSerialized] internal int SectionMatchVersion;

        internal string GetDisplayText(int maxLines)
        {
            if (displayText == null || displayTextLines != maxLines)
            {
                displayText = LogEntryFormatter.TruncateMessage(Message, maxLines);
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

        internal bool IsError => Type is LogType.Error or LogType.Exception or LogType.Assert;

        internal bool IsCompilerMessage => !string.IsNullOrEmpty(CompilerAssembly);

        internal bool IsCompilerError => IsCompilerMessage && Type == LogType.Error;
    }
}
#endif
