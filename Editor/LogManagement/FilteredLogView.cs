#if UNITY_EDITOR
using System;
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
        private readonly Predicate<LogEntry> isFilteredOut;
        private LogFilterManager currentFilters;
        private int builtStructureVersion;
        private int builtFilterVersion = -1;
        private int filteredSourceCount;

        // The list is owned by this view: don't modify it.
        public List<LogEntry> Entries => entries;

        // Bumped each time the view is rebuilt or narrowed, so indices into it are stale.
        public int ResetCount { get; private set; }

        public FilteredLogView()
        {
            isFilteredOut = IsFilteredOut;
        }

        public void Refresh(IReadOnlyList<LogEntry> source, LogFilterManager filters, int structureVersion)
        {
            currentFilters = filters;
            if (structureVersion != builtStructureVersion)
                Reset(filters, structureVersion);
            else if (filters.Version != builtFilterVersion)
                Refilter(filters, structureVersion);

            for (int i = filteredSourceCount; i < source.Count; i++)
            {
                if (filters.Matches(source[i]))
                    entries.Add(source[i]);
            }

            filteredSourceCount = source.Count;
        }

        // Typing more search text only hides logs, so only the visible ones are checked again. Logs already
        // filtered out stay out, so the source position doesn't move.
        private void Refilter(LogFilterManager filters, int structureVersion)
        {
            if (!filters.IsNarrowedSince(builtFilterVersion))
            {
                Reset(filters, structureVersion);
                return;
            }

            entries.RemoveAll(isFilteredOut);
            builtFilterVersion = filters.Version;
            ResetCount++;
        }

        private void Reset(LogFilterManager filters, int structureVersion)
        {
            entries.Clear();
            filteredSourceCount = 0;
            builtFilterVersion = filters.Version;
            builtStructureVersion = structureVersion;
            ResetCount++;
        }

        private bool IsFilteredOut(LogEntry entry)
        {
            return !currentFilters.Matches(entry);
        }
    }
}
#endif
