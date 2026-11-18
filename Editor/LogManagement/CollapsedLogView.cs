#if UNITY_EDITOR
using System.Collections.Generic;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// One entry per group of identical logs, with how many logs each group holds. Kept up to date as logs
    /// are added and pruned, so turning Collapse on doesn't regroup every log.
    /// </summary>
    internal sealed class CollapsedLogView
    {
        private readonly List<LogEntry> groups = new();
        private readonly Dictionary<CollapseKey, LogEntry> groupsByKey = new();

        public IReadOnlyList<LogEntry> Entries => groups;
        public LogLevelCounts Counts { get; } = new();
        public bool IsEnabled { get; private set; }
        public bool MatchesStackTrace { get; private set; }

        public bool IsOutOfDate(LogFilterManager filters)
        {
            return IsEnabled != filters.IsCollapseEnabled ||
                   (IsEnabled && MatchesStackTrace != filters.IsCollapseExactStacktraceEnabled);
        }

        public void Rebuild(IReadOnlyList<LogEntry> allEntries, LogFilterManager filters)
        {
            IsEnabled = filters.IsCollapseEnabled;
            MatchesStackTrace = filters.IsCollapseExactStacktraceEnabled;
            groups.Clear();
            groupsByKey.Clear();
            Counts.Clear();
            if (!IsEnabled)
                return;

            for (int i = 0; i < allEntries.Count; i++)
                Add(allEntries[i]);
        }

        public void Add(LogEntry entry)
        {
            if (!IsEnabled)
                return;

            CollapseKey key = CreateKey(entry);
            if (groupsByKey.TryGetValue(key, out LogEntry group))
            {
                group.count++;
                return;
            }

            group = CreateGroup(entry);
            groupsByKey.Add(key, group);
            groups.Add(group);
            Counts.Add(entry.type);
        }

        // Call before the oldest entries are removed from the full list.
        public void RemoveOldest(IReadOnlyList<LogEntry> allEntries, int count)
        {
            if (!IsEnabled)
                return;

            for (int i = 0; i < count; i++)
                RemoveFromGroup(allEntries[i]);

            groups.RemoveAll(IsEmptyGroup);
        }

        public CollapseKey CreateKey(LogEntry entry)
        {
            return MatchesStackTrace ? CollapseKey.ForMessageAndStackTrace(entry) : CollapseKey.ForMessage(entry);
        }

        private void RemoveFromGroup(LogEntry entry)
        {
            CollapseKey key = CreateKey(entry);
            if (!groupsByKey.TryGetValue(key, out LogEntry group) || --group.count > 0)
                return;

            groupsByKey.Remove(key);
            Counts.Remove(group.type);
        }

        private static bool IsEmptyGroup(LogEntry group)
        {
            return group.count <= 0;
        }

        private static LogEntry CreateGroup(LogEntry entry)
        {
            return new LogEntry
            {
                message = entry.message,
                stackTrace = entry.stackTrace,
                type = entry.type,
                realtimeSinceStartup = entry.realtimeSinceStartup,
                wallClockTicks = entry.wallClockTicks,
                frameCount = entry.frameCount,
                compilerAssembly = entry.compilerAssembly,
                count = 1,
                contextGameObjectInstanceId = entry.contextGameObjectInstanceId
            };
        }
    }
}
#endif
