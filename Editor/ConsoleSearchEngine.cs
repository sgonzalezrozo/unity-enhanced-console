#if UNITY_EDITOR
using System;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Matches the search text against a log's message and stack trace, ignoring case.
    /// </summary>
    internal class ConsoleSearchEngine
    {
        private string searchFilter = "";

        public void SetSearchFilter(string filter)
        {
            searchFilter = filter ?? "";
        }

        public string GetSearchFilter()
        {
            return searchFilter;
        }

        public bool MatchesSearchFilter(LogEntry log)
        {
            if (string.IsNullOrEmpty(searchFilter))
                return true;

            return SearchInText(log.message, searchFilter) ||
                   SearchInText(log.stackTrace, searchFilter);
        }

        private static bool SearchInText(string text, string searchTerm)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
#endif
