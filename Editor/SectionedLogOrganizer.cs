#if UNITY_EDITOR
using System;
using EnhancedConsole.Editor.Sections;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Filters logs by section. Sections are compiled once and each entry caches the index of its first matching
    /// section, so filtering doesn't re-run the matchers on every GUI event.
    /// </summary>
    internal class SectionedLogOrganizer
    {
        private SelectiveLogsScriptableObject config;
        private CompiledSection[] sections = Array.Empty<CompiledSection>();
        private int matchSignature;
        private int stateSignature;
        private int activeSectionCount;

        // Changes when which section an entry matches can change: names, patterns, order or filter mode.
        public int MatchVersion { get; private set; } = 1;

        // Changes whenever anything that affects section filtering changes. Only ever increases.
        public int Version { get; private set; } = 1;

        public bool IsFilterActive { get; private set; }
        public SectionFilterMode FilterMode { get; private set; } = SectionFilterMode.StackAndContent;

        public void Initialize(SelectiveLogsScriptableObject configuration)
        {
            if (!ReferenceEquals(config, configuration))
            {
                config = configuration;
                matchSignature = SectionSignature.NONE;
                stateSignature = SectionSignature.NONE;
            }

            RefreshSectionStates();
        }

        // Picks up edits to the config asset. Cheap enough to call every GUI event: it only hashes the section
        // fields and recompiles when something changed.
        public void RefreshSectionStates()
        {
            if (config == null)
            {
                RemoveSections();
                return;
            }

            int newMatchSignature = SectionSignature.ComputeMatchSignature(config);
            int newStateSignature = SectionSignature.ComputeStateSignature(config);
            if (newMatchSignature != matchSignature)
                RecompileSections();
            else if (newStateSignature != stateSignature)
                ReloadSectionStates();
            else
                return;

            matchSignature = newMatchSignature;
            stateSignature = newStateSignature;
            Version++;
        }

        public void SetFilterActive(bool active)
        {
            if (IsFilterActive == active)
                return;

            IsFilterActive = active;
            Version++;
        }

        public void SetFilterMode(SectionFilterMode mode)
        {
            if (FilterMode == mode)
                return;

            FilterMode = mode;
            MatchVersion++;
            Version++;
        }

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
            if (config == null || !IsFilterActive)
                return null;

            int index = GetSectionIndex(log);
            return index >= 0 ? sections[index].Source : null;
        }

        // A log that matches no section is shown.
        public bool MatchesSectionFilter(LogEntry log)
        {
            if (!IsFilterActive || config == null)
                return true;

            int index = GetSectionIndex(log);
            if (index < 0)
                return true;

            if (!sections[index].Enabled)
                return false;

            return FilterMode switch
            {
                SectionFilterMode.OnlyStack => !string.IsNullOrEmpty(log.stackTrace),
                SectionFilterMode.OnlyContent => !string.IsNullOrEmpty(log.message),
                _ => true
            };
        }

        private int GetSectionIndex(LogEntry log)
        {
            if (log.sectionMatchVersion == MatchVersion)
                return log.sectionIndex;

            log.sectionIndex = FindFirstMatchingSection(log);
            log.sectionMatchVersion = MatchVersion;
            return log.sectionIndex;
        }

        private int FindFirstMatchingSection(LogEntry log)
        {
            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i].Matches(log, FilterMode))
                    return i;
            }

            return -1;
        }

        private void RemoveSections()
        {
            if (sections.Length == 0)
                return;

            SetSections(Array.Empty<CompiledSection>());
            matchSignature = SectionSignature.NONE;
            stateSignature = SectionSignature.NONE;
            MatchVersion++;
            Version++;
        }

        private void RecompileSections()
        {
            SetSections(SectionCompiler.Compile(config));
            MatchVersion++;
        }

        // Only enabled states or colors changed, so the parsed matchers are reused.
        private void ReloadSectionStates()
        {
            CompiledSection[] reloaded = SectionCompiler.Reload(config, sections);
            if (reloaded == null)
                RecompileSections();
            else
                SetSections(reloaded);
        }

        private void SetSections(CompiledSection[] compiled)
        {
            sections = compiled;
            activeSectionCount = 0;
            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i].Enabled)
                    activeSectionCount++;
            }
        }
    }
}
#endif
