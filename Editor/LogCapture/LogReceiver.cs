#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using EnhancedConsole.Editor.LogManagement;
using EnhancedConsole.Editor.Window;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.LogCapture
{
    /// <summary>
    /// Receives logs from every thread. Main-thread logs are added at once; logs from other threads are queued
    /// with the times they were written and added on the next Editor update.
    /// </summary>
    internal sealed class LogReceiver
    {
        private readonly ConsoleContext context;
        private readonly ThreadSafeLogClock clock = new();
        private readonly ConcurrentQueue<CapturedLog> pendingLogs = new();

        public LogReceiver(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void Register()
        {
            LogContextHelper.Initialize();
            LogContextHelper.ClearLogContext();
            clock.Synchronize();
            // The threaded event receives logs from every thread, the main thread included.
            Application.logMessageReceivedThreaded += OnLogReceived;
        }

        public void Unregister()
        {
            Application.logMessageReceivedThreaded -= OnLogReceived;
        }

        // Main thread only.
        public void Update()
        {
            clock.Synchronize();
            DrainPendingLogs();
        }

        // Called on whichever thread logged.
        private void OnLogReceived(string message, string stackTrace, LogType type)
        {
            CapturedLog log = Capture(message, stackTrace, type);
            if (!LogContextHelper.IsMainThread)
            {
                pendingLogs.Enqueue(log);
                context.Repaints.Request();
                return;
            }

            // Keep order: add anything queued from other threads first.
            DrainPendingLogs();
            Add(log);
        }

        private CapturedLog Capture(string message, string stackTrace, LogType type)
        {
            return new CapturedLog
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
                // The context handler runs just before this callback on the same thread.
                contextInstanceId = LogContextHelper.ConsumeLogContextInstanceId(),
                realtimeSinceStartup = clock.GetRealtimeSinceStartup(),
                wallClockTicks = DateTime.Now.Ticks,
                frameCount = clock.GetFrameCount()
            };
        }

        private void DrainPendingLogs()
        {
            while (pendingLogs.TryDequeue(out CapturedLog log))
                Add(log);
        }

        private void Add(CapturedLog log)
        {
            context.Logs.AddLog(log);
            context.Repaints.Request();
            PauseOnError(log.type);
        }

        // Like Unity's Error Pause: errors, exceptions and failed asserts pause Play Mode.
        private void PauseOnError(LogType type)
        {
            bool isError = type is LogType.Error or LogType.Exception or LogType.Assert;
            if (isError && context.Preferences.ErrorPause.Value && EditorApplication.isPlaying &&
                !EditorApplication.isPaused)
                EditorApplication.isPaused = true;
        }
    }
}
#endif
