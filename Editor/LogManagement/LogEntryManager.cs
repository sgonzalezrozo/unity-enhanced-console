#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// Manages log entry lifecycle: adding, clearing, collapsing and the cached filtered view.
    /// Single Responsibility: Log storage and lifecycle only.
    /// </summary>
    internal class LogEntryManager
    {
        private const int MAX_LOG_ENTRIES = 10000;

        // Entries are removed in chunks so the list isn't shifted on every log once it is full,
        // and the filtered view is rebuilt once per chunk instead of once per log.
        private const int PRUNE_CHUNK = 1000;

        private readonly List<LogEntry> allEntries = new();
        private readonly List<LogEntry> collapsedEntries = new();
        private readonly Dictionary<CollapseKey, LogEntry> collapseGroups = new();
        private readonly LogFilterManager filterManager;

        // Counts per level for each view: [info, warning, error].
        private readonly int[] allCounts = new int[3];
        private readonly int[] collapsedCounts = new int[3];

        // Texts of the current compiler messages, so the same message arriving through
        // Application.logMessageReceived isn't listed twice.
        private readonly HashSet<string> compilerMessageTexts = new();
        private int compilationStartIndex = -1;

        private bool builtCollapsed;
        private bool builtExactStacktrace;

        // Bumped when entries are removed, reordered or regrouped. Appends don't bump it.
        private int structureVersion = 1;

        /// <summary>Time source for new entries. Replaceable so the storage logic can be tested outside Unity.</summary>
        internal static Func<float> Clock = () => Time.realtimeSinceStartup;

        internal static Func<int> FrameCounter = () => Time.frameCount;

        private readonly List<LogEntry> filteredEntries = new();
        private int filteredStructureVersion;
        private int filteredFilterVersion = -1;
        private int filteredSourceCount;

        /// <summary>Bumped each time the filtered view is rebuilt from scratch, so indices into it are stale.</summary>
        public int FilteredResetCount { get; private set; }

        public LogEntryManager(LogFilterManager filterManager)
        {
            this.filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
        }

        public int InfoCount => CurrentCounts[0];
        public int WarningCount => CurrentCounts[1];
        public int ErrorCount => CurrentCounts[2];

        /// <summary>The unfiltered view: every entry, or one entry per group while collapsed.</summary>
        public IReadOnlyList<LogEntry> Entries
        {
            get
            {
                EnsureCollapseMode();
                return builtCollapsed ? collapsedEntries : allEntries;
            }
        }

        private int[] CurrentCounts
        {
            get
            {
                EnsureCollapseMode();
                return builtCollapsed ? collapsedCounts : allCounts;
            }
        }

        public void AddLog(string message, string stackTrace, LogType type, int contextGameObjectInstanceId)
        {
            AddLog(message, stackTrace, type, contextGameObjectInstanceId, Clock(), DateTime.Now.Ticks, FrameCounter());
        }

        /// <summary>
        /// Adds a log with times captured when it was written, such as a log from a background thread
        /// that's added later on the main thread. Main thread only.
        /// </summary>
        public void AddLog(string message, string stackTrace, LogType type, int contextGameObjectInstanceId,
            float realtimeSinceStartup, long wallClockTicks, int frameCount)
        {
            if (compilerMessageTexts.Count > 0 && message != null && compilerMessageTexts.Contains(message))
                return;

            LogEntry entry = CreateEntry(message, stackTrace, type, contextGameObjectInstanceId);
            entry.RealtimeSinceStartup = realtimeSinceStartup;
            entry.WallClockTicks = wallClockTicks;
            entry.FrameCount = frameCount;
            Append(entry);
        }

        /// <summary>Marks where a compilation started, so duplicates logged during it can be found.</summary>
        public void BeginCompilation()
        {
            compilationStartIndex = allEntries.Count;
        }

        /// <summary>
        /// Replaces the compiler messages of one assembly with the latest results. Messages logged
        /// during this compilation with the same text are dropped, since this list replaces them.
        /// </summary>
        public void SetCompilerMessages(string assemblyPath, IReadOnlyList<(string message, LogType type)> messages)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                return;

            HashSet<string> newTexts = new();
            foreach ((string message, LogType _) in messages)
            {
                if (!string.IsNullOrEmpty(message))
                    newTexts.Add(message);
            }

            int searchFrom = Math.Max(0, compilationStartIndex);
            bool removedAny = false;
            for (int i = allEntries.Count - 1; i >= 0; i--)
            {
                LogEntry entry = allEntries[i];
                bool isOldResult = entry.CompilerAssembly == assemblyPath;
                bool isDuplicate = !entry.IsCompilerMessage && i >= searchFrom && entry.Message != null && newTexts.Contains(entry.Message);
                if (!isOldResult && !isDuplicate)
                    continue;

                allEntries.RemoveAt(i);
                removedAny = true;
            }

            if (removedAny)
                OnEntriesRemoved();

            foreach ((string message, LogType type) in messages)
            {
                if (string.IsNullOrEmpty(message))
                    continue;

                LogEntry entry = CreateEntry(message, "", type, 0);
                entry.CompilerAssembly = assemblyPath;
                Append(entry);
            }

            RebuildCompilerMessageTexts();
        }

        /// <summary>Clears every log except compiler errors, which stay until the code compiles, like Unity's Console.</summary>
        public void ClearKeepingCompilerErrors()
        {
            allEntries.RemoveAll(entry => !entry.IsCompilerError);
            compilationStartIndex = -1;
            OnEntriesRemoved();
            RebuildCompilerMessageTexts();
        }

        /// <summary>
        /// Removes logs picked from <see cref="Entries"/> or the filtered view. In collapse mode an entry
        /// stands for its whole group, so every log in the group is removed.
        /// </summary>
        /// <returns>How many logs were removed.</returns>
        public int RemoveEntries(IReadOnlyCollection<LogEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return 0;

            EnsureCollapseMode();

            HashSet<LogEntry> exact = null;
            HashSet<CollapseKey> groups = null;
            if (builtCollapsed)
            {
                groups = new HashSet<CollapseKey>();
                foreach (LogEntry entry in entries)
                    groups.Add(new CollapseKey(entry, builtExactStacktrace));
            }
            else
            {
                exact = new HashSet<LogEntry>(entries);
            }

            int write = 0;
            int removedBeforeCompilation = 0;
            bool removedCompilerMessage = false;

            for (int read = 0; read < allEntries.Count; read++)
            {
                LogEntry entry = allEntries[read];
                bool remove = exact?.Contains(entry) ?? groups.Contains(new CollapseKey(entry, builtExactStacktrace));
                if (!remove)
                {
                    allEntries[write++] = entry;
                    continue;
                }

                if (read < compilationStartIndex)
                    removedBeforeCompilation++;
                removedCompilerMessage |= entry.IsCompilerMessage;
            }

            int removed = allEntries.Count - write;
            if (removed == 0)
                return 0;

            allEntries.RemoveRange(write, removed);
            if (compilationStartIndex >= 0)
                compilationStartIndex -= removedBeforeCompilation;

            OnEntriesRemoved();
            if (removedCompilerMessage)
                RebuildCompilerMessageTexts();

            return removed;
        }

        private LogEntry CreateEntry(string message, string stackTrace, LogType type, int contextGameObjectInstanceId)
        {
            return new LogEntry()
            {
                Message = message,
                StackTrace = stackTrace,
                Type = type,
                RealtimeSinceStartup = Clock(),
                WallClockTicks = DateTime.Now.Ticks,
                FrameCount = FrameCounter(),
                Count = 1,
                ContextGameObjectInstanceId = contextGameObjectInstanceId
            };
        }

        private void Append(LogEntry entry)
        {
            EnsureCollapseMode();

            allEntries.Add(entry);
            allCounts[LevelIndex(entry.Type)]++;

            if (builtCollapsed)
                AddToCollapsed(entry);

            if (allEntries.Count > MAX_LOG_ENTRIES)
                PruneOldest(PRUNE_CHUNK);
        }

        public void Clear()
        {
            allEntries.Clear();
            collapsedEntries.Clear();
            collapseGroups.Clear();
            Array.Clear(allCounts, 0, allCounts.Length);
            Array.Clear(collapsedCounts, 0, collapsedCounts.Length);
            compilerMessageTexts.Clear();
            compilationStartIndex = -1;
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

            int start = Math.Max(0, entries.Count - MAX_LOG_ENTRIES);
            for (int i = start; i < entries.Count; i++)
            {
                LogEntry entry = entries[i];
                if (entry == null)
                    continue;

                entry.Count = 1;
                allEntries.Add(entry);
                allCounts[LevelIndex(entry.Type)]++;
            }

            RebuildWithCollapse();
            RebuildCompilerMessageTexts();
        }

        public void RebuildWithCollapse()
        {
            builtCollapsed = filterManager.IsCollapseEnabled;
            builtExactStacktrace = filterManager.IsCollapseExactStacktraceEnabled;

            collapsedEntries.Clear();
            collapseGroups.Clear();
            Array.Clear(collapsedCounts, 0, collapsedCounts.Length);

            if (builtCollapsed)
            {
                for (int i = 0; i < allEntries.Count; i++)
                    AddToCollapsed(allEntries[i]);
            }

            structureVersion++;
        }

        /// <summary>
        /// Returns the filtered view. The list is cached and owned by this manager: don't modify it.
        /// New logs are filtered incrementally; the view is rebuilt only when entries are removed,
        /// regrouped, or a filter changes.
        /// </summary>
        public List<LogEntry> GetFilteredEntries()
        {
            IReadOnlyList<LogEntry> source = Entries;
            int filterVersion = filterManager.Version;

            if (filterVersion != filteredFilterVersion || structureVersion != filteredStructureVersion)
            {
                filteredEntries.Clear();
                filteredSourceCount = 0;
                filteredFilterVersion = filterVersion;
                filteredStructureVersion = structureVersion;
                FilteredResetCount++;
            }

            for (int i = filteredSourceCount; i < source.Count; i++)
            {
                LogEntry entry = source[i];
                if (filterManager.Matches(entry))
                    filteredEntries.Add(entry);
            }

            filteredSourceCount = source.Count;
            return filteredEntries;
        }

        private void EnsureCollapseMode()
        {
            if (builtCollapsed != filterManager.IsCollapseEnabled ||
                (builtCollapsed && builtExactStacktrace != filterManager.IsCollapseExactStacktraceEnabled))
                RebuildWithCollapse();
        }

        private void AddToCollapsed(LogEntry entry)
        {
            CollapseKey key = new(entry, builtExactStacktrace);
            if (collapseGroups.TryGetValue(key, out LogEntry group))
            {
                group.Count++;
                return;
            }

            group = CloneEntry(entry);
            collapseGroups.Add(key, group);
            collapsedEntries.Add(group);
            collapsedCounts[LevelIndex(entry.Type)]++;
        }

        private void PruneOldest(int count)
        {
            count = Math.Min(count, allEntries.Count);

            for (int i = 0; i < count; i++)
            {
                LogEntry oldest = allEntries[i];
                allCounts[LevelIndex(oldest.Type)]--;

                if (!builtCollapsed)
                    continue;

                CollapseKey key = new(oldest, builtExactStacktrace);
                if (collapseGroups.TryGetValue(key, out LogEntry group) && --group.Count <= 0)
                {
                    collapseGroups.Remove(key);
                    collapsedCounts[LevelIndex(group.Type)]--;
                }
            }

            allEntries.RemoveRange(0, count);
            if (compilationStartIndex >= 0)
                compilationStartIndex = Math.Max(0, compilationStartIndex - count);

            if (builtCollapsed)
                collapsedEntries.RemoveAll(group => group.Count <= 0);

            structureVersion++;
        }

        // Recounts and regroups after entries were removed from the middle of the list.
        private void OnEntriesRemoved()
        {
            Array.Clear(allCounts, 0, allCounts.Length);
            foreach (LogEntry entry in allEntries)
                allCounts[LevelIndex(entry.Type)]++;

            RebuildWithCollapse();
        }

        private void RebuildCompilerMessageTexts()
        {
            compilerMessageTexts.Clear();
            foreach (LogEntry entry in allEntries)
            {
                if (entry.IsCompilerMessage && entry.Message != null)
                    compilerMessageTexts.Add(entry.Message);
            }
        }

        private static int LevelIndex(LogType type)
        {
            return type switch
            {
                LogType.Log => 0,
                LogType.Warning => 1,
                _ => 2
            };
        }

        private static LogEntry CloneEntry(LogEntry entry)
        {
            return new LogEntry
            {
                Message = entry.Message,
                StackTrace = entry.StackTrace,
                Type = entry.Type,
                RealtimeSinceStartup = entry.RealtimeSinceStartup,
                WallClockTicks = entry.WallClockTicks,
                FrameCount = entry.FrameCount,
                CompilerAssembly = entry.CompilerAssembly,
                Count = 1,
                ContextGameObjectInstanceId = entry.ContextGameObjectInstanceId
            };
        }

        private readonly struct CollapseKey : IEquatable<CollapseKey>
        {
            private readonly string message;
            private readonly string stackTrace;
            private readonly LogType type;

            public CollapseKey(LogEntry entry, bool includeStackTrace)
            {
                message = entry.Message ?? "";
                stackTrace = includeStackTrace ? entry.StackTrace ?? "" : null;
                type = entry.Type;
            }

            public bool Equals(CollapseKey other)
            {
                return type == other.type &&
                       string.Equals(message, other.message, StringComparison.Ordinal) &&
                       string.Equals(stackTrace, other.stackTrace, StringComparison.Ordinal);
            }

            public override bool Equals(object obj) => obj is CollapseKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = message.GetHashCode();
                    hash = (hash * 397) ^ (int)type;
                    if (stackTrace != null)
                        hash = (hash * 397) ^ stackTrace.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
#endif
