#if UNITY_EDITOR
using System;
using UnityEngine;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// How many logs of each level a view holds. Exceptions and failed asserts count as errors.
    /// </summary>
    internal sealed class LogLevelCounts
    {
        private const int INFO = 0;
        private const int WARNING = 1;
        private const int ERROR = 2;

        private readonly int[] counts = new int[3];

        public int Info => counts[INFO];
        public int Warning => counts[WARNING];
        public int Error => counts[ERROR];

        public void Add(LogType type)
        {
            counts[GetLevel(type)]++;
        }

        public void Remove(LogType type)
        {
            counts[GetLevel(type)]--;
        }

        public void Clear()
        {
            Array.Clear(counts, 0, counts.Length);
        }

        private static int GetLevel(LogType type)
        {
            return type switch
            {
                LogType.Log => INFO,
                LogType.Warning => WARNING,
                _ => ERROR
            };
        }
    }
}
#endif
