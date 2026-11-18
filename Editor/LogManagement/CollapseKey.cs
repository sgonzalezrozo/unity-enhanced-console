#if UNITY_EDITOR
using System;
using UnityEngine;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// What makes two logs one collapse group: message and type, and the stack trace in exact mode.
    /// </summary>
    internal readonly struct CollapseKey : IEquatable<CollapseKey>
    {
        private const int HASH_MULTIPLIER = 397;

        private readonly string message;
        private readonly string stackTrace;
        private readonly LogType type;

        private CollapseKey(LogEntry entry, string groupStackTrace)
        {
            message = entry.message ?? "";
            stackTrace = groupStackTrace;
            type = entry.type;
        }

        public static CollapseKey ForMessage(LogEntry entry)
        {
            return new CollapseKey(entry, null);
        }

        public static CollapseKey ForMessageAndStackTrace(LogEntry entry)
        {
            return new CollapseKey(entry, entry.stackTrace ?? "");
        }

        public bool Equals(CollapseKey other)
        {
            return type == other.type &&
                   string.Equals(message, other.message, StringComparison.Ordinal) &&
                   string.Equals(stackTrace, other.stackTrace, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
            => obj is CollapseKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = message.GetHashCode();
                hash = (hash * HASH_MULTIPLIER) ^ (int)type;
                if (stackTrace != null)
                    hash = (hash * HASH_MULTIPLIER) ^ stackTrace.GetHashCode();
                return hash;
            }
        }
    }
}
#endif
