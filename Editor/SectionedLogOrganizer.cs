#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EnhancedConsole.Editor
{
	/// <summary>
	/// Manages section-based log organization and filtering.
	/// Sections are compiled once (regexes parsed, plain names normalized) and each entry caches
	/// the index of its first matching section, so filtering doesn't re-run the matchers every GUI event.
	/// Single Responsibility: Section organization only.
	/// </summary>
	internal class SectionedLogOrganizer
	{
		private SelectiveLogsScriptableObject config;
		private CompiledSection[] sections = Array.Empty<CompiledSection>();
		private int matchSignature;
		private int stateSignature;
		private int activeSectionCount;
		private bool filterActive = false; // Default: OFF to show all logs
		private SLFilterMode filterMode = SLFilterMode.StackAndContent;

		/// <summary>Changes when which section an entry matches can change: names, patterns, order or filter mode.</summary>
		public int MatchVersion { get; private set; } = 1;

		/// <summary>Changes whenever anything that affects section filtering changes. Only ever increases.</summary>
		public int Version { get; private set; } = 1;

		public void Initialize(SelectiveLogsScriptableObject configuration)
		{
			if (!ReferenceEquals(config, configuration))
			{
				config = configuration;
				matchSignature = 0;
				stateSignature = 0;
			}

			RefreshSectionStates();
		}

		/// <summary>
		/// Picks up edits to the config asset. Cheap enough to call every GUI event:
		/// it only hashes the section fields and recompiles when something changed.
		/// </summary>
		public void RefreshSectionStates()
		{
			if (config == null)
			{
				if (sections.Length > 0)
				{
					sections = Array.Empty<CompiledSection>();
					activeSectionCount = 0;
					matchSignature = 0;
					stateSignature = 0;
					MatchVersion++;
					Version++;
				}

				return;
			}

			ComputeSignatures(out int newMatchSignature, out int newStateSignature);
			if (newMatchSignature == matchSignature && newStateSignature == stateSignature)
				return;

			bool matchChanged = newMatchSignature != matchSignature;
			matchSignature = newMatchSignature;
			stateSignature = newStateSignature;
			Compile(matchChanged);

			if (matchChanged)
				MatchVersion++;
			Version++;
		}

		public void SetFilterActive(bool active)
		{
			if (filterActive == active)
				return;

			filterActive = active;
			Version++;
		}

		public bool IsFilterActive => filterActive;

		public void SetFilterMode(SLFilterMode mode)
		{
			if (filterMode == mode)
				return;

			filterMode = mode;
			MatchVersion++;
			Version++;
		}

		public SLFilterMode GetFilterMode => filterMode;

		public int GetActiveSectionCount()
		{
			return activeSectionCount;
		}

		public int GetTotalSectionCount()
		{
			return sections.Length;
		}

		public SelectiveLogSection GetSectionForLog(LogEntry log)
		{
			if (config == null || !filterActive)
				return null;

			int index = GetSectionIndex(log);
			return index >= 0 ? sections[index].Source : null;
		}

		public bool MatchesSectionFilter(LogEntry log)
		{
			if (!filterActive || config == null)
				return true;

			int index = GetSectionIndex(log);
			// If log doesn't match any section, show it (don't filter out)
			if (index < 0)
				return true;

			if (!sections[index].Enabled)
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

		private int GetSectionIndex(LogEntry log)
		{
			if (log.SectionMatchVersion == MatchVersion)
				return log.SectionIndex;

			int found = -1;
			for (int i = 0; i < sections.Length; i++)
			{
				if (sections[i].Matches(log, filterMode))
				{
					found = i;
					break;
				}
			}

			log.SectionIndex = found;
			log.SectionMatchVersion = MatchVersion;
			return found;
		}

		private void ComputeSignatures(out int match, out int state)
		{
			unchecked
			{
				match = 17;
				state = 17;
				HashSections(config.SystemDefinedSections, ref match, ref state);
				HashSections(config.UserDefinedSections, ref match, ref state);
				// Never 0, which means "not computed yet".
				match |= 1;
				state |= 1;
			}
		}

		private static void HashSections(IReadOnlyList<SelectiveLogSection> list, ref int match, ref int state)
		{
			unchecked
			{
				match = match * 31 + list.Count;
				for (int i = 0; i < list.Count; i++)
				{
					SelectiveLogSection section = list[i];
					if (section == null)
					{
						match = match * 31;
						continue;
					}

					match = match * 31 + (section.name?.GetHashCode() ?? 0);
					match = match * 31 + (section.pattern?.GetHashCode() ?? 0);
					match = match * 31 + (section.useRegex ? 1 : 2);

					// Enabled state and color don't change which section matches, only what's shown.
					// The section reference is included so replaced objects (undo, reimport) are picked up.
					state = state * 31 + (section.enabled ? 1 : 2);
					state = state * 31 + section.color.GetHashCode();
					state = state * 31 + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(section);
				}
			}
		}

		private void Compile(bool matchChanged)
		{
			List<CompiledSection> compiled = new();
			AddSections(config.SystemDefinedSections, compiled);
			AddSections(config.UserDefinedSections, compiled);

			// Reuse parsed regexes when only enabled states or colors changed.
			if (!matchChanged && compiled.Count == sections.Length)
			{
				for (int i = 0; i < compiled.Count; i++)
					compiled[i].ReuseMatcher(sections[i]);
			}
			else
			{
				foreach (CompiledSection section in compiled)
					section.BuildMatcher();
			}

			sections = compiled.ToArray();

			activeSectionCount = 0;
			foreach (CompiledSection section in sections)
			{
				if (section.Enabled)
					activeSectionCount++;
			}
		}

		private static void AddSections(IReadOnlyList<SelectiveLogSection> list, List<CompiledSection> target)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null)
					target.Add(new CompiledSection(list[i]));
			}
		}

		private sealed class CompiledSection
		{
			public readonly SelectiveLogSection Source;
			public readonly bool Enabled;

			private Regex regex;
			private string plainNeedle;
			private bool plainIgnoresSpaces;

			public CompiledSection(SelectiveLogSection source)
			{
				Source = source;
				Enabled = source.enabled;
			}

			public void ReuseMatcher(CompiledSection previous)
			{
				regex = previous.regex;
				plainNeedle = previous.plainNeedle;
				plainIgnoresSpaces = previous.plainIgnoresSpaces;
			}

			public void BuildMatcher()
			{
				SectionTextMatcher.Build(Source, out regex, out plainNeedle, out plainIgnoresSpaces);
			}

			public bool Matches(LogEntry log, SLFilterMode mode)
			{
				switch (mode)
				{
					case SLFilterMode.OnlyStack:
						return MatchesText(log.StackTrace);
					case SLFilterMode.OnlyContent:
						return MatchesText(log.Message);
					default:
						return MatchesText(log.Message) || MatchesText(log.StackTrace);
				}
			}

			private bool MatchesText(string text)
			{
				return SectionTextMatcher.Matches(text, regex, plainNeedle, plainIgnoresSpaces);
			}
		}
	}
}
#endif
