#if UNITY_EDITOR
using System;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Decides which logs are visible: log type, search text and sections.
    /// </summary>
    internal class LogFilterManager
    {
        private bool showInfo = true;
        private bool showWarnings = true;
        private bool showErrors = true;
        private bool collapseEnabled = false;
        private bool collapseExactStacktrace = false;
        private int version = 1;

        // The versions before and after the last search change that only narrowed the search (more text typed).
        private int narrowedFromVersion = -1;
        private int narrowedToVersion = -1;

        private readonly ConsoleSearchEngine searchEngine;
        private readonly SectionedLogOrganizer sectionOrganizer;

        public LogFilterManager(SectionedLogOrganizer sections)
        {
            searchEngine = new();
            sectionOrganizer = sections;
        }

        // Both parts only ever increase, so their sum changes whenever a visibility filter changes.
        public int Version => version + sectionOrganizer.Version;

        public bool MatchesLogType(LogEntry log)
        {
            return (log.type == LogType.Log && showInfo) ||
                   (log.type == LogType.Warning && showWarnings) ||
                   (log.IsError && showErrors);
        }

        public void SetShowInfo(bool show)
        {
            if (showInfo == show)
                return;

            showInfo = show;
            version++;
        }

        public void SetShowWarnings(bool show)
        {
            if (showWarnings == show)
                return;

            showWarnings = show;
            version++;
        }

        public void SetShowErrors(bool show)
        {
            if (showErrors == show)
                return;

            showErrors = show;
            version++;
        }

        public bool IsShowingInfo => showInfo;
        public bool IsShowingWarnings => showWarnings;
        public bool IsShowingErrors => showErrors;

        // LogEntryManager regroups entries when the collapse settings change.
        public void SetCollapse(bool enabled)
        {
            collapseEnabled = enabled;
        }

        public bool IsCollapseEnabled => collapseEnabled;

        public void SetCollapseExactStacktrace(bool enabled)
        {
            collapseExactStacktrace = enabled;
        }

        public bool IsCollapseExactStacktraceEnabled => collapseExactStacktrace;

        public void SetSearchFilter(string filter)
        {
            filter ??= "";
            if (filter == searchEngine.GetSearchFilter())
                return;

            string previous = searchEngine.GetSearchFilter();
            int previousVersion = Version;
            searchEngine.SetSearchFilter(filter);
            version++;

            // A log that contains the new text contains the old one too.
            bool narrows = previous.Length > 0 && filter.IndexOf(previous, StringComparison.OrdinalIgnoreCase) >= 0;
            narrowedFromVersion = narrows ? previousVersion : -1;
            narrowedToVersion = narrows ? Version : -1;
        }

        // True when the only change since fromVersion narrowed the search, so the logs visible then are the only
        // ones that can still match.
        public bool IsNarrowedSince(int fromVersion)
        {
            return fromVersion == narrowedFromVersion && Version == narrowedToVersion;
        }

        public string GetSearchFilter()
        {
            return searchEngine.GetSearchFilter();
        }

        public bool Matches(LogEntry log)
        {
            return MatchesLogType(log) &&
                   searchEngine.MatchesSearchFilter(log) &&
                   sectionOrganizer.MatchesSectionFilter(log);
        }
    }
}
#endif
