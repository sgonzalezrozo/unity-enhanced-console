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

        // Derived values, computed once per entry instead of on every GUI event.
        [NonSerialized] private string displayText;
        [NonSerialized] private string timeText;

        // Section match cache. Valid while SectionMatchVersion equals SectionedLogOrganizer.MatchVersion.
        [NonSerialized] internal int SectionIndex;
        [NonSerialized] internal int SectionMatchVersion;

        internal string DisplayText => displayText ??= LogEntryFormatter.TruncateMessage(Message);

        internal string TimeText => timeText ??= LogEntryFormatter.FormatTimeDisplay(RealtimeSinceStartup);

        internal bool IsError => Type is LogType.Error or LogType.Exception or LogType.Assert;
    }
}
#endif
