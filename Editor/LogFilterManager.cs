#if UNITY_EDITOR
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

            searchEngine.SetSearchFilter(filter);
            version++;
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
