#if UNITY_EDITOR
namespace EnhancedConsole.Editor.Sections
{
    /// <summary>
    /// A section with its matcher built, and its enabled state at the time it was compiled.
    /// </summary>
    internal sealed class CompiledSection
    {
        public CompiledSection(SelectiveLogSection section, SectionMatcher sectionMatcher)
        {
            Source = section;
            Enabled = section.enabled;
            Matcher = sectionMatcher;
        }

        public SelectiveLogSection Source { get; }
        public bool Enabled { get; }
        public SectionMatcher Matcher { get; }

        public bool Matches(LogEntry log, SectionFilterMode mode)
        {
            return mode switch
            {
                SectionFilterMode.OnlyStack => Matcher.Matches(log.stackTrace),
                SectionFilterMode.OnlyContent => Matcher.Matches(log.message),
                _ => Matcher.Matches(log.message) || Matcher.Matches(log.stackTrace)
            };
        }
    }
}
#endif
