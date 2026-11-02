#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
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

		private ConsoleSearchEngine searchEngine;
		private SectionedLogOrganizer sectionOrganizer;

		public LogFilterManager()
		{
			searchEngine = new();
			sectionOrganizer = new();
		}

		// Log Type Filter Implementation
		public bool MatchesLogType(LogEntry log)
		{
			return (log.Type == LogType.Log && showInfo) ||
				   (log.Type == LogType.Warning && showWarnings) ||
				   (log.Type is LogType.Error or LogType.Exception or LogType.Assert && showErrors);
		}

		// Log Type Configuration
		public void SetShowInfo(bool show)
		{
			showInfo = show;
		}

		public void SetShowWarnings(bool show)
		{
			showWarnings = show;
		}

		public void SetShowErrors(bool show)
		{
			showErrors = show;
		}

		public bool IsShowingInfo => showInfo;
		public bool IsShowingWarnings => showWarnings;
		public bool IsShowingErrors => showErrors;

		public int GetInfoCount(List<LogEntry> logs)
		{
			return logs.Count(e => e.Type == LogType.Log);
		}

		public int GetWarningCount(List<LogEntry> logs)
		{
			return logs.Count(e => e.Type == LogType.Warning);
		}

		public int GetErrorCount(List<LogEntry> logs)
		{
			return logs.Count(e => e.Type == LogType.Error || e.Type == LogType.Exception || e.Type == LogType.Assert);
		}

		// Collapse Configuration
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

		// Search Engine Access
		public ConsoleSearchEngine GetSearchEngine()
		{
			return searchEngine;
		}

		public void SetSearchFilter(string filter)
		{
			searchEngine.SetSearchFilter(filter);
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
		public List<LogEntry> FilterLogs(List<LogEntry> logs)
		{
			return logs
				.Where(log =>
					MatchesLogType(log) &&
					searchEngine.MatchesSearchFilter(log) &&
					sectionOrganizer.MatchesSectionFilter(log))
				.ToList();
		}

		// Analytics
		public int GetTotalLogCount(List<LogEntry> logs)
		{
			return logs.Count;
		}

		public bool HasActiveFilters()
		{
			return !showInfo || !showWarnings || !showErrors ||
				   !string.IsNullOrEmpty(searchEngine.GetSearchFilter()) ||
				   sectionOrganizer.IsFilterActive;
		}
	}
}
#endif

