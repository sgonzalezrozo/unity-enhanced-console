#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// Implements search and text filtering across log entries.
	/// Single Responsibility: Search functionality only.
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

		public void ClearSearchFilter()
		{
			searchFilter = "";
		}

		public bool MatchesSearchFilter(LogEntry log)
		{
			if (string.IsNullOrEmpty(searchFilter))
				return true;

			return SearchInText(log.Message, searchFilter) ||
				   SearchInText(log.StackTrace, searchFilter);
		}

		public List<LogEntry> FilterLogs(List<LogEntry> logs, Func<LogEntry, bool> additionalFilter)
		{
			return logs
				.Where(log => MatchesSearchFilter(log) && additionalFilter(log))
				.ToList();
		}

		private bool SearchInText(string text, string searchTerm)
		{
			if (string.IsNullOrEmpty(text))
				return false;

			return text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}
#endif

