#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// The logs to remove for a selection. In collapse mode a selected entry stands for its whole group.
    /// </summary>
    internal sealed class LogRemovalSet
    {
        private readonly HashSet<LogEntry> entries;
        private readonly HashSet<CollapseKey> groups;
        private readonly CollapsedLogView collapsedView;

        public LogRemovalSet(IReadOnlyCollection<LogEntry> selected, CollapsedLogView collapsed)
        {
            collapsedView = collapsed;
            if (!collapsed.IsEnabled)
            {
                entries = new HashSet<LogEntry>(selected);
                return;
            }

            groups = new HashSet<CollapseKey>();
            foreach (LogEntry entry in selected)
                groups.Add(collapsed.CreateKey(entry));
        }

        public int CountIn(IReadOnlyList<LogEntry> allEntries, int end)
        {
            int count = 0;
            for (int i = 0; i < Math.Min(end, allEntries.Count); i++)
            {
                if (Contains(allEntries[i]))
                    count++;
            }

            return count;
        }

        public bool Contains(LogEntry entry)
        {
            return entries != null ? entries.Contains(entry) : groups.Contains(collapsedView.CreateKey(entry));
        }
    }
}
#endif
