using System.Collections.Generic;
using EnhancedConsole.Editor.LogManagement;
using NUnit.Framework;

namespace EnhancedConsole.Editor.Tests
{
    /// <summary>
    /// Narrowing the search refilters only the visible logs, and gives the same result as filtering them all.
    /// </summary>
    public class FilteredLogViewTests
    {
        private const int LOG_COUNT = 500;

        [Test]
        public void TypingMoreText_ShowsTheSameLogsAsAFreshSearch()
        {
            LogEntryManager typed = LogFixtures.CreateManager(out LogFilterManager typedFilters, LOG_COUNT);
            foreach (string search in new[] { "s", "sh", "shop", "shopscreen.method1" })
            {
                typedFilters.SetSearchFilter(search);
                typed.GetFilteredEntries();
            }

            CollectionAssert.AreEqual(FreshSearch("shopscreen.method1"), Messages(typed.GetFilteredEntries()));
        }

        [Test]
        public void DeletingText_ShowsTheLogsTheShorterSearchMatches()
        {
            LogEntryManager manager = LogFixtures.CreateManager(out LogFilterManager filters, LOG_COUNT);
            filters.SetSearchFilter("shopscreen");
            manager.GetFilteredEntries();
            filters.SetSearchFilter("shop");

            CollectionAssert.AreEqual(FreshSearch("shop"), Messages(manager.GetFilteredEntries()));
        }

        [Test]
        public void LogsAddedAfterNarrowing_AreFilteredByTheNewSearch()
        {
            LogEntryManager manager = LogFixtures.CreateManager(out LogFilterManager filters, LOG_COUNT);
            filters.SetSearchFilter("audio");
            manager.GetFilteredEntries();
            filters.SetSearchFilter("audiomixer.method7");
            manager.GetFilteredEntries();
            for (int i = LOG_COUNT; i < LOG_COUNT * 2; i++)
                manager.AddLog(LogFixtures.Create(i));

            List<string> expected = FreshSearch("audiomixer.method7", LOG_COUNT * 2);
            CollectionAssert.AreEqual(expected, Messages(manager.GetFilteredEntries()));
        }

        [Test]
        public void ChangingAnotherFilterBetweenSearches_FiltersEveryLogAgain()
        {
            LogEntryManager manager = LogFixtures.CreateManager(out LogFilterManager filters, LOG_COUNT);
            filters.SetShowWarnings(false);
            filters.SetSearchFilter("player");
            manager.GetFilteredEntries();
            filters.SetShowWarnings(true);
            filters.SetSearchFilter("playercontroller");

            List<string> expected = FreshSearch("playercontroller");
            CollectionAssert.AreEqual(expected, Messages(manager.GetFilteredEntries()));
        }

        [Test]
        public void Narrowing_MarksTheViewIndicesStale()
        {
            LogEntryManager manager = LogFixtures.CreateManager(out LogFilterManager filters, LOG_COUNT);
            filters.SetSearchFilter("shop");
            manager.GetFilteredEntries();
            int resetCount = manager.FilteredResetCount;
            filters.SetSearchFilter("shopscreen");
            manager.GetFilteredEntries();

            Assert.Greater(manager.FilteredResetCount, resetCount);
        }

        private static List<string> FreshSearch(string search, int count = LOG_COUNT)
        {
            LogEntryManager manager = LogFixtures.CreateManager(out LogFilterManager filters, count);
            filters.SetSearchFilter(search);
            return Messages(manager.GetFilteredEntries());
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
