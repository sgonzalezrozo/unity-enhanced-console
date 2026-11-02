#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
    [Serializable]
    internal class LogEntry
    {
        public string Message;
        public string StackTrace;
        public LogType Type;
        public DateTime Timestamp;
        public float RealtimeSinceStartup;
        public int Count;
        public int ContextGameObjectInstanceId;
    }
}
#endif

