#if UNITY_EDITOR
using UnityEngine;

namespace EnhancedConsole.Editor
{
	/// <summary>
	/// Interface for log type filtering.
	/// Segregates filter concerns per ISP.
	/// </summary>
	internal interface ILogTypeFilter
	{
		bool MatchesLogType(LogEntry log);
	}

	/// <summary>
	/// Manages all filtering logic: type, search, sections.
	/// Uses interface segregation for flexible filtering.
	/// Single Responsibility: Filter orchestration.
	/// </summary>
	internal class LogFilterManager : ILogTypeFilter
	{
		private bool showInfo = true;
		private bool showWarnings = true;
		private bool showErrors = true;
		private bool collapseEnabled = false;
		private bool collapseExactStacktrace = false;
		private int version = 1;

		private readonly ConsoleSearchEngine searchEngine;
		private readonly SectionedLogOrganizer sectionOrganizer;

		public LogFilterManager()
		{
			searchEngine = new();
			sectionOrganizer = new();
		}

		/// <summary>
		/// Changes whenever a filter that decides which entries are visible changes.
		/// Both parts only ever increase, so their sum changes on every update.
		/// </summary>
		public int Version => version + sectionOrganizer.Version;

		// Log Type Filter Implementation
		public bool MatchesLogType(LogEntry log)
		{
			return (log.Type == LogType.Log && showInfo) ||
				   (log.Type == LogType.Warning && showWarnings) ||
				   (log.IsError && showErrors);
		}

		// Log Type Configuration
		public void SetShowInfo(bool show)
		{
			SetFlag(ref showInfo, show);
		}

		public void SetShowWarnings(bool show)
		{
			SetFlag(ref showWarnings, show);
		}

		public void SetShowErrors(bool show)
		{
			SetFlag(ref showErrors, show);
		}

		public bool IsShowingInfo => showInfo;
		public bool IsShowingWarnings => showWarnings;
		public bool IsShowingErrors => showErrors;

		// Collapse Configuration. LogEntryManager regroups entries when these change.
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

		// Search
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

		// Section Organizer Access
		public SectionedLogOrganizer GetSectionOrganizer()
		{
			return sectionOrganizer;
		}

		public void InitializeSectionOrganizer(SelectiveLogsScriptableObject config)
		{
			sectionOrganizer.Initialize(config);
		}

		// Combined Filtering
		public bool Matches(LogEntry log)
		{
			return MatchesLogType(log) &&
				   searchEngine.MatchesSearchFilter(log) &&
				   sectionOrganizer.MatchesSectionFilter(log);
		}

		public bool HasActiveFilters()
		{
			return !showInfo || !showWarnings || !showErrors ||
				   !string.IsNullOrEmpty(searchEngine.GetSearchFilter()) ||
				   sectionOrganizer.IsFilterActive;
		}

		private void SetFlag(ref bool field, bool value)
		{
			if (field == value)
				return;

			field = value;
			version++;
		}
	}
}
#endif
