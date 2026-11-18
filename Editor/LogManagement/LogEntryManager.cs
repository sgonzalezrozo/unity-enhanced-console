#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// Stores every log, up to a limit, and keeps the collapsed and filtered views in step with it.
    /// </summary>
    internal class LogEntryManager
    {
        private const int MAX_LOG_ENTRIES = 10000;

        // Entries are removed in chunks so the list isn't shifted on every log once it is full, and the
        // filtered view is rebuilt once per chunk instead of once per log.
        private const int PRUNE_CHUNK = 1000;

        private readonly List<LogEntry> allEntries = new();
        private readonly LogLevelCounts allCounts = new();
        private readonly CollapsedLogView collapsedView = new();
        private readonly FilteredLogView filteredView = new();
        private readonly CompilerMessageTracker compilerMessages = new();
        private readonly LogFilterManager filterManager;

        // Bumped when entries are removed, reordered or regrouped. Appends don't bump it.
        private int structureVersion = 1;

        public LogEntryManager(LogFilterManager filters)
        {
            filterManager = filters ?? throw new ArgumentNullException(nameof(filters));
        }

        public int FilteredResetCount => filteredView.ResetCount;
        public int InfoCount => CurrentCounts.Info;
        public int WarningCount => CurrentCounts.Warning;
        public int ErrorCount => CurrentCounts.Error;

        // The unfiltered view: every entry, or one entry per group while collapsed.
        public IReadOnlyList<LogEntry> Entries => IsCollapsed() ? collapsedView.Entries : allEntries;

        private LogLevelCounts CurrentCounts => IsCollapsed() ? collapsedView.Counts : allCounts;

        public void AddLog(CapturedLog log)
        {
            if (!compilerMessages.IsCompilerText(log.message))
                Append(LogEntry.FromCapturedLog(log));
        }

        // Marks where a compilation started, so duplicates logged during it can be found.
        public void BeginCompilation()
        {
            compilerMessages.BeginCompilation(allEntries.Count);
        }

        // Replaces the compiler messages of one assembly with the latest results. Messages logged during this
        // compilation with the same text are dropped, since this list replaces them.
        public void SetCompilerMessages(string assemblyPath, IReadOnlyList<CapturedLog> messages)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                return;

            int countBefore = allEntries.Count;
            compilerMessages.RemoveReplacedEntries(allEntries, assemblyPath, messages);
            if (allEntries.Count != countBefore)
                OnEntriesRemoved();

            AppendCompilerMessages(assemblyPath, messages);
            compilerMessages.Rebuild(allEntries);
        }

        // Clears every log except compiler errors, which stay until the code compiles, like Unity's Console.
        public void ClearKeepingCompilerErrors()
        {
            allEntries.RemoveAll(IsNotCompilerError);
            compilerMessages.EndCompilation();
            OnEntriesRemoved();
            compilerMessages.Rebuild(allEntries);
        }

        // Removes logs picked from Entries or the filtered view. In collapse mode an entry stands for its whole
        // group, so every log in the group is removed.
        public void RemoveEntries(IReadOnlyCollection<LogEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            EnsureCollapseMode();
            LogRemovalSet removal = new(entries, collapsedView);
            int removedBeforeCompilation = removal.CountIn(allEntries, compilerMessages.CompilationStartIndex);
            int countBefore = allEntries.Count;
            allEntries.RemoveAll(removal.Contains);
            if (allEntries.Count == countBefore)
                return;

            compilerMessages.OnEntriesRemovedBeforeStart(removedBeforeCompilation);
            OnEntriesRemoved();
            compilerMessages.Rebuild(allEntries);
        }

        public void Clear()
        {
            allEntries.Clear();
            allCounts.Clear();
            collapsedView.Rebuild(allEntries, filterManager);
            compilerMessages.Clear();
            structureVersion++;
        }

        public List<LogEntry> GetPersistableEntries()
        {
            return new List<LogEntry>(allEntries);
        }

        public void Restore(List<LogEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            Clear();
            for (int i = Math.Max(0, entries.Count - MAX_LOG_ENTRIES); i < entries.Count; i++)
            {
                if (entries[i] == null)
                    continue;

                entries[i].count = 1;
                allEntries.Add(entries[i]);
                allCounts.Add(entries[i].type);
            }

            RebuildWithCollapse();
            compilerMessages.Rebuild(allEntries);
        }

        public void RebuildWithCollapse()
        {
            collapsedView.Rebuild(allEntries, filterManager);
            structureVersion++;
        }

        // The cached filtered view, owned by this manager: don't modify it.
        public List<LogEntry> GetFilteredEntries()
        {
            filteredView.Refresh(Entries, filterManager, structureVersion);
            return filteredView.Entries;
        }

        private void Append(LogEntry entry)
        {
            EnsureCollapseMode();
            allEntries.Add(entry);
            allCounts.Add(entry.type);
            collapsedView.Add(entry);
            if (allEntries.Count > MAX_LOG_ENTRIES)
                PruneOldest(PRUNE_CHUNK);
        }

        private void PruneOldest(int count)
        {
            count = Math.Min(count, allEntries.Count);
            for (int i = 0; i < count; i++)
                allCounts.Remove(allEntries[i].type);

            collapsedView.RemoveOldest(allEntries, count);
            allEntries.RemoveRange(0, count);
            compilerMessages.OnEntriesRemovedBeforeStart(count);
            structureVersion++;
        }

        private void AppendCompilerMessages(string assemblyPath, IReadOnlyList<CapturedLog> messages)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (string.IsNullOrEmpty(messages[i].message))
                    continue;

                LogEntry entry = LogEntry.FromCapturedLog(messages[i]);
                entry.compilerAssembly = assemblyPath;
                Append(entry);
            }
        }

        // Recounts and regroups after entries were removed from the middle of the list.
        private void OnEntriesRemoved()
        {
            allCounts.Clear();
            for (int i = 0; i < allEntries.Count; i++)
                allCounts.Add(allEntries[i].type);

            RebuildWithCollapse();
        }

        private bool IsCollapsed()
        {
            EnsureCollapseMode();
            return collapsedView.IsEnabled;
        }

        private void EnsureCollapseMode()
        {
            if (collapsedView.IsOutOfDate(filterManager))
                RebuildWithCollapse();
        }

        private static bool IsNotCompilerError(LogEntry entry)
        {
            return !entry.IsCompilerError;
        }
    }
}
#endif
