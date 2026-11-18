#if UNITY_EDITOR
using System;

namespace EnhancedConsole.Editor
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

		public bool MatchesSearchFilter(LogEntry log)
		{
			if (string.IsNullOrEmpty(searchFilter))
				return true;

			return SearchInText(log.Message, searchFilter) ||
				   SearchInText(log.StackTrace, searchFilter);
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
