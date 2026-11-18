#if UNITY_EDITOR
using UnityEngine;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// A log as received, with the times it was written. Logs from background threads are added later on the
    /// main thread, so their times are captured when they arrive.
    /// </summary>
    internal struct CapturedLog
    {
        public string message;
        public string stackTrace;
        public LogType type;
        public int contextInstanceId;
        public float realtimeSinceStartup;
        public long wallClockTicks;
        public int frameCount;
    }
}
#endif
