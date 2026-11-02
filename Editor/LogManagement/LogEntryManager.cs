#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// Manages log entry lifecycle: adding, clearing, filtering, and compression.
    /// Single Responsibility: Log storage and lifecycle only.
    /// </summary>
    internal class LogEntryManager
    {
        private const int MAX_LOG_ENTRIES = 10000;

        private readonly List<LogEntry> logEntries = new();
        private readonly List<LogEntry> originalLogEntries = new();
        private readonly LogFilterManager filterManager;

        public int TotalCount => logEntries.Count;
        public int OriginalCount => originalLogEntries.Count;

        public LogEntryManager(LogFilterManager filterManager)
        {
            this.filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
        }

	public void AddLog(string message, string stackTrace, LogType type)
	{
		int contextGameObjectId = TryExtractGameObjectContextId(message, stackTrace);


		LogEntry newEntry = new()
		{
			Message = message,
			StackTrace = stackTrace,
			Type = type,
			RealtimeSinceStartup = Time.realtimeSinceStartup,
			Count = 1,
			ContextGameObjectInstanceId = contextGameObjectId
		};

            originalLogEntries.Add(new LogEntry
            {
                Message = message,
                StackTrace = stackTrace,
                Type = type,
                RealtimeSinceStartup = Time.realtimeSinceStartup,
                Count = 1,
                ContextGameObjectInstanceId = contextGameObjectId
            });

            PruneOriginalIfExceedsLimit();

            if (filterManager.IsCollapseEnabled)
            {
                LogEntry existing = FindExistingEntry(newEntry);
                if (existing != null)
                {
                    existing.Count++;
                    return;
                }
            }

            logEntries.Add(newEntry);
            PruneVisibleIfExceedsLimit();
        }

        public void Clear()
        {
            logEntries.Clear();
            originalLogEntries.Clear();
        }

        public List<LogEntry> GetPersistableEntries()
        {
            return new List<LogEntry>(originalLogEntries);
        }

        public void Restore(List<LogEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            Clear();
            originalLogEntries.AddRange(entries);
            RebuildWithCollapse();
        }

        public void RebuildWithCollapse()
        {
            if (originalLogEntries.Count == 0)
                return;

            List<LogEntry> rebuiltEntries = new();

            if (filterManager.IsCollapseEnabled)
            {
                foreach (LogEntry entry in originalLogEntries)
                {
                    LogEntry existing = FindExistingInList(rebuiltEntries, entry);
                    if (existing != null)
                        existing.Count++;
                    else
                        rebuiltEntries.Add(CloneEntry(entry));
                }
            }
            else
            {
                rebuiltEntries = new List<LogEntry>(originalLogEntries);
            }

            logEntries.Clear();
            logEntries.AddRange(rebuiltEntries);
        }

        public List<LogEntry> GetEntries()
        {
            return new List<LogEntry>(logEntries);
        }

        public List<LogEntry> GetFilteredEntries()
        {
            return filterManager.FilterLogs(logEntries);
        }

        private void PruneOriginalIfExceedsLimit()
        {
            if (originalLogEntries.Count <= MAX_LOG_ENTRIES)
                return;

            LogEntry oldest = originalLogEntries[0];
            originalLogEntries.RemoveAt(0);

            if (!filterManager.IsCollapseEnabled)
                return;

            LogEntry group = FindExistingInList(logEntries, oldest);
            if (group != null && --group.Count <= 0)
                logEntries.Remove(group);
        }

        private void PruneVisibleIfExceedsLimit()
        {
            if (logEntries.Count <= MAX_LOG_ENTRIES)
                return;

            logEntries.RemoveAt(0);
        }

        private LogEntry FindExistingEntry(LogEntry newEntry)
        {
            if (filterManager.IsCollapseExactStacktraceEnabled)
            {
                return logEntries.Find(e =>
                    e.Message == newEntry.Message &&
                    e.Type == newEntry.Type &&
                    e.StackTrace == newEntry.StackTrace);
            }

            return logEntries.Find(e =>
                e.Message == newEntry.Message &&
                e.Type == newEntry.Type);
        }

        private LogEntry FindExistingInList(List<LogEntry> list, LogEntry entry)
        {
            if (filterManager.IsCollapseExactStacktraceEnabled)
            {
                return list.Find(e =>
                    e.Message == entry.Message &&
                    e.Type == entry.Type &&
                    e.StackTrace == entry.StackTrace);
            }

            return list.Find(e =>
                e.Message == entry.Message &&
                e.Type == entry.Type);
        }

        private static LogEntry CloneEntry(LogEntry entry)
        {
            return new LogEntry
            {
                Message = entry.Message,
                StackTrace = entry.StackTrace,
                Type = entry.Type,
                RealtimeSinceStartup = entry.RealtimeSinceStartup,
                Count = 1,
                ContextGameObjectInstanceId = entry.ContextGameObjectInstanceId
            };
        }

	private static int TryExtractGameObjectContextId(string message, string stackTrace)
	{
		// First, try to get context from LogContextHelper (captures Unity's Debug.Log context parameter)
		int contextId = LogContextHelper.GetLastLogContextInstanceId();
		if (contextId != 0)
		{
			// Clear the context after use
			LogContextHelper.ClearLogContext();
			return contextId;
		}

        // Try to extract GameObject reference from message if it's logged with context
        // Unity format: "Message (UnityEngine.GameObject)"
        if (string.IsNullOrEmpty(message))
            return 0;

        // Check if message ends with a context identifier (e.g., "(GameObjectName (UnityEngine.GameObject))")
        int openParenIndex = message.LastIndexOf('(');
        if (openParenIndex > 0)
        {
            string possibleContext = message.Substring(openParenIndex);

            // Check if this looks like a Unity object context
            if (possibleContext.Contains("GameObject") || possibleContext.Contains("Component") || possibleContext.Contains("MonoBehaviour"))
            {
                // Try to find the GameObject instance from the stack trace
                // This is a best-effort approach
                contextId = TryFindGameObjectInstanceFromStackTrace(stackTrace);
            }
        }

        return contextId;
    }

    private static int TryFindGameObjectInstanceFromStackTrace(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return 0;

            try
            {
                // Try to extract context from Unity object reference in message
                // When Debug.Log is called with an object context, we can use that
                string[] lines = stackTrace.Split('\n');
                foreach (string line in lines)
                {
                    // Look for MonoBehaviour method calls in stack trace
                    if (line.Contains("at ") && (line.Contains(".cs:") || line.Contains(".cs(") || line.Contains(".cs)")))
                    {
                        // This line has file info, but we can't reliably extract GameObject without reflection
                        // Return the first found frame's context (would need more sophisticated parsing)
                        break;
                    }
                }
            }
            catch
            {
                // Silently ignore any parsing errors
            }

            return 0;
        }
    }
}
#endif
