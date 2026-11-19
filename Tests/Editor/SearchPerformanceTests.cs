using System.Collections.Generic;
using System.Diagnostics;
using EnhancedConsole.Editor.LogManagement;
using NUnit.Framework;
using Debug = UnityEngine.Debug;

namespace EnhancedConsole.Editor.Tests
{
    /// <summary>
    /// Times searches over a full console. Logs the times; asserts only that the results are right.
    /// </summary>
    public class SearchPerformanceTests
    {
        private const int LOG_COUNT = 10000;
        private const string TYPED = "inventorymodel.method1";

        [Test]
        public void TypingASearch_OverAFullConsole()
        {
            LogEntryManager manager = LogFixtures.CreateManager(out LogFilterManager filters, LOG_COUNT);
            manager.GetFilteredEntries();

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<LogEntry> visible = null;
            for (int length = 1; length <= TYPED.Length; length++)
            {
                filters.SetSearchFilter(TYPED.Substring(0, length));
                visible = manager.GetFilteredEntries();
            }

            stopwatch.Stop();
            Debug.Log($"[EnhancedConsole bench] typing {TYPED.Length} characters over {LOG_COUNT} logs: " +
                      $"{stopwatch.ElapsedMilliseconds} ms, {visible.Count} visible");
            Assert.AreEqual(LOG_COUNT / 5, visible.Count);
        }

        [Test]
        public void SearchingEveryLog_ForTextNoLogHas()
        {
            LogEntryManager manager = LogFixtures.CreateManager(out LogFilterManager filters, LOG_COUNT);
            manager.GetFilteredEntries();

            Stopwatch stopwatch = Stopwatch.StartNew();
            const int RUNS = 5;
            for (int run = 0; run < RUNS; run++)
            {
                filters.SetSearchFilter(run % 2 == 0 ? "missingsystem" : "missingsystem2");
                filters.SetSearchFilter(run % 2 == 0 ? "nothing here" : "nothing there");
                manager.GetFilteredEntries();
            }

            stopwatch.Stop();
            long each = stopwatch.ElapsedMilliseconds / RUNS;
            Debug.Log($"[EnhancedConsole bench] full search over {LOG_COUNT} logs: {each} ms each");
            Assert.AreEqual(0, manager.GetFilteredEntries().Count);
        }
    }
}
