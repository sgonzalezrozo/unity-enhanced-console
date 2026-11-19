#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace EnhancedConsole.Editor.LogManagement
{
    /// <summary>
    /// Remembers the texts of the current compiler messages, so the same message arriving through
    /// Application.logMessageReceived isn't listed twice, and where the running compilation started.
    /// </summary>
    internal sealed class CompilerMessageTracker
    {
        private readonly HashSet<string> texts = new();

        public int CompilationStartIndex { get; private set; } = -1;

        public bool IsCompilerText(string message)
        {
            return texts.Count > 0 && message != null && texts.Contains(message);
        }

        public void BeginCompilation(int entryCount)
        {
            CompilationStartIndex = entryCount;
        }

        public void EndCompilation()
        {
            CompilationStartIndex = -1;
        }

        public void OnEntriesRemovedBeforeStart(int count)
        {
            if (CompilationStartIndex >= 0)
                CompilationStartIndex = Math.Max(0, CompilationStartIndex - count);
        }

        // Removes the assembly's previous results, and regular logs from this compilation with the same text as a
        // new result.
        public void RemoveReplacedEntries(List<LogEntry> allEntries, string assemblyPath,
            IReadOnlyList<CapturedLog> results)
        {
            HashSet<string> newTexts = CollectTexts(results);
            int searchFrom = Math.Max(0, CompilationStartIndex);

            // One pass that moves the kept entries down, instead of a RemoveAt per entry that shifts the rest.
            int kept = 0;
            for (int i = 0; i < allEntries.Count; i++)
            {
                LogEntry entry = allEntries[i];
                bool isOldResult = entry.compilerAssembly == assemblyPath;
                bool isDuplicate = i >= searchFrom && !entry.IsCompilerMessage && entry.message != null &&
                                   newTexts.Contains(entry.message);
                if (!isOldResult && !isDuplicate)
                    allEntries[kept++] = entry;
            }

            allEntries.RemoveRange(kept, allEntries.Count - kept);
        }

        public void Rebuild(IReadOnlyList<LogEntry> allEntries)
        {
            texts.Clear();
            for (int i = 0; i < allEntries.Count; i++)
            {
                LogEntry entry = allEntries[i];
                if (entry.IsCompilerMessage && entry.message != null)
                    texts.Add(entry.message);
            }
        }

        public void Clear()
        {
            texts.Clear();
            CompilationStartIndex = -1;
        }

        private static HashSet<string> CollectTexts(IReadOnlyList<CapturedLog> results)
        {
            HashSet<string> newTexts = new();
            for (int i = 0; i < results.Count; i++)
            {
                if (!string.IsNullOrEmpty(results[i].message))
                    newTexts.Add(results[i].message);
            }

            return newTexts;
        }
    }
}
#endif
