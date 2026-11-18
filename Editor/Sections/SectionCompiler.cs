#if UNITY_EDITOR
using System.Collections.Generic;

namespace EnhancedConsole.Editor.Sections
{
    /// <summary>
    /// Turns a section config into compiled sections, in order, skipping empty slots.
    /// </summary>
    internal static class SectionCompiler
    {
        public static CompiledSection[] Compile(SelectiveLogsScriptableObject config)
        {
            List<SelectiveLogSection> sources = CollectSections(config);
            CompiledSection[] compiled = new CompiledSection[sources.Count];
            for (int i = 0; i < sources.Count; i++)
                compiled[i] = new CompiledSection(sources[i], SectionMatcher.Create(sources[i]));

            return compiled;
        }

        // Picks up new enabled states and colors, reusing the parsed matchers. Returns null when the sections
        // no longer line up with the previous ones, so they need compiling.
        public static CompiledSection[] Reload(SelectiveLogsScriptableObject config, CompiledSection[] previous)
        {
            List<SelectiveLogSection> sources = CollectSections(config);
            if (sources.Count != previous.Length)
                return null;

            CompiledSection[] reloaded = new CompiledSection[sources.Count];
            for (int i = 0; i < sources.Count; i++)
                reloaded[i] = new CompiledSection(sources[i], previous[i].Matcher);

            return reloaded;
        }

        private static List<SelectiveLogSection> CollectSections(SelectiveLogsScriptableObject config)
        {
            List<SelectiveLogSection> sources = new();
            AddNonNull(config.SystemDefinedSections, sources);
            AddNonNull(config.UserDefinedSections, sources);
            return sources;
        }

        private static void AddNonNull(IReadOnlyList<SelectiveLogSection> list, List<SelectiveLogSection> target)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                    target.Add(list[i]);
            }
        }
    }
}
#endif
