#if UNITY_EDITOR
using System.Collections.Generic;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// The logs that pass the filters. New logs are filtered incrementally; the view is rebuilt only when
    /// entries are removed or regrouped, or a filter changes.
    /// </summary>
    internal sealed class FilteredLogView
    {
        private readonly List<LogEntry> entries = new();
        private int builtStructureVersion;
        private int builtFilterVersion = -1;
        private int filteredSourceCount;

        // The list is owned by this view: don't modify it.
        public List<LogEntry> Entries => entries;

        // Bumped each time the view is rebuilt from scratch, so indices into it are stale.
        public int ResetCount { get; private set; }

        public void Refresh(IReadOnlyList<LogEntry> source, LogFilterManager filters, int structureVersion)
        {
            if (filters.Version != builtFilterVersion || structureVersion != builtStructureVersion)
            {
                entries.Clear();
                filteredSourceCount = 0;
                builtFilterVersion = filters.Version;
                builtStructureVersion = structureVersion;
                ResetCount++;
            }

            for (int i = filteredSourceCount; i < source.Count; i++)
            {
                if (filters.Matches(source[i]))
                    entries.Add(source[i]);
            }

            filteredSourceCount = source.Count;
        }
    }
}
#endif
