using System.Collections.Generic;
using EnhancedConsole.Editor.LogManagement;
using NUnit.Framework;
using UnityEngine;

namespace EnhancedConsole.Editor.Tests
{
    /// <summary>
    /// New compiler results replace the assembly's old ones and the same messages logged during the compilation.
    /// </summary>
    public class CompilerMessageTrackerTests
    {
        private const string ASSEMBLY = "Library/ScriptAssemblies/Game.dll";
        private const string OTHER_ASSEMBLY = "Library/ScriptAssemblies/Tools.dll";

        [Test]
        public void NewResults_ReplaceTheAssemblysOldResultsAndDuplicatesLoggedDuringTheCompilation()
        {
            List<LogEntry> entries = new()
            {
                Compiler("old Game warning", ASSEMBLY),
                Compiler("Tools warning", OTHER_ASSEMBLY),
                Log("CS0168 unused variable"),
                Log("regular log")
            };
            CompilerMessageTracker tracker = new();
            tracker.BeginCompilation(1);
            entries.Add(Log("CS0168 unused variable"));
            entries.Add(Log("another log"));

            tracker.RemoveReplacedEntries(entries, ASSEMBLY, Results("CS0168 unused variable"));

            CollectionAssert.AreEqual(new[] { "Tools warning", "regular log", "another log" }, Messages(entries));
        }

        [Test]
        public void WithoutACompilationStart_DuplicatesAnywhereAreReplaced()
        {
            List<LogEntry> entries = new()
            {
                Log("CS0168 unused variable"), Compiler("old Game warning", ASSEMBLY), Log("regular log")
            };
            CompilerMessageTracker tracker = new();

            tracker.RemoveReplacedEntries(entries, ASSEMBLY, Results("CS0168 unused variable"));

            CollectionAssert.AreEqual(new[] { "regular log" }, Messages(entries));
        }

        private static LogEntry Log(string message)
        {
            return new LogEntry { message = message, type = LogType.Warning, count = 1 };
        }

        private static LogEntry Compiler(string message, string assembly)
        {
            LogEntry entry = Log(message);
            entry.compilerAssembly = assembly;
            return entry;
        }

        private static List<CapturedLog> Results(string message)
        {
            return new List<CapturedLog> { new() { message = message, type = LogType.Warning } };
        }

        private static List<string> Messages(List<LogEntry> entries)
        {
            List<string> messages = new();
            foreach (LogEntry entry in entries)
                messages.Add(entry.message);

            return messages;
        }
    }
}
