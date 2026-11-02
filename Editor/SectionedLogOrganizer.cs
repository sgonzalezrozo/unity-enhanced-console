#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace EnhancedConsole.Editor
{
	/// <summary>
	/// Manages section-based log organization and filtering.
	/// Single Responsibility: Section organization only.
	/// </summary>
	internal class SectionedLogOrganizer
	{
		private SelectiveLogsScriptableObject config;
		private Dictionary<string, bool> sectionEnabled = new();
		private bool filterActive = false; // Default: OFF to show all logs
		private SLFilterMode filterMode = SLFilterMode.StackAndContent;

		public void Initialize(SelectiveLogsScriptableObject configuration)
		{
			config = configuration;
			RefreshSectionStates();
		}

		public void RefreshSectionStates()
		{
			if (config == null)
				return;

			sectionEnabled.Clear();
			foreach (SelectiveLogSection section in config.GetAllSections())
				sectionEnabled[section.name] = section.enabled;
		}

		public void SetFilterActive(bool active)
		{
			filterActive = active;
		}

		public bool IsFilterActive => filterActive;

		public void SetFilterMode(SLFilterMode mode)
		{
			filterMode = mode;
		}

		public SLFilterMode GetFilterMode => filterMode;

		public void SetSectionEnabled(string sectionName, bool enabled)
		{
			if (sectionEnabled.ContainsKey(sectionName))
				sectionEnabled[sectionName] = enabled;
		}

		public bool IsSectionEnabled(string sectionName)
		{
			return !sectionEnabled.ContainsKey(sectionName) || sectionEnabled[sectionName];
		}

		public int GetActiveSectionCount()
		{
			return sectionEnabled.Count(s => s.Value);
		}

		public int GetTotalSectionCount()
		{
			return config != null ? config.GetAllSections().Length : 0;
		}

		public SelectiveLogSection GetSectionForLog(LogEntry log)
		{
			if (config == null || !filterActive)
				return null;

			return config.GetFirstMatchingSection(log, filterMode);
		}

	public bool MatchesSectionFilter(LogEntry log)
	{
		if (!filterActive || config == null)
			return true;

		SelectiveLogSection section = GetSectionForLog(log);
		// If log doesn't match any section, show it (don't filter out)
		if (section == null)
			return true;

		if (!IsSectionEnabled(section.name))
			return false;

		switch (filterMode)
		{
			case SLFilterMode.OnlyStack:
				return !string.IsNullOrEmpty(log.StackTrace);
			case SLFilterMode.OnlyContent:
				return !string.IsNullOrEmpty(log.Message);
			default:
				return true;
		}
	}

		public bool HasValidConfiguration()
		{
			return config != null;
		}
	}
}
#endif

