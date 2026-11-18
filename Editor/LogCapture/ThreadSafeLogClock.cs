#if UNITY_EDITOR
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace EnhancedConsole.Editor.LogCapture
{
    /// <summary>
    /// Log times that can be read from any thread. Unity's Time can only be read on the main thread, so other
    /// threads use a stopwatch that the main thread keeps in step with Time.realtimeSinceStartup.
    /// </summary>
    internal sealed class ThreadSafeLogClock
    {
        private static readonly Stopwatch STOPWATCH = Stopwatch.StartNew();

        private double realtimeOffset;
        private volatile int lastFrameCount;

        // Main thread only.
        public void Synchronize()
        {
            Volatile.Write(ref realtimeOffset, Time.realtimeSinceStartupAsDouble - STOPWATCH.Elapsed.TotalSeconds);
            lastFrameCount = Time.frameCount;
        }

        public float GetRealtimeSinceStartup()
        {
            if (LogContextHelper.IsMainThread)
                return Time.realtimeSinceStartup;

            return (float)(STOPWATCH.Elapsed.TotalSeconds + Volatile.Read(ref realtimeOffset));
        }

        public int GetFrameCount()
        {
            return LogContextHelper.IsMainThread ? Time.frameCount : lastFrameCount;
        }
    }
}
#endif
